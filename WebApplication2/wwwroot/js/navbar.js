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

document.addEventListener("DOMContentLoaded", function () {

    const profileDropdown = document.querySelector(".profile-dropdown");
    const profileButton = document.querySelector(".profile-button");

    if (!profileDropdown || !profileButton) {
        return;
    }

    profileButton.addEventListener("click", function (e) {

        e.stopPropagation();

        profileDropdown.classList.toggle("open");

    });

    document.addEventListener("click", function (e) {

        if (!profileDropdown.contains(e.target)) {

            profileDropdown.classList.remove("open");

        }

    });

});