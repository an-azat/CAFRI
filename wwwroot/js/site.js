document.addEventListener("DOMContentLoaded", () => {
    // Listing filter forms (Intelligence archive, Publications) apply as soon as a select changes.
    document.querySelectorAll("[data-auto-submit-filters]").forEach((form) => {
        form.querySelectorAll("select").forEach((select) => {
            select.addEventListener("change", () => form.requestSubmit());
        });
    });

    const initLoadMore = () => {
        const button = document.querySelector("[data-load-more]");
        const list = document.querySelector("[data-load-more-list]");
        if (!button || !list) {
            return;
        }

        button.addEventListener("click", async (event) => {
            event.preventDefault();
            if (button.getAttribute("aria-busy") === "true") {
                return;
            }

            button.setAttribute("aria-busy", "true");
            const originalHtml = button.innerHTML;
            button.textContent = "Loading...";

            try {
                const response = await fetch(button.href, { headers: { "X-Requested-With": "XMLHttpRequest" } });
                if (!response.ok) {
                    throw new Error(`Request failed: ${response.status}`);
                }

                const nextPage = new DOMParser().parseFromString(await response.text(), "text/html");
                nextPage.querySelectorAll("[data-load-more-list] > *").forEach((item) => {
                    list.appendChild(document.importNode(item, true));
                });

                const count = document.querySelector("[data-load-more-count]");
                const nextCount = nextPage.querySelector("[data-load-more-count]");
                if (count && nextCount) {
                    // The next page reports how many items have been shown up to and including it,
                    // which matches the items now on screen.
                    count.textContent = nextCount.textContent;
                }

                const nextButton = nextPage.querySelector("[data-load-more]");
                if (nextButton) {
                    button.href = nextButton.getAttribute("href");
                    button.innerHTML = originalHtml;
                    button.removeAttribute("aria-busy");
                } else {
                    button.remove();
                }
            } catch {
                // Fall back to a regular navigation to the next page.
                window.location.href = button.href;
            }
        });
    };

    initLoadMore();
});
