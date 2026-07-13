document.addEventListener("DOMContentLoaded", () => {
    const initModal = (modalSelector, openSelector, closeSelector) => {
        const modal = document.querySelector(modalSelector);
        if (!modal) {
            return;
        }

        const openButtons = document.querySelectorAll(openSelector);
        const closeButtons = modal.querySelectorAll(closeSelector);

        const openModal = () => {
            modal.hidden = false;
            document.body.classList.add("is-modal-open");
        };

        const closeModal = () => {
            modal.hidden = true;
            document.body.classList.remove("is-modal-open");
        };

        openButtons.forEach((button) => {
            button.addEventListener("click", openModal);
        });

        closeButtons.forEach((button) => {
            button.addEventListener("click", closeModal);
        });

        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape" && !modal.hidden) {
                closeModal();
            }
        });
    };

    initModal("[data-intelligence-filter-modal]", "[data-intelligence-filter-open]", "[data-intelligence-filter-close]");
    initModal("[data-publications-filter-modal]", "[data-publications-filter-open]", "[data-publications-filter-close]");
});
