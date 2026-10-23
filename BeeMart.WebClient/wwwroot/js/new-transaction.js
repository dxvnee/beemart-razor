(() => {
    const $ = id => document.getElementById(id);
    const rupiah = n => 'Rp ' + Number(n).toLocaleString('id-ID');
    const esc = s => {
        const d = document.createElement('div');
        d.textContent = s;
        return d.innerHTML;
    };
    const antiforgery = document.querySelector('input[name="__RequestVerificationToken"]').value;

    let cart = [];
    let searchTimer = null;

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

    function tick() {
        const now = new Date();
        const date = now.toLocaleDateString('id-ID', { timeZone: 'Asia/Jakarta', day: '2-digit', month: 'long', year: 'numeric' });
        const time = now.toLocaleTimeString('en-GB', { timeZone: 'Asia/Jakarta', hour12: false });
        $('clock').textContent = date + ', ' + time;
    }

    async function loadInvoice() {
        const r = await call('/NewTransaction?handler=NextInvoice');
        if (!r) return;
        if (r.success) $('invoice').value = r.data.invoiceNo;
        else showToast('error', r.message);
    }

    async function loadPaymentMethods() {
        const r = await call('/NewTransaction?handler=PaymentMethods');
        if (!r) return;
        const select = $('payment');
        select.innerHTML = '<option value="">-- Pilih metode --</option>';
        if (r.success && r.data) {
            r.data.forEach(pm => {
                const opt = document.createElement('option');
                opt.value = pm.idPaymentMethod;
                opt.textContent = pm.methodName;
                select.appendChild(opt);
            });
        } else {
            showToast('error', r.message || 'Gagal memuat metode pembayaran.');
        }
    }

    async function search() {
        const q = $('search').value.trim();
        const r = await call('/NewTransaction?handler=Search&q=' + encodeURIComponent(q));
        if (!r) return;
        if (!r.success) return showToast('error', r.message);
        renderResults(r.data);
    }

    function renderResults(products) {
        const body = $('search-results');
        if (!products.length) {
            body.innerHTML = '<tr><td colspan="4" class="empty">Barang tidak ditemukan.</td></tr>';
            return;
        }
        body.innerHTML = products.map(p => `
            <tr>
                <td>${esc(p.name)}</td>
                <td>${rupiah(p.price)}</td>
                <td><input type="number" min="1" max="9999" value="1" class="qty-input" id="qty-${p.idProduct}"></td>
                <td><button type="button" class="btn btn-small" data-add="${p.idProduct}">Tambah</button></td>
            </tr>`).join('');

        body.querySelectorAll('[data-add]').forEach(btn => {
            btn.addEventListener('click', () => {
                const product = products.find(p => p.idProduct === btn.dataset.add);
                addToCart(product, parseInt($('qty-' + product.idProduct).value, 10));
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

        if (existing) existing.qty = newQty;
        else cart.push({ idProduct: product.idProduct, name: product.name, price: product.price, qty: qty, stock: product.stock });
        renderCart();
    }

    function renderCart() {
        const body = $('cart-body');
        if (!cart.length) {
            body.innerHTML = '<tr><td colspan="5" class="empty">Keranjang kosong.</td></tr>';
        } else {
            body.innerHTML = cart.map((c, i) => `
                <tr>
                    <td>${esc(c.name)}</td>
                    <td>${rupiah(c.price)}</td>
                    <td><input type="number" min="1" max="9999" value="${c.qty}" class="qty-input" data-qty="${i}"></td>
                    <td>${rupiah(c.price * c.qty)}</td>
                    <td><button type="button" class="btn btn-small btn-danger" data-remove="${i}">Hapus</button></td>
                </tr>`).join('');
        }
        $('total').textContent = rupiah(cart.reduce((sum, c) => sum + c.price * c.qty, 0));
    }

    $('cart-body').addEventListener('change', e => {
        if (e.target.dataset.qty === undefined) return;
        const item = cart[Number(e.target.dataset.qty)];
        const qty = parseInt(e.target.value, 10);
        if (validQty(qty, item.stock)) item.qty = qty;
        renderCart();
    });

    $('cart-body').addEventListener('click', e => {
        if (e.target.dataset.remove === undefined) return;
        cart.splice(Number(e.target.dataset.remove), 1);
        renderCart();
    });

    $('search').addEventListener('input', () => {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(search, 300);
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
            $('payment').value = '';
            renderCart();
            loadInvoice();
            search();
        } catch {
            showToast('error', 'Gagal menyimpan transaksi. Periksa koneksi Anda.');
        } finally {
            btn.disabled = false;
        }
    });

    tick();
    setInterval(tick, 1000);
    loadInvoice();
    loadPaymentMethods();
    search();
    renderCart();
})();