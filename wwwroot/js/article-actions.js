(function () {
  const buttons = document.querySelectorAll(".article-actions button");
  buttons.forEach((button) => {
    button.addEventListener("click", () => {
      if (button.textContent?.trim() === "Save") {
        button.classList.toggle("is-active");
      }
      if (button.textContent?.trim() === "Cite") {
        button.textContent = "Copied";
        window.setTimeout(() => {
          button.textContent = "Cite";
        }, 1200);
      }
    });
  });
})();
