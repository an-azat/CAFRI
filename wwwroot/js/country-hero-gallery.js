(function () {
  const mainImage = document.querySelector("[data-country-hero-main]");
  if (!(mainImage instanceof HTMLImageElement)) {
    return;
  }

  const title = document.querySelector("[data-country-hero-title]");
  const caption = document.querySelector("[data-country-hero-caption]");

  document.querySelectorAll("[data-country-hero-thumb]").forEach((button) => {
    button.addEventListener("click", () => {
      const imageUrl = button.getAttribute("data-image-url") || "";
      const imageAlt = button.getAttribute("data-image-alt") || "";
      const imageTitle = button.getAttribute("data-image-title") || "";
      const imageCaption = button.getAttribute("data-image-caption") || "";

      if (imageUrl) {
        mainImage.src = imageUrl;
      }

      mainImage.alt = imageAlt;

      if (title) {
        title.textContent = imageTitle;
      }

      if (caption) {
        caption.textContent = imageCaption;
      }
    });
  });
})();
