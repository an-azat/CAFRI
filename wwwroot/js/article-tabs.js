(function () {
  const tabs = document.querySelectorAll(".article-tabs__button");
  if (tabs.length === 0) {
    return;
  }

  tabs.forEach((tab) => {
    tab.addEventListener("click", () => {
      tabs.forEach((item) => item.classList.remove("is-active"));
      tab.classList.add("is-active");
    });
  });
})();
