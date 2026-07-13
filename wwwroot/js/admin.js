document.addEventListener("DOMContentLoaded", () => {
  const toggle = document.querySelector("[data-admin-sidebar-toggle]");
  const sidebar = document.querySelector("[data-admin-sidebar]");

  if (toggle && sidebar) {
    toggle.addEventListener("click", () => {
      sidebar.classList.toggle("is-open");
      toggle.setAttribute("aria-expanded", sidebar.classList.contains("is-open") ? "true" : "false");
    });
  }

  const previewForms = document.querySelectorAll("[data-live-preview-form]");
  previewForms.forEach((form) => {
    form.addEventListener("input", () => {
      const mappings = form.querySelectorAll("[data-preview-target]");
      mappings.forEach((field) => {
        const inputName = field.getAttribute("data-preview-target");
        const previewKey = field.getAttribute("data-preview-key");
        if (!inputName || !previewKey) {
          return;
        }

        const input = form.querySelector(`[name="${inputName}"]`);
        const preview = document.querySelector(`[data-preview="${previewKey}"]`);
        if (!input || !preview) {
          return;
        }

        const value = "value" in input ? input.value : "";
        preview.textContent = value.trim() || preview.getAttribute("data-preview-fallback") || "Not set";
      });
    });
  });
});
