function showToast(type, message) {
    const box = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = 'toast toast-' + type;
    toast.textContent = message;
    box.appendChild(toast);
    setTimeout(() => toast.remove(), 5000);
}

// Toast dari server (dikirim lewat TempData setelah redirect)
document.addEventListener('DOMContentLoaded', () => {
    const data = document.getElementById('toast-data');
    if (!data) return;
    const [type, ...rest] = data.dataset.toast.split('|');
    showToast(type, rest.join('|'));
});