function getCart() {
    try {
        return JSON.parse(localStorage.getItem("master_cart")) || [];
    } catch {
        return [];
    }
}

function updateBadge() {
    const cart = getCart();
    const total = cart.reduce((s, i) => s + i.qty, 0);

    document.querySelectorAll(".cart-badge").forEach(el => {
        el.textContent = total;
        el.style.display = total > 0 ? "flex" : "none";
    });
}

document.addEventListener("DOMContentLoaded", updateBadge);