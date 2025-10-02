import { useMapEvents } from "react-leaflet";
import { addLocalReport } from "../utils/storage";

export default function ReportHandler({ armingReport, setArmingReport, addReport, setLastReport }) {
  useMapEvents({
    click(e) {
      if (armingReport) {
        const rpt = addLocalReport(e.latlng.lat, e.latlng.lng);
        addReport(rpt);
        setLastReport(rpt);
        setArmingReport(false);
      }
    },
  });
  return null;
}
