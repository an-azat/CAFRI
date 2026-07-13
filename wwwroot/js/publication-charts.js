(function () {
  if (typeof window.Chart === "undefined") {
    return;
  }

  const canvases = document.querySelectorAll("[data-publication-chart]");
  canvases.forEach((canvas) => {
    const raw = canvas.getAttribute("data-publication-chart");
    if (!raw) {
      return;
    }

    let config;
    try {
      config = JSON.parse(raw);
    } catch {
      return;
    }

    const chartType = config.type === "doughnut" ? "doughnut" : config.type === "bar" ? "bar" : "line";
    const palette = config.series?.map((item) => item.color) || ["#1476ff"];

    const doughnutPalette = ["#1476ff", "#55c6ff", "#4de0b5", "#f6b74a", "#ff7e67"];

    const datasets = chartType === "doughnut"
      ? [
          {
            label: config.series?.[0]?.label || config.title,
            data: config.series?.[0]?.values || [],
            backgroundColor: (config.labels || []).map((_, index) => doughnutPalette[index % doughnutPalette.length]),
            borderColor: "#031126",
            borderWidth: 2
          }
        ]
      : (config.series || []).map((item, index) => ({
          label: item.label,
          data: item.values,
          borderColor: item.color,
          backgroundColor: chartType === "bar" ? item.color : `${item.color}33`,
          pointBackgroundColor: item.color,
          pointBorderColor: "#dbe8ff",
          pointRadius: chartType === "line" ? 3 : 0,
          pointHoverRadius: chartType === "line" ? 4 : 0,
          borderWidth: 2,
          tension: 0.35,
          fill: chartType === "line" && index === 0
        }));

    new window.Chart(canvas, {
      type: chartType,
      data: {
        labels: config.labels || [],
        datasets
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: "bottom",
            labels: {
              color: "rgba(219, 232, 255, 0.82)",
              boxWidth: 14,
              usePointStyle: chartType === "line"
            }
          },
          tooltip: {
            backgroundColor: "rgba(7, 19, 39, 0.96)",
            borderColor: "rgba(115, 154, 204, 0.24)",
            borderWidth: 1,
            callbacks: {
              label(context) {
                const suffix = config.suffix || "";
                return `${context.dataset.label}: ${context.formattedValue}${suffix}`;
              }
            }
          }
        },
        scales: chartType === "doughnut" ? {} : {
          x: {
            ticks: {
              color: "rgba(169, 181, 200, 0.88)"
            },
            grid: {
              color: "rgba(115, 154, 204, 0.08)"
            }
          },
          y: {
            ticks: {
              color: "rgba(169, 181, 200, 0.88)"
            },
            grid: {
              color: "rgba(115, 154, 204, 0.08)"
            }
          }
        }
      }
    });
  });
})();
