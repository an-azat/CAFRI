(function () {
  const map = document.querySelector("[data-interactive-map]");
  if (!map) {
    return;
  }

  const tooltip = map.querySelector("[data-interactive-map-tooltip]");
  const indicatorSelect = map.querySelector("[data-map-indicator-select]");
  const regions = Array.from(map.querySelectorAll(".interactive-map-country"));
  if (!tooltip || regions.length === 0) {
    return;
  }

  const fields = {
    code: tooltip.querySelector("[data-map-tooltip-code]"),
    name: tooltip.querySelector("[data-map-tooltip-name]"),
    capital: tooltip.querySelector("[data-map-tooltip-capital]"),
    gdp: tooltip.querySelector("[data-map-tooltip-gdp]"),
    population: tooltip.querySelector("[data-map-tooltip-population]"),
    bankAssets: tooltip.querySelector("[data-map-tooltip-bank-assets]"),
    indicatorLabel: tooltip.querySelector("[data-map-tooltip-indicator-label]"),
    indicatorValue: tooltip.querySelector("[data-map-tooltip-indicator-value]"),
    summary: tooltip.querySelector("[data-map-tooltip-summary]"),
    link: tooltip.querySelector("[data-map-tooltip-link]")
  };

  let activeRegion = null;

  function readIndicators(region) {
    const raw = region.dataset.indicators;
    if (!raw) {
      return [];
    }

    try {
      const parsed = JSON.parse(raw);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }

  function applyIndicator(region, indicatorName) {
    const indicators = readIndicators(region);
    const selected = indicators.find((item) => item.name === indicatorName) || indicators[0];
    if (!selected) {
      region.dataset.indicatorLabel = "Indicator";
      region.dataset.indicatorValue = "No data";
      return;
    }

    region.dataset.indicatorLabel = selected.label || selected.name || "Indicator";
    region.dataset.indicatorValue = selected.value || "No data";
  }

  function setTooltipContent(region) {
    const data = region.dataset;
    fields.code.textContent = data.code || "Country";
    fields.name.textContent = data.name || "Data temporarily unavailable";
    fields.capital.textContent = data.capital || "Data temporarily unavailable";
    fields.gdp.textContent = data.gdp || "Data temporarily unavailable";
    fields.population.textContent = data.population || "Data temporarily unavailable";
    fields.bankAssets.textContent = data.bankAssets || "Data temporarily unavailable";
    fields.indicatorLabel.textContent = data.indicatorLabel || "Indicator";
    fields.indicatorValue.textContent = data.indicatorValue || "Indicator data temporarily unavailable";
    fields.summary.textContent = data.summary || "Data temporarily unavailable";
    fields.link.href = data.url || "#";
  }

  function syncIndicatorSelection(indicatorName) {
    regions.forEach((region) => applyIndicator(region, indicatorName));

    if (activeRegion) {
      setTooltipContent(activeRegion);
      positionTooltip(activeRegion);
    }
  }

  function positionTooltip(region) {
    if (window.innerWidth <= 991) {
      tooltip.style.left = "";
      tooltip.style.top = "";
      return;
    }

    const mapRect = map.getBoundingClientRect();
    const regionRect = region.getBoundingClientRect();
    const left = Math.min(regionRect.right - mapRect.left + 18, mapRect.width - tooltip.offsetWidth - 14);
    const top = Math.max(regionRect.top - mapRect.top - 6, 12);
    tooltip.style.left = `${left}px`;
    tooltip.style.top = `${top}px`;
  }

  function openTooltip(region) {
    activeRegion = region;
    regions.forEach((item) => item.classList.toggle("is-active", item === region));
    setTooltipContent(region);
    tooltip.hidden = false;
    positionTooltip(region);
  }

  function closeTooltip() {
    if (window.innerWidth <= 991) {
      return;
    }

    tooltip.hidden = true;
    regions.forEach((item) => item.classList.remove("is-active"));
    activeRegion = null;
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
  document.addEventListener("click", (event) => {
    if (window.innerWidth > 991 || tooltip.hidden) {
      return;
    }

    if (!map.contains(event.target)) {
      tooltip.hidden = true;
      regions.forEach((item) => item.classList.remove("is-active"));
      activeRegion = null;
    }
  });
  window.addEventListener("resize", () => {
    if (activeRegion) {
      positionTooltip(activeRegion);
    }
  });

  if (indicatorSelect) {
    indicatorSelect.addEventListener("change", () => {
      syncIndicatorSelection(indicatorSelect.value);
    });

    syncIndicatorSelection(indicatorSelect.value);
  }
})();
