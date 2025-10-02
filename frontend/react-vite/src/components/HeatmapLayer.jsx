import { useEffect } from "react";
import { useMap } from "react-leaflet";
import L from "leaflet";
import "leaflet.heat";

export default function HeatmapLayer({ reports, enabled }) {
  const map = useMap();

  useEffect(() => {
    if (!enabled || reports.length === 0) return;

    const points = reports.map((r) => [r.lat, r.lng, 0.5]);
    const layer = L.heatLayer(points, { radius: 25, blur: 15, maxZoom: 17 });
    layer.addTo(map);

    return () => {
      map.removeLayer(layer);
    };
  }, [reports, enabled, map]);

  return null;
}
