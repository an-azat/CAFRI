(function () {
  const buttons = document.querySelectorAll("[data-content-action]");
  const modal = document.querySelector("[data-download-modal]");
  const modalCloseButtons = document.querySelectorAll("[data-download-close]");
  const modalConfirmButton = document.querySelector("[data-download-confirm]");
  const storagePrefix = "cafri-saved:";

  const openModal = () => {
    if (!modal) {
      return;
    }

    modal.hidden = false;
    document.body.classList.add("modal-open");
  };

  const closeModal = () => {
    if (!modal) {
      return;
    }

    modal.hidden = true;
    document.body.classList.remove("modal-open");
  };

  const getSelectedDownloadUrl = () => {
    if (!modal) {
      return "";
    }

    const selected = modal.querySelector("input[type='radio']:checked");
    return selected?.value || "";
  };

  const copyText = async (value) => {
    if (!value) {
      return false;
    }

    try {
      await navigator.clipboard.writeText(value);
      return true;
    } catch {
      return false;
    }
  };

  const updateSaveButton = (button, key) => {
    const isSaved = window.localStorage.getItem(key) === "true";
    button.classList.toggle("is-active", isSaved);
    button.textContent = isSaved ? "Saved" : "Save";
  };

  buttons.forEach((button) => {
    const action = button.getAttribute("data-content-action") || "default";
    const title = button.getAttribute("data-content-title") || document.title;
    const pageUrl = window.location.href;
    const saveKey = storagePrefix + pageUrl;

    if (action === "save") {
      updateSaveButton(button, saveKey);
    }

    button.addEventListener("click", async () => {
      if (action === "download") {
        openModal();
        return;
      }

      if (action === "print") {
        window.print();
        return;
      }

      if (action === "share") {
        if (navigator.share) {
          try {
            await navigator.share({ title, url: pageUrl });
            return;
          } catch {
          }
        }

        const copied = await copyText(pageUrl);
        if (copied) {
          const originalLabel = button.textContent;
          button.textContent = "Link Copied";
          window.setTimeout(() => {
            button.textContent = originalLabel;
          }, 1400);
        }

        return;
      }

      if (action === "cite") {
        const citation = `${title} — CAFRI — ${pageUrl}`;
        const copied = await copyText(citation);
        if (copied) {
          const originalLabel = button.textContent;
          button.textContent = "Copied";
          window.setTimeout(() => {
            button.textContent = originalLabel;
          }, 1400);
        }

        return;
      }

      if (action === "save") {
        const isSaved = window.localStorage.getItem(saveKey) === "true";
        window.localStorage.setItem(saveKey, isSaved ? "false" : "true");
        updateSaveButton(button, saveKey);
      }
    });
  });

  modalCloseButtons.forEach((button) => {
    button.addEventListener("click", closeModal);
  });

  modalConfirmButton?.addEventListener("click", () => {
    const url = getSelectedDownloadUrl();
    if (!url) {
      return;
    }

    window.open(url, "_blank", "noopener,noreferrer");
    closeModal();
  });

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
      closeModal();
    }
  });
})();
