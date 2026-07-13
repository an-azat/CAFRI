(function () {
  const cards = document.querySelectorAll("[data-publication-card]");
  if (cards.length === 0) {
    return;
  }

  const navigate = (card) => {
    const href = card.getAttribute("data-href");
    if (!href) {
      return;
    }

    window.location.href = href;
  };

  cards.forEach((card) => {
    card.addEventListener("click", (event) => {
      if (event.target instanceof HTMLElement && event.target.closest("a, button")) {
        return;
      }

      navigate(card);
    });

    card.addEventListener("keydown", (event) => {
      if (event.key === "Enter" || event.key === " ") {
        event.preventDefault();
        navigate(card);
      }
    });
  });
})();
