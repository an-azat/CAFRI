(function () {
  const links = Array.from(document.querySelectorAll("[data-section-link]"));
  const sections = Array.from(document.querySelectorAll("[data-section]"));
  if (links.length === 0 || sections.length === 0) {
    return;
  }

  const activate = (id) => {
    links.forEach((link) => {
      link.classList.toggle("is-active", link.getAttribute("data-target") === id);
    });
  };

  const observer = new window.IntersectionObserver(
    (entries) => {
      const visible = entries
        .filter((entry) => entry.isIntersecting)
        .sort((left, right) => right.intersectionRatio - left.intersectionRatio)[0];

      if (visible?.target instanceof HTMLElement) {
        activate(visible.target.id);
      }
    },
    {
      rootMargin: "-20% 0px -55% 0px",
      threshold: [0.2, 0.45, 0.7]
    }
  );

  sections.forEach((section) => observer.observe(section));

  links.forEach((link) => {
    link.addEventListener("click", () => {
      const id = link.getAttribute("data-target");
      if (id) {
        activate(id);
      }
    });
  });
})();
