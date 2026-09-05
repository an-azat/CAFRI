(function () {
  const mapEl = document.getElementById("countries-leaflet-map");
  const frame = document.querySelector(".countries-map-frame");
  const fallback = document.querySelector("[data-map-error]");

  if (!mapEl || !frame) {
    return;
  }

  function showFallback() {
    if (fallback) {
      fallback.hidden = false;
    }
    mapEl.hidden = true;
  }

  if (typeof L === "undefined") {
    showFallback();
    return;
  }

  const dataEl = document.getElementById("countries-map-data");
  let countries = [];
  try {
    countries = dataEl ? JSON.parse(dataEl.textContent) : [];
  } catch (error) {
    countries = [];
  }
  const byCode = new Map(countries.map((c) => [c.code, c]));

  // --- GDP growth range helpers (shared by fill color + legend semantics) ---
  const RANGE_COLORS = {
    high: "#10b981",
    good: "#2563eb",
    mid: "#eab308",
    low: "#f97316",
    negative: "#ef4444",
    none: "#94a3b8"
  };

  function rangeFor(value) {
    if (value >= 6) return "high";
    if (value >= 4) return "good";
    if (value >= 2) return "mid";
    if (value >= 0) return "low";
    return "negative";
  }

  const rangeByCode = {};
  const growthDisplayByCode = {};

  function computeGrowth(year) {
    countries.forEach((c) => {
      const raw = c.growth ? c.growth[year] : undefined;
      const numeric = raw ? parseFloat(String(raw).split(" ")[0]) : NaN;
      const hasValue = !Number.isNaN(numeric);
      rangeByCode[c.code] = hasValue ? rangeFor(numeric) : "none";
      growthDisplayByCode[c.code] = hasValue ? (numeric >= 0 ? "+" : "") + numeric.toFixed(1) + "%" : "No data";
    });
  }

  function tooltipHtml(code) {
    const data = byCode.get(code);
    const name = (data ? data.name : code).toUpperCase();
    const growth = growthDisplayByCode[code] || "No data";
    return (
      '<span class="map-country-tooltip__name">' + name + "</span>" +
      '<span class="map-country-tooltip__growth">' + growth + "</span>"
    );
  }

  // --- Map setup ---
  const map = L.map(mapEl, {
    zoomControl: false,
    attributionControl: false,
    scrollWheelZoom: false,
    minZoom: 4,
    maxZoom: 8
  }).setView([44, 66], 5);

  mapEl.addEventListener("click", () => map.scrollWheelZoom.enable());
  mapEl.addEventListener("mouseleave", () => map.scrollWheelZoom.disable());

  // Deliberately physical/terrain basemaps rather than Esri's political
  // "Canvas" basemaps: those bake in their own country borders and place
  // labels, which visibly duplicated (and slightly misaligned with) the
  // borders/labels our own GeoJSON overlay already draws.
  const TILE_LAYERS = {
    light: {
      url: "https://server.arcgisonline.com/ArcGIS/rest/services/World_Physical_Map/MapServer/tile/{z}/{y}/{x}",
      options: { maxZoom: 8, attribution: "Tiles &copy; Esri" }
    },
    dark: {
      url: "https://server.arcgisonline.com/ArcGIS/rest/services/Elevation/World_Hillshade_Dark/MapServer/tile/{z}/{y}/{x}",
      options: { maxZoom: 8, attribution: "Tiles &copy; Esri" }
    },
    satellite: {
      url: "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}",
      options: { maxZoom: 19, attribution: "Tiles &copy; Esri" }
    }
  };

  let tileLayer = null;

  function setTileStyle(style) {
    const config = TILE_LAYERS[style] || TILE_LAYERS.light;
    if (tileLayer) {
      map.removeLayer(tileLayer);
    }
    tileLayer = L.tileLayer(config.url, config.options).addTo(map);
    tileLayer.bringToBack();
    mapEl.dataset.mapStyle = style;
  }

  setTileStyle("light");

  L.control.zoom({ position: "bottomright" }).addTo(map);
  L.control.scale({ position: "bottomright", imperial: false, maxWidth: 120 }).addTo(map);

  const FullscreenControl = L.Control.extend({
    options: { position: "bottomright" },
    onAdd: function () {
      const container = L.DomUtil.create("div", "leaflet-bar countries-map-fullscreen");
      const link = L.DomUtil.create("a", "", container);
      link.href = "#";
      link.title = "Toggle fullscreen";
      link.setAttribute("role", "button");
      link.innerHTML = '<svg viewBox="0 0 20 20" aria-hidden="true"><path d="M3 8V4a1 1 0 0 1 1-1h4M13 3h4a1 1 0 0 1 1 1v4M17 12v4a1 1 0 0 1-1 1h-4M8 17H4a1 1 0 0 1-1-1v-4" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"/></svg>';
      L.DomEvent.on(link, "click", L.DomEvent.stop).on(link, "click", () => {
        if (document.fullscreenElement) {
          document.exitFullscreen();
        } else {
          frame.requestFullscreen?.();
        }
      });
      return container;
    }
  });
  map.addControl(new FullscreenControl());
  frame.addEventListener("fullscreenchange", () => {
    setTimeout(() => map.invalidateSize(), 50);
  });

  const layerByCode = new Map();
  let activeCode = null;

  function styleForCode(code) {
    const range = rangeByCode[code] || "none";
    const isActive = code === activeCode;
    const dim = activeCode && !isActive;
    return {
      fillColor: RANGE_COLORS[range] || RANGE_COLORS.none,
      fillOpacity: dim ? 0.28 : 0.62,
      color: isActive ? "#0b1f35" : "rgba(11, 31, 53, 0.4)",
      weight: isActive ? 2.5 : 1.25
    };
  }

  function refreshStyles() {
    layerByCode.forEach((layer, code) => {
      layer.setStyle(styleForCode(code));
      if (code === activeCode) {
        layer.bringToFront();
      }
    });
  }

  function preview(code) {
    activeCode = code;
    refreshStyles();
  }

  function restore() {
    activeCode = null;
    refreshStyles();
  }

  const yearSelect = document.querySelector("[data-map-year-select]");
  const legendYear = document.querySelector("[data-map-legend-year]");

  function applyGrowthForYear(year) {
    computeGrowth(year);
    layerByCode.forEach((layer, code) => {
      layer.setTooltipContent(tooltipHtml(code));
    });
    refreshStyles();
    if (legendYear) {
      legendYear.textContent = year;
    }
  }

  if (yearSelect) {
    yearSelect.addEventListener("change", () => applyGrowthForYear(yearSelect.value));
  }

  computeGrowth(mapEl.dataset.selectedYear || "");

  const styleOptions = Array.from(document.querySelectorAll("[data-map-style-option]"));
  styleOptions.forEach((input) => {
    input.addEventListener("change", () => {
      if (input.checked) {
        setTileStyle(input.value);
      }
    });
  });

  const overlayOptions = Array.from(document.querySelectorAll("[data-map-overlay-option]"));
  overlayOptions.forEach((input) => {
    input.addEventListener("change", () => {
      input.closest(".countries-map-overlay-option")?.classList.toggle("is-checked", input.checked);
    });
  });

  const categoryOptions = Array.from(document.querySelectorAll("[data-map-category-option]"));

  const resetButton = document.querySelector("[data-map-reset-layers]");
  if (resetButton) {
    resetButton.addEventListener("click", () => {
      categoryOptions.forEach((input) => {
        input.checked = true;
      });
      overlayOptions.forEach((input) => {
        input.checked = false;
        input.closest(".countries-map-overlay-option")?.classList.remove("is-checked");
      });
      const lightOption = styleOptions.find((input) => input.value === "light");
      if (lightOption && !lightOption.checked) {
        lightOption.checked = true;
        setTileStyle("light");
      }
      if (countryLayer) {
        // Cap fitBounds' own zoom independently of the map's manual-zoom ceiling -
        // otherwise a wide viewport asks fitBounds for a zoom higher than maxZoom,
        // it clamps, and the "fit the region" view degenerates into "fill the
        // screen with whichever country is biggest".
        map.fitBounds(countryLayer.getBounds(), { padding: [24, 24], maxZoom: 6 });
      }
    });
  }

  // Neighboring countries: non-interactive context layer with plain labels.
  const NEIGHBOR_LABELS = {
    RUS: [53.5, 66],
    CHN: [40.5, 84],
    IRN: [36.0, 54],
    AFG: [34.2, 65],
    PAK: [33.0, 72.5]
  };

  fetch("/js/data/central-asia-neighbors.geo.json")
    .then((response) => response.json())
    .then((geojson) => {
      L.geoJSON(geojson, {
        interactive: false,
        style: { color: "rgba(148, 163, 184, 0.45)", weight: 1, fillColor: "#94a3b8", fillOpacity: 0.12 }
      }).addTo(map);

      geojson.features.forEach((feature) => {
        const position = NEIGHBOR_LABELS[feature.id];
        if (!position) {
          return;
        }
        L.marker(position, {
          interactive: false,
          keyboard: false,
          icon: L.divIcon({
            className: "map-neighbor-label",
            html: feature.properties.name.toUpperCase(),
            iconSize: null
          })
        }).addTo(map);
      });
    })
    .catch(() => {
      /* neighbor context layer is decorative; ignore load failures */
    });

  let countryLayer = null;

  fetch("/js/data/central-asia.geo.json")
    .then((response) => response.json())
    .then((geojson) => {
      countryLayer = L.geoJSON(geojson, {
        style: (feature) => styleForCode(feature.properties.code),
        onEachFeature: (feature, layer) => {
          const code = feature.properties.code;
          layerByCode.set(code, layer);

          layer.bindTooltip(tooltipHtml(code), {
            permanent: true,
            direction: "center",
            className: "map-country-tooltip",
            opacity: 1
          });

          layer.on("mouseover", () => preview(code));
          layer.on("mouseout", () => restore());
          layer.on("click", () => {
            const data = byCode.get(code);
            if (data && data.url) {
              window.location.href = data.url;
            }
          });

          const data = byCode.get(code);
          if (data && data.capitalLat != null && data.capitalLng != null) {
            L.circleMarker([data.capitalLat, data.capitalLng], {
              radius: 4,
              className: "map-capital-dot",
              color: "#0b1f35",
              weight: 1.5,
              fillColor: "#ffffff",
              fillOpacity: 1,
              interactive: false
            })
              .addTo(map)
              .bindTooltip(data.capital, {
                permanent: true,
                direction: "right",
                offset: [6, 0],
                className: "map-capital-label",
                opacity: 1
              });
          }
        }
      }).addTo(map);

      // The container is sized by CSS grid + aspect-ratio, which can settle
      // after Leaflet's initial (stale) size measurement - without this,
      // fitBounds silently miscalculates the zoom/center.
      map.invalidateSize();
      // Cap fitBounds' own zoom independently of the map's manual-zoom ceiling -
        // otherwise a wide viewport asks fitBounds for a zoom higher than maxZoom,
        // it clamps, and the "fit the region" view degenerates into "fill the
        // screen with whichever country is biggest".
        map.fitBounds(countryLayer.getBounds(), { padding: [24, 24], maxZoom: 6 });
      map.setMaxBounds(countryLayer.getBounds().pad(1.4));

      applyGrowthForYear(mapEl.dataset.selectedYear || "");
      refreshStyles();
    })
    .catch(() => {
      showFallback();
    });

  // --- Country cards carousel (below the map) ---
  const grid = document.querySelector("[data-countries-grid]");
  const prevBtn = document.querySelector("[data-countries-grid-prev]");
  const nextBtn = document.querySelector("[data-countries-grid-next]");
  if (grid && prevBtn && nextBtn) {
    const scrollByCard = (direction) => {
      const card = grid.querySelector(".country-card");
      const amount = card ? card.getBoundingClientRect().width + 20 : grid.clientWidth * 0.8;
      grid.scrollBy({ left: direction * amount, behavior: "instant" });
    };
    prevBtn.addEventListener("click", () => scrollByCard(-1));
    nextBtn.addEventListener("click", () => scrollByCard(1));
  }
})();
