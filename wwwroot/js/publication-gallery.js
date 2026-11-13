(function () {
  const gallery = document.querySelector(".photo-gallery");
  if (!gallery) {
    return;
  }

  const featuredVisual = gallery.querySelector(".photo-gallery__featured-visual");
  const featuredImage = gallery.querySelector(".photo-gallery__featured-image");
  const featuredTitle = gallery.querySelector(".photo-gallery__featured-copy h3");
  const featuredCaption = gallery.querySelector(".photo-gallery__featured-copy p");
  const lightbox = gallery.querySelector("[data-gallery-lightbox]");
  const lightboxVisual = gallery.querySelector("[data-gallery-lightbox-visual]");
  const lightboxImage = gallery.querySelector("[data-gallery-lightbox-image]");
  const lightboxTitle = gallery.querySelector("[data-gallery-lightbox-title]");
  const lightboxCaption = gallery.querySelector("[data-gallery-lightbox-caption]");

  const allVisualClasses = Array.from(gallery.querySelectorAll("[data-gallery-select]"))
    .map((item) => item.getAttribute("data-gallery-select"))
    .filter(Boolean);

  const setVisualClass = (element, className) => {
    allVisualClasses.forEach((item) => element.classList.remove(item));
    if (className) {
      element.classList.add(className);
    }
  };

  const setImageState = (imageElement, visualElement, imageUrl, className, altText) => {
    if (imageElement instanceof HTMLImageElement && imageUrl) {
      imageElement.src = imageUrl;
      imageElement.alt = altText || "";
      imageElement.hidden = false;
      if (visualElement instanceof HTMLElement) {
        setVisualClass(visualElement, className);
        visualElement.hidden = true;
      }
      return;
    }

    if (imageElement instanceof HTMLImageElement) {
      imageElement.hidden = true;
    }

    if (visualElement instanceof HTMLElement) {
      visualElement.hidden = false;
      setVisualClass(visualElement, className);
    }
  };

  gallery.querySelectorAll("[data-gallery-select]").forEach((button) => {
    button.addEventListener("click", () => {
      const className = button.getAttribute("data-gallery-class") || "";
      const imageUrl = button.getAttribute("data-gallery-image") || "";
      const title = button.getAttribute("data-gallery-title");
      const caption = button.getAttribute("data-gallery-caption");
      const altText = button.querySelector("img")?.getAttribute("alt") || title || "";

      setImageState(featuredImage, featuredVisual, imageUrl, className, altText);
      if (featuredTitle) {
        featuredTitle.textContent = title || "";
      }

      if (featuredCaption) {
        featuredCaption.textContent = caption || "";
      }
    });
  });

  gallery.querySelectorAll("[data-gallery-open]").forEach((button) => {
    button.addEventListener("click", () => {
      const className = button.getAttribute("data-gallery-class") || "";
      const imageUrl = button.getAttribute("data-gallery-image") || featuredImage?.getAttribute("src") || "";
      const title = button.getAttribute("data-gallery-title") || featuredTitle?.textContent || "";
      const caption = button.getAttribute("data-gallery-caption") || featuredCaption?.textContent || "";
      const altText = button.querySelector("img")?.getAttribute("alt") || title || "";

      setImageState(lightboxImage, lightboxVisual, imageUrl, className, altText);
      if (lightboxTitle) {
        lightboxTitle.textContent = title;
      }

      if (lightboxCaption) {
        lightboxCaption.textContent = caption;
      }

      if (lightbox instanceof HTMLElement) {
        lightbox.hidden = false;
        document.body.classList.add("is-modal-open");
      }
    });
  });

  gallery.querySelector("[data-gallery-close]")?.addEventListener("click", () => {
    if (lightbox instanceof HTMLElement) {
      lightbox.hidden = true;
      document.body.classList.remove("is-modal-open");
    }
  });

  lightbox?.addEventListener("click", (event) => {
    if (event.target === lightbox && lightbox instanceof HTMLElement) {
      lightbox.hidden = true;
      document.body.classList.remove("is-modal-open");
    }
  });

  // Thumbnail strip: prev/next buttons appear only when the strip overflows.
  const thumbs = gallery.querySelector("[data-gallery-thumbs]");
  const navButtons = Array.from(gallery.querySelectorAll("[data-gallery-scroll]"));
  if (thumbs instanceof HTMLElement && navButtons.length > 0) {
    const updateNav = () => {
      const maxScroll = thumbs.scrollWidth - thumbs.clientWidth;
      const overflows = maxScroll > 1;
      navButtons.forEach((button) => {
        button.hidden = !overflows;
        const direction = Number(button.getAttribute("data-gallery-scroll"));
        button.disabled = direction < 0 ? thumbs.scrollLeft <= 1 : thumbs.scrollLeft >= maxScroll - 1;
      });
    };

    navButtons.forEach((button) => {
      button.addEventListener("click", () => {
        const direction = Number(button.getAttribute("data-gallery-scroll"));
        thumbs.scrollBy({ left: direction * thumbs.clientWidth * 0.8, behavior: "smooth" });
      });
    });

    thumbs.addEventListener("scroll", updateNav, { passive: true });
    window.addEventListener("resize", updateNav);
    updateNav();
  }
})();
