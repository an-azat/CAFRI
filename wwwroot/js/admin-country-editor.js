// Country profile editor (Areas/Admin/Views/Countries/Edit.cshtml).
(function () {
  const form = document.querySelector("[data-country-editor]");
  if (!(form instanceof HTMLFormElement)) {
    return;
  }

  // ---- Repeating rows -------------------------------------------------------------------
  // ASP.NET binds lists only while indexes are contiguous, so a removed row is cleared and
  // hidden (blank rows are dropped on save) instead of being taken out of the DOM.
  const visibleItems = (container) =>
    [...container.querySelectorAll(":scope > [data-repeat-item]")].filter((item) => !item.hidden);

  const clearFields = (item) => {
    item.querySelectorAll("input, textarea").forEach((field) => {
      field.value = "";
    });
    item.querySelectorAll("select").forEach((field) => {
      field.selectedIndex = 0;
    });
  };

  const renumber = (container) => {
    visibleItems(container).forEach((item, index) => {
      item.querySelectorAll("[data-repeat-number]").forEach((label) => {
        label.textContent = label.textContent.length === 2 ? String(index + 1).padStart(2, "0") : String(index + 1);
      });
    });
  };

  const reindex = (item, index) => {
    item.querySelectorAll("[name], [id], label[for], [data-valmsg-for]").forEach((element) => {
      ["name", "data-valmsg-for"].forEach((attribute) => {
        const value = element.getAttribute(attribute);
        if (value) {
          element.setAttribute(attribute, value.replace(/\[\d+\]/, `[${index}]`));
        }
      });
      ["id", "for"].forEach((attribute) => {
        const value = element.getAttribute(attribute);
        if (value) {
          element.setAttribute(attribute, value.replace(/_\d+__/, `_${index}__`));
        }
      });
    });
  };

  form.querySelectorAll("[data-repeat-add]").forEach((button) => {
    button.addEventListener("click", () => {
      const container = document.getElementById(button.getAttribute("data-repeat-add") || "");
      if (!container) {
        return;
      }

      const allItems = container.querySelectorAll(":scope > [data-repeat-item]");
      const template = allItems[allItems.length - 1];
      if (!template) {
        return;
      }

      const clone = template.cloneNode(true);
      clone.hidden = false;
      clearFields(clone);
      reindex(clone, allItems.length);

      if (button.hasAttribute("data-repeat-year")) {
        const years = [...container.querySelectorAll("[data-year]")].map((input) => Number(input.value) || 0);
        const yearInput = clone.querySelector("[data-year]");
        if (yearInput instanceof HTMLInputElement) {
          yearInput.value = String(years.length && Math.max(...years) > 0 ? Math.max(...years) + 1 : new Date().getFullYear());
        }
      }

      container.appendChild(clone);
      renumber(container);
      clone.querySelector("input, textarea")?.focus();
    });
  });

  form.addEventListener("click", (event) => {
    const button = event.target instanceof Element ? event.target.closest("[data-repeat-remove]") : null;
    if (!button) {
      return;
    }

    const item = button.closest("[data-repeat-item]");
    const container = item?.parentElement;
    if (!item || !container) {
      return;
    }

    clearFields(item);
    item.hidden = true;
    renumber(container);
  });

  // ---- Gallery tiles ------------------------------------------------------------------
  // Tiles are serialized into the hidden HeroGalleryText field as "url | alt | title | caption".
  const gallery = form.querySelector("[data-gallery-editor]");
  const raw = gallery?.querySelector("[data-gallery-raw]");
  const list = gallery?.querySelector("[data-gallery-list]");
  const count = gallery?.querySelector("[data-gallery-count]");

  const syncGallery = () => {
    if (!(raw instanceof HTMLTextAreaElement) || !list) {
      return;
    }

    const tiles = [...list.querySelectorAll("[data-gallery-item]")];
    raw.value = tiles
      .map((tile) => {
        const url = tile.querySelector("[data-gallery-url]")?.value.trim() || "";
        const title = tile.querySelector("[data-gallery-title]")?.value.trim() || "";
        const alt = (tile.getAttribute("data-alt") || "").trim();
        const caption = (tile.getAttribute("data-caption") || "").trim();
        return url ? `${url} | ${alt} | ${title} | ${caption}` : "";
      })
      .filter(Boolean)
      .join("\n");

    if (count) {
      const filled = tiles.filter((tile) => tile.querySelector("[data-gallery-url]")?.value.trim()).length;
      count.textContent = `${filled} image${filled === 1 ? "" : "s"}`;
    }
  };

  const createTile = () => {
    const tile = document.createElement("div");
    tile.className = "cpe-photo";
    tile.setAttribute("data-gallery-item", "");
    tile.innerHTML =
      '<button type="button" class="cpe-photo__remove" data-gallery-remove aria-label="Remove image">×</button>' +
      '<img src="" alt="" />' +
      '<div class="cpe-photo__fields">' +
      '<input aria-label="Image URL" placeholder="Image URL" data-gallery-url />' +
      '<input aria-label="Image caption" placeholder="Image caption" data-gallery-title />' +
      "</div>";
    return tile;
  };

  gallery?.querySelector("[data-gallery-add]")?.addEventListener("click", () => {
    const tile = createTile();
    list?.appendChild(tile);
    tile.querySelector("[data-gallery-url]")?.focus();
    syncGallery();
  });

  list?.addEventListener("click", (event) => {
    const button = event.target instanceof Element ? event.target.closest("[data-gallery-remove]") : null;
    if (button) {
      button.closest("[data-gallery-item]")?.remove();
      syncGallery();
    }
  });

  list?.addEventListener("input", (event) => {
    const input = event.target;
    if (input instanceof HTMLInputElement && input.hasAttribute("data-gallery-url")) {
      const image = input.closest("[data-gallery-item]")?.querySelector("img");
      if (image) {
        image.src = input.value.trim();
      }
    }
    syncGallery();
  });

  // ---- Flag preview -------------------------------------------------------------------
  const flagPreviews = form.querySelectorAll("[data-flag-preview]");
  const setFlag = (src) => flagPreviews.forEach((image) => {
    if (src) {
      image.src = src;
    }
  });

  form.querySelector("[data-flag-file]")?.addEventListener("change", (event) => {
    const file = event.target instanceof HTMLInputElement ? event.target.files?.[0] : null;
    if (file) {
      setFlag(URL.createObjectURL(file));
    }
  });

  form.querySelector("[data-flag-url]")?.addEventListener("input", (event) => {
    if (event.target instanceof HTMLInputElement) {
      setFlag(event.target.value.trim());
    }
  });

  form.addEventListener("submit", syncGallery);
  syncGallery();
})();
