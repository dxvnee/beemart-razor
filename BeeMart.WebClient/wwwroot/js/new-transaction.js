(() => {
    const $ = id => document.getElementById(id);
    const rupiah = n => 'Rp ' + Number(n).toLocaleString('id-ID');
    const pct = n => Number(n) + '%';
    const esc = s => {
        const d = document.createElement('div');
        d.textContent = s;
        return d.innerHTML;
    };
    const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
    const antiforgery = tokenEl ? tokenEl.value : '';

    let cart = [];

    // Live Header Clock
    function updateClock() {
        const clockEl = $('header-clock');
        if (!clockEl) return;
        const now = new Date();
        const pad = n => String(n).padStart(2, '0');
        const day = now.getDate();
        const month = now.getMonth() + 1;
        const year = now.getFullYear();
        const h = pad(now.getHours());
        const m = pad(now.getMinutes());
        const s = pad(now.getSeconds());
        clockEl.textContent = `${day}/${month}/${year}, ${h}.${m}.${s}`;
    }
    updateClock();
    setInterval(updateClock, 1000);

    async function call(url, options) {
        const res = await fetch(url, options);
        if (res.status === 401) {
            window.location.href = '/Login';
            return null;
        }
        const data = await res.json();
        if (data.statusCode === 401) {
            window.location.href = '/Login';
            return null;
        }
        return data;
    }

    async function loadInvoice() {
        const r = await call('/NewTransaction?handler=NextInvoice');
        if (!r) return;
        if (r.success) $('invoice').textContent = r.data.invoiceNo;
        else showToast('error', r.message);
    }

    async function loadPaymentMethods() {
        const r = await call('/NewTransaction?handler=PaymentMethods');
        if (!r) return;
        const select = $('payment');
        select.innerHTML = '<option value="">-- Pilih metode --</option>';
        if (r.success && r.data) {
            let cashId = null;
            r.data.forEach(pm => {
                const opt = document.createElement('option');
                opt.value = pm.idPaymentMethod;
                opt.textContent = pm.methodName;
                if (pm.methodName.toLowerCase() === 'cash') {
                    cashId = pm.idPaymentMethod;
                }
                select.appendChild(opt);
            });
            if (cashId !== null) {
                select.value = cashId;
            }
        } else {
            showToast('error', r.message || 'Gagal memuat metode pembayaran.');
        }
    }

    async function search() {
        const q = $('search').value.trim();
        if (!q) {
            $('search-results-section').style.display = 'none';
            $('search-results').innerHTML = '';
            return;
        }
        const r = await call('/NewTransaction?handler=Search&q=' + encodeURIComponent(q));
        if (!r) return;
        if (!r.success) return showToast('error', r.message);
        renderResults(r.data);
    }

    function renderResults(products) {
        const section = $('search-results-section');
        const body = $('search-results');
        if (!products.length) {
            section.style.display = 'none';
            body.innerHTML = '';
            showToast('info', 'Barang tidak ditemukan.');
            return;
        }
        section.style.display = 'block';
        body.innerHTML = products.map(p => `
            <tr>
                <td>${esc(p.name)}</td>
                <td>${p.weight} Kg</td>
                <td>${p.stock}</td>
                <td>${rupiah(p.price)}</td>
                <td>${pct(p.discount)}</td>
                <td><button type="button" class="btn btn-small btn-select" data-add="${p.idProduct}">Pilih</button></td>
            </tr>`).join('');

        body.querySelectorAll('[data-add]').forEach(btn => {
            btn.addEventListener('click', () => {
                const product = products.find(p => p.idProduct === btn.dataset.add);
                addToCart(product, 1);
            });
        });
    }

    function validQty(qty, stock) {
        if (!Number.isInteger(qty) || qty < 1 || qty > 9999) {
            showToast('error', 'Quantity harus berupa angka bulat 1 sampai 9999.');
            return false;
        }
        if (qty > stock) {
            showToast('error', 'Stok tidak mencukupi (tersisa ' + stock + ').');
            return false;
        }
        return true;
    }

    function addToCart(product, qty) {
        const existing = cart.find(c => c.idProduct === product.idProduct);
        const newQty = (existing ? existing.qty : 0) + qty;
        if (!validQty(qty, product.stock) || !validQty(newQty, product.stock)) return;

        if (existing) {
            existing.qty = newQty;
        } else {
            cart.push({
                idProduct: product.idProduct,
                name: product.name,
                price: product.price,
                discount: product.discount,
                qty: qty,
                stock: product.stock
            });
        }
        renderCart();
    }

    function renderCart() {
        const body = $('cart-body');
        if (!cart.length) {
            body.innerHTML = '';
        } else {
            body.innerHTML = cart.map((c, i) => {
                const gross = c.price * c.qty;
                const discNominal = gross * (c.discount / 100);
                const sub = gross - discNominal;
                return `
                <tr>
                    <td>${esc(c.name)}</td>
                    <td>${rupiah(c.price)}</td>
                    <td>${pct(c.discount)}</td>
                    <td>${c.qty}</td>
                    <td>${rupiah(sub)}</td>
                    <td><button type="button" class="btn btn-small btn-danger" data-remove="${i}">Hapus</button></td>
                </tr>`;
            }).join('');
        }

        const total = cart.reduce((sum, c) => {
            const gross = c.price * c.qty;
            const discNominal = gross * (c.discount / 100);
            return sum + (gross - discNominal);
        }, 0);
        $('total').textContent = rupiah(total);
    }

    $('cart-body').addEventListener('click', e => {
        if (e.target.dataset.remove === undefined) return;
        cart.splice(Number(e.target.dataset.remove), 1);
        renderCart();
    });

    $('search').addEventListener('keydown', e => {
        if (e.key === 'Enter') {
            e.preventDefault();
            search();
        }
    });

    $('btn-save').addEventListener('click', async () => {
        if (!cart.length) return showToast('error', 'Keranjang masih kosong.');
        const payment = $('payment').value;
        if (!payment) return showToast('error', 'Pilih metode pembayaran terlebih dahulu.');

        const btn = $('btn-save');
        btn.disabled = true;
        try {
            const r = await call('/NewTransaction?handler=Save', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': antiforgery },
                body: JSON.stringify({
                    idPaymentMethod: parseInt(payment, 10),
                    items: cart.map(c => ({ idProduct: c.idProduct, qty: c.qty }))
                })
            });
            if (!r) return;
            if (!r.success) {
                const details = r.errors ? Object.values(r.errors).flat().join(' ') : '';
                return showToast('error', (r.message + ' ' + details).trim());
            }

            showToast('success', 'Transaksi ' + r.data.invoiceNo + ' berhasil. Total ' + rupiah(r.data.totalPrice));
            cart = [];
            renderCart();
            loadInvoice();
            $('search').value = '';
            $('search-results-section').style.display = 'none';
        } catch {
            showToast('error', 'Gagal menyimpan transaksi. Periksa koneksi Anda.');
        } finally {
            btn.disabled = false;
        }
    });

    loadInvoice();
    loadPaymentMethods();
    renderCart();
})();