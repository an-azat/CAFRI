(function () {
  const copyButtons = document.querySelectorAll("[data-copy-text]");
  if (!copyButtons.length) {
    return;
  }

  async function copyValue(text) {
    if (navigator.clipboard && navigator.clipboard.writeText) {
      await navigator.clipboard.writeText(text);
      return true;
    }

    const helper = document.createElement("textarea");
    helper.value = text;
    helper.setAttribute("readonly", "readonly");
    helper.style.position = "fixed";
    helper.style.opacity = "0";
    document.body.appendChild(helper);
    helper.select();
    const copied = document.execCommand("copy");
    document.body.removeChild(helper);
    return copied;
  }

  copyButtons.forEach((button) => {
    button.addEventListener("click", async () => {
      const text = button.getAttribute("data-copy-text") || "";
      const original = button.textContent;
      if (!text) {
        return;
      }

      try {
        await copyValue(text);
        button.textContent = "Copied";
      } catch {
        button.textContent = "Copy Failed";
      }

      window.setTimeout(() => {
        button.textContent = original;
      }, 1800);
    });
  });
})();
