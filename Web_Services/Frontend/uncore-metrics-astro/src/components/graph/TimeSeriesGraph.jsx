// Uptime or player-count history for one server, one game, or every game.
// Data comes through this site's /api proxy; endpoint and bucket choice live in lib/timeseries.ts.

import { useEffect, useRef, useState } from "react";
import Plotly from "plotly.js-basic-dist-min";
import { axis, baseLayout, compareColor, isDarkMode, themeRelayout } from "./chartTheme";
import { endpointFor, rangesFor, toSeries } from "../../lib/timeseries";

const TimeSeriesGraph = ({ kind, scope, title, defaultHours = 72, gameNames = {} }) => {
  const plotRef = useRef(null);
  const ranges = rangesFor(scope);
  const initialHours = ranges.some((range) => range.hours === defaultHours) ? defaultHours : ranges[0].hours;
  const [hours, setHours] = useState(initialHours);
  const [series, setSeries] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState(false);

  // Fetch whenever the range changes. A stale response (range changed mid-flight) is ignored.
  useEffect(() => {
    let current = true;
    setIsLoading(true);
    setLoadError(false);
    fetch(`/api/${endpointFor(kind, scope, hours)}`)
      .then((response) => {
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        return response.json();
      })
      .then((json) => {
        if (current) setSeries(toSeries(kind, scope, json.data ?? [], gameNames));
      })
      .catch(() => {
        if (current) {
          setSeries([]);
          setLoadError(true);
        }
      })
      .finally(() => {
        if (current) setIsLoading(false);
      });
    return () => {
      current = false;
    };
  }, [hours]);

  useEffect(() => {
    const node = plotRef.current;
    if (!node || series.length === 0) return;

    const dark = isDarkMode();
    const data = series.map((s, index) => ({
      type: "scatter",
      mode: "lines",
      name: s.name,
      x: s.x,
      y: s.y,
      line: {
        width: 2,
        color: compareColor(s.name, index),
        shape: s.step ? "hv" : "spline",
        smoothing: 0.4,
        dash: s.dash ? "dot" : "solid",
      },
      hovertemplate: kind === "uptime" ? "%{y:.1f}%" : "%{y:,.0f}",
    }));

    const layout = {
      ...baseLayout(dark),
      margin: { l: 60, r: 20, t: 10, b: 40 },
      showlegend: series.length > 1,
      xaxis: axis(dark, { type: "date" }),
      yaxis: axis(dark, kind === "uptime" ? { range: [0, 105], ticksuffix: "%" } : { rangemode: "tozero" }),
    };

    let disposed = false;
    const observer = new ResizeObserver(() => {
      if (!disposed && node._fullLayout) Plotly.Plots.resize(node);
    });
    observer.observe(node);
    Plotly.newPlot(node, data, layout, { displaylogo: false, responsive: true });

    return () => {
      disposed = true;
      observer.disconnect();
      Plotly.purge(node);
    };
  }, [series]);

  useEffect(() => {
    const onThemeChange = () => {
      if (plotRef.current?._fullLayout) Plotly.relayout(plotRef.current, themeRelayout(isDarkMode()));
    };
    window.addEventListener("themeChange", onThemeChange);
    return () => window.removeEventListener("themeChange", onThemeChange);
  }, []);

  const status = isLoading
    ? "Loading chart…"
    : loadError
      ? "Chart unavailable. Try another range or reload."
      : series.length === 0
        ? "No data in this period."
        : null;

  return (
    <div className="w-full">
      <div className="flex flex-wrap items-center justify-between gap-2 mb-3">
        <h3 className="text-base font-semibold">{title}</h3>
        <select
          aria-label={`${title} time range`}
          value={hours}
          onChange={(event) => setHours(Number(event.target.value))}
          className="graph-select"
        >
          {ranges.map((range) => (
            <option key={range.hours} value={range.hours}>
              {range.label}
            </option>
          ))}
        </select>
      </div>
      <div role="status" className="chart-state">
        {status}
      </div>
      <div
        ref={plotRef}
        aria-busy={isLoading}
        className={isLoading && series.length === 0 ? "animate-pulse" : undefined}
        style={{ height: "var(--monitor-chart-height)", width: "100%", display: series.length ? "block" : "none" }}
      />
    </div>
  );
};

export default TimeSeriesGraph;
