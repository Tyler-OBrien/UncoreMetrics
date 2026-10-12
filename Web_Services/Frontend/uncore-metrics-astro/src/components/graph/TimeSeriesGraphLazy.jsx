// Renders nothing until the surrounding <details> is opened, then loads TimeSeriesGraph.
// Keeps pages with many collapsed charts from firing every chart's API request up front.

import { useEffect, useRef, useState } from "react";

const TimeSeriesGraphLazy = (props) => {
  const anchorRef = useRef(null);
  const [Graph, setGraph] = useState(null);

  useEffect(() => {
    const details = anchorRef.current?.closest("details");
    const load = () => {
      if (details && !details.open) return;
      import("./TimeSeriesGraph").then((module) => setGraph(() => module.default));
      details?.removeEventListener("toggle", load);
    };
    details?.addEventListener("toggle", load);
    load();
    return () => details?.removeEventListener("toggle", load);
  }, []);

  return <div ref={anchorRef}>{Graph ? <Graph {...props} /> : <div className="chart-state">Open to load chart.</div>}</div>;
};

export default TimeSeriesGraphLazy;
