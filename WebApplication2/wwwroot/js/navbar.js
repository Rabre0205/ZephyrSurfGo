document.addEventListener("DOMContentLoaded", () => {
    const profileDropdown = document.querySelector(".profile-dropdown");
    const profileButton = document.querySelector(".profile-button");

    if (profileDropdown && profileButton) {
        profileButton.addEventListener("click", event => {
            event.stopPropagation();
            const open = profileDropdown.classList.toggle("open");
            profileButton.setAttribute("aria-expanded", open.toString());
        });

        document.addEventListener("click", event => {
            if (profileDropdown.contains(event.target)) return;
            profileDropdown.classList.remove("open");
            profileButton.setAttribute("aria-expanded", "false");
        });
    }

    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll("#navbar .nav-links a").forEach(link => {
        const target = new URL(link.href, window.location.origin).pathname.toLowerCase();
        if (target !== "/" && currentPath.startsWith(target)) {
            link.classList.add("active");
            link.setAttribute("aria-current", "page");
        }
    });
});
