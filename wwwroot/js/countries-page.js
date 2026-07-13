(function () {
  const map = document.querySelector("[data-countries-map]");
  if (!map) {
    return;
  }

  const tooltip = map.querySelector("[data-map-tooltip]");
  const regions = Array.from(map.querySelectorAll(".countries-map-region"));
  if (!tooltip || regions.length === 0) {
    return;
  }

  const fields = {
    code: tooltip.querySelector("[data-tooltip-code]"),
    name: tooltip.querySelector("[data-tooltip-name]"),
    capital: tooltip.querySelector("[data-tooltip-capital]"),
    gdp: tooltip.querySelector("[data-tooltip-gdp]"),
    population: tooltip.querySelector("[data-tooltip-population]"),
    bankAssets: tooltip.querySelector("[data-tooltip-bank-assets]"),
    summary: tooltip.querySelector("[data-tooltip-summary]"),
    link: tooltip.querySelector("[data-tooltip-link]")
  };

  let activeRegion = null;

  function setTooltipContent(region) {
    const data = region.dataset;
    fields.code.textContent = data.country || "Country";
    fields.name.textContent = data.name || "Data temporarily unavailable";
    fields.capital.textContent = data.capital || "Data temporarily unavailable";
    fields.gdp.textContent = data.gdp || "Data temporarily unavailable";
    fields.population.textContent = data.population || "Data temporarily unavailable";
    fields.bankAssets.textContent = data.bankAssets || "Data temporarily unavailable";
    fields.summary.textContent = data.summary || "Data temporarily unavailable";
    fields.link.href = data.url || "#";
  }

  function placeTooltip(region) {
    if (window.innerWidth <= 991) {
      tooltip.style.left = "";
      tooltip.style.top = "";
      return;
    }

    const mapRect = map.getBoundingClientRect();
    const regionRect = region.getBoundingClientRect();
    const left = Math.min(regionRect.right - mapRect.left + 16, mapRect.width - tooltip.offsetWidth - 12);
    const top = Math.max(regionRect.top - mapRect.top - 8, 12);
    tooltip.style.left = `${left}px`;
    tooltip.style.top = `${top}px`;
  }

  function openTooltip(region) {
    activeRegion = region;
    regions.forEach((item) => item.classList.toggle("is-active", item === region));
    setTooltipContent(region);
    tooltip.hidden = false;
    placeTooltip(region);
  }

  function closeTooltip() {
    if (window.innerWidth <= 991) {
      return;
    }

    activeRegion = null;
    regions.forEach((item) => item.classList.remove("is-active"));
    tooltip.hidden = true;
  }

  regions.forEach((region) => {
    region.addEventListener("mouseenter", () => openTooltip(region));
    region.addEventListener("focus", () => openTooltip(region));
    region.addEventListener("click", (event) => {
      if (window.innerWidth > 991) {
        return;
      }

      if (activeRegion !== region) {
        event.preventDefault();
        openTooltip(region);
      }
    });
  });

  map.addEventListener("mouseleave", closeTooltip);
  window.addEventListener("resize", () => {
    if (activeRegion) {
      placeTooltip(activeRegion);
    }
  });
})();
