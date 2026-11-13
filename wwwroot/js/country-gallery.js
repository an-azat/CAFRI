// Country profile gallery: horizontal scroll track with previous/next buttons.
(function () {
  document.querySelectorAll("[data-cp-gallery]").forEach((gallery) => {
    const track = gallery.querySelector("[data-cp-gallery-track]");
    if (!(track instanceof HTMLElement)) {
      return;
    }

    const scrollByItem = (direction) => {
      const item = track.querySelector(".cp-gallery__item");
      const step = item instanceof HTMLElement ? item.getBoundingClientRect().width + 12 : track.clientWidth * 0.8;
      track.scrollBy({ left: direction * step, behavior: "smooth" });
    };

    gallery.querySelector("[data-cp-gallery-prev]")?.addEventListener("click", () => scrollByItem(-1));
    gallery.querySelector("[data-cp-gallery-next]")?.addEventListener("click", () => scrollByItem(1));
  });
})();
