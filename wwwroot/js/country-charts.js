(function () {
  if (typeof window.Chart === "undefined") {
    return;
  }

  const canvases = document.querySelectorAll("[data-country-chart]");
  canvases.forEach((canvas) => {
    const labels = (canvas.dataset.labels || "")
      .split(",")
      .map((item) => item.trim().replace(/^'|'$/g, ""))
      .filter(Boolean);
    const values = (canvas.dataset.values || "")
      .split(",")
      .map((item) => Number.parseFloat(item))
      .filter((item) => !Number.isNaN(item));
    const suffix = canvas.dataset.suffix || "";
    const title = canvas.dataset.title || "";

    const isBar = title.includes("Banking Sector Assets");

    new window.Chart(canvas, {
      type: isBar ? "bar" : "line",
      data: {
        labels,
        datasets: [
          {
            label: title,
            data: values,
            borderColor: "#4f8cff",
            backgroundColor: isBar ? "rgba(79, 140, 255, 0.88)" : "rgba(79, 140, 255, 0.18)",
            pointBackgroundColor: "#7db1ff",
            pointBorderColor: "#dbe8ff",
            pointRadius: isBar ? 0 : 4,
            pointHoverRadius: isBar ? 0 : 5,
            borderWidth: 2,
            tension: 0.35,
            fill: !isBar
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            backgroundColor: "rgba(7, 19, 39, 0.96)",
            borderColor: "rgba(115, 154, 204, 0.24)",
            borderWidth: 1,
            displayColors: false,
            callbacks: {
              title(items) {
                return `Year: ${items[0].label}`;
              },
              label(context) {
                return `Value: ${context.formattedValue}${suffix}`;
              }
            }
          }
        },
        scales: {
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
