(function () {
  const escapeHtml = (value) =>
    String(value ?? "")
      .replace(/&/g, "&amp;")
      .replace(/"/g, "&quot;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;");

  const parseGalleryRows = (raw) => {
    if (!raw || !raw.trim()) {
      return [];
    }

    return raw
      .split(/\r?\n/)
      .map((line) => line.trim())
      .filter(Boolean)
      .map((line) => {
        const parts = line.split("|").map((part) => part.trim());
        return {
          imageUrl: parts[0] || "",
          alt: parts[1] || "",
          title: parts[2] || "",
          caption: parts.slice(3).join(" | ") || ""
        };
      });
  };

  const serializeGalleryRows = (list, rawField) => {
    const rows = Array.from(list.querySelectorAll("[data-media-gallery-row]"))
      .map((row) => {
        const imageUrl = row.querySelector("[data-media-gallery-field='imageUrl']")?.value?.trim() || "";
        const alt = row.querySelector("[data-media-gallery-field='alt']")?.value?.trim() || "";
        const title = row.querySelector("[data-media-gallery-field='title']")?.value?.trim() || "";
        const caption = row.querySelector("[data-media-gallery-field='caption']")?.value?.trim() || "";
        if (!imageUrl && !alt && !title && !caption) {
          return null;
        }

        return `${imageUrl} | ${alt} | ${title} | ${caption}`;
      })
      .filter(Boolean);

    rawField.value = rows.join("\n");
  };

  const syncRowPreview = (row) => {
    const imageUrl = row.querySelector("[data-media-gallery-field='imageUrl']")?.value?.trim() || "";
    const title = row.querySelector("[data-media-gallery-field='title']")?.value?.trim() || "Untitled image";
    const caption = row.querySelector("[data-media-gallery-field='caption']")?.value?.trim() || "No caption yet.";
    const image = row.querySelector("[data-media-gallery-preview-image]");
    const placeholder = row.querySelector("[data-media-gallery-preview-placeholder]");
    const previewTitle = row.querySelector("[data-media-gallery-preview-title]");
    const previewCaption = row.querySelector("[data-media-gallery-preview-caption]");

    if (previewTitle) {
      previewTitle.textContent = title;
    }

    if (previewCaption) {
      previewCaption.textContent = caption;
    }

    if (image instanceof HTMLImageElement) {
      if (imageUrl) {
        image.src = imageUrl;
        image.hidden = false;
        if (placeholder instanceof HTMLElement) {
          placeholder.hidden = true;
        }
      } else {
        image.removeAttribute("src");
        image.hidden = true;
        if (placeholder instanceof HTMLElement) {
          placeholder.hidden = false;
        }
      }
    }
  };

  const buildGalleryRow = (item, list, rawField) => {
    const row = document.createElement("div");
    row.className = "admin-media-gallery-row";
    row.setAttribute("data-media-gallery-row", "");
    row.setAttribute("draggable", "true");
    row.innerHTML = `
      <div class="admin-media-gallery-row__preview">
        <img data-media-gallery-preview-image alt="${escapeHtml(item.alt || item.title || "Gallery image")}" ${item.imageUrl ? `src="${escapeHtml(item.imageUrl)}"` : "hidden"} />
        <div class="admin-media-gallery-row__placeholder" data-media-gallery-preview-placeholder ${item.imageUrl ? "hidden" : ""}>No image</div>
        <div class="admin-media-gallery-row__meta">
          <strong data-media-gallery-preview-title>${escapeHtml(item.title || "Untitled image")}</strong>
          <p data-media-gallery-preview-caption>${escapeHtml(item.caption || "No caption yet.")}</p>
        </div>
      </div>
      <div class="admin-media-gallery-row__grid">
        <div class="admin-media-gallery-row__full">
          <label class="form-label">Image URL</label>
          <input type="text" class="form-control" data-media-gallery-field="imageUrl" value="${escapeHtml(item.imageUrl)}">
        </div>
        <div>
          <label class="form-label">Alt</label>
          <input type="text" class="form-control" data-media-gallery-field="alt" value="${escapeHtml(item.alt)}">
        </div>
        <div>
          <label class="form-label">Title</label>
          <input type="text" class="form-control" data-media-gallery-field="title" value="${escapeHtml(item.title)}">
        </div>
        <div class="admin-media-gallery-row__full">
          <label class="form-label">Caption</label>
          <textarea class="form-control" rows="3" data-media-gallery-field="caption">${escapeHtml(item.caption)}</textarea>
        </div>
      </div>
      <div class="admin-media-gallery-row__actions">
        <span class="admin-media-gallery-row__drag">Drag to reorder</span>
        <button type="button" class="btn btn-sm btn-outline-danger" data-media-gallery-remove>Remove</button>
      </div>
    `;

    row.querySelectorAll("input, textarea").forEach((input) => {
      input.addEventListener("input", () => {
        syncRowPreview(row);
        serializeGalleryRows(list, rawField);
      });
    });

    row.querySelector("[data-media-gallery-remove]")?.addEventListener("click", () => {
      row.remove();
      serializeGalleryRows(list, rawField);
    });

    row.addEventListener("dragstart", () => {
      row.classList.add("is-dragging");
    });

    row.addEventListener("dragend", () => {
      row.classList.remove("is-dragging");
      serializeGalleryRows(list, rawField);
    });

    syncRowPreview(row);
    return row;
  };

  document.querySelectorAll("[data-media-gallery-editor]").forEach((editor) => {
    const rawField = editor.querySelector("[data-media-gallery-raw]");
    const list = editor.querySelector("[data-media-gallery-list]");
    const addButton = editor.querySelector("[data-media-gallery-add]");

    if (!(rawField instanceof HTMLTextAreaElement) || !(list instanceof HTMLElement) || !(addButton instanceof HTMLElement)) {
      return;
    }

    const initialRows = parseGalleryRows(rawField.value);
    if (initialRows.length === 0) {
      list.appendChild(buildGalleryRow({ imageUrl: "", alt: "", title: "", caption: "" }, list, rawField));
    } else {
      initialRows.forEach((item) => list.appendChild(buildGalleryRow(item, list, rawField)));
    }

    addButton.addEventListener("click", () => {
      list.appendChild(buildGalleryRow({ imageUrl: "", alt: "", title: "", caption: "" }, list, rawField));
      serializeGalleryRows(list, rawField);
    });

    list.addEventListener("dragover", (event) => {
      event.preventDefault();
      const draggingRow = list.querySelector(".admin-media-gallery-row.is-dragging");
      if (!(draggingRow instanceof HTMLElement)) {
        return;
      }

      const siblings = Array.from(list.querySelectorAll(".admin-media-gallery-row:not(.is-dragging)"));
      const nextSibling = siblings.find((sibling) => {
        const rect = sibling.getBoundingClientRect();
        return event.clientY < rect.top + rect.height / 2;
      });

      if (nextSibling instanceof HTMLElement) {
        list.insertBefore(draggingRow, nextSibling);
      } else {
        list.appendChild(draggingRow);
      }
    });

    rawField.form?.addEventListener("submit", () => {
      serializeGalleryRows(list, rawField);
    });

    serializeGalleryRows(list, rawField);
  });

  document.querySelectorAll("[data-media-hero-editor]").forEach((editor) => {
    const urlInput = editor.querySelector("[data-media-hero-url]");
    const fileInput = editor.querySelector("[data-media-hero-file]");
    const image = editor.querySelector("[data-media-hero-preview-image]");
    const placeholder = editor.querySelector("[data-media-hero-preview-placeholder]");
    const label = editor.querySelector("[data-media-hero-preview-label]");

    if (!(image instanceof HTMLImageElement) || !(placeholder instanceof HTMLElement)) {
      return;
    }

    const syncUrlPreview = () => {
      const url = urlInput instanceof HTMLInputElement ? urlInput.value.trim() : "";
      if (!url) {
        image.removeAttribute("src");
        image.hidden = true;
        placeholder.hidden = false;
        if (label) {
          label.textContent = "No hero image uploaded yet";
        }
        return;
      }

      image.src = url;
      image.hidden = false;
      placeholder.hidden = true;
      if (label) {
        label.textContent = "Current hero image";
      }
    };

    const syncFilePreview = () => {
      if (!(fileInput instanceof HTMLInputElement) || !fileInput.files || fileInput.files.length === 0) {
        syncUrlPreview();
        return;
      }

      const [file] = fileInput.files;
      const reader = new FileReader();
      reader.onload = () => {
        image.src = String(reader.result || "");
        image.hidden = false;
        placeholder.hidden = true;
        if (label) {
          label.textContent = `Selected file: ${file.name}`;
        }
      };
      reader.readAsDataURL(file);
    };

    if (urlInput instanceof HTMLInputElement) {
      urlInput.addEventListener("input", syncUrlPreview);
    }

    if (fileInput instanceof HTMLInputElement) {
      fileInput.addEventListener("change", syncFilePreview);
    }

    syncUrlPreview();
  });
})();
