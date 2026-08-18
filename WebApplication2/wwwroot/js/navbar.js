function getCart() {
    try {
        return JSON.parse(localStorage.getItem("master_cart")) || [];
    } catch {
        return [];
    }
}

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

    document.addEventListener("DOMContentLoaded", () => {
        const profileButton = document.querySelector(".profile-button");
        const profileDropdown = document.querySelector(".profile-dropdown");

        if (!profileButton || !profileDropdown) {
            return;
        }

        profileButton.addEventListener("click", (event) => {
            event.stopPropagation();

            const estaAbierto =
                profileDropdown.classList.toggle("open");

            profileButton.setAttribute(
                "aria-expanded",
                estaAbierto.toString()
            );
        });

        document.addEventListener("click", () => {
            profileDropdown.classList.remove("open");
            profileButton.setAttribute("aria-expanded", "false");
        });
    });
});
