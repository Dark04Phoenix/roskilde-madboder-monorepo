import { useEffect, useState, useCallback, useMemo } from "react";
import { MapContainer, TileLayer, Marker, CircleMarker, Popup } from "react-leaflet";
import "leaflet/dist/leaflet.css";
import L from "leaflet";
import { useGeolocation } from "@uidotdev/usehooks";

// Components
import Modal from "./components/Modal";
import Topbar from "./components/Topbar";
import Dashboard from "./components/Dashboard";
import ReportHandler from "./components/ReportHandler";
import HeatmapLayer from "./components/HeatmapLayer";

// Utils
import { loadReports, removeReportById, saveReports } from "./utils/storage";

// Kort-område: Roskilde Festival (afgrænset for at holde UX i scope)
const FESTIVAL_CENTER = [55.6175, 12.0789]; // midt på festivalpladsen
const FESTIVAL_BOUNDS = [
  [55.607, 12.05],
  [55.642, 12.10],
];

const FESTIVAL_ZOOM = 16; // tæt nok til at se pladsen fra start
const MIN_ZOOM = 15; // undgå for langt ude overview
const MAX_ZOOM = 18; // undgå for langt inde (unødvendige tiles)
const BOUNDS_VISCOUS = 1.0; // låser pan blødt inde i festivalområdet

// ---- Blå ikon til madboder ----
const stallIcon = new L.Icon({
  iconUrl: "https://maps.google.com/mapfiles/ms/icons/blue-dot.png",
  iconSize: [32, 32],
  iconAnchor: [16, 32],
});
const stationIcon = new L.Icon({
  iconUrl: "https://cdn-icons-png.flaticon.com/512/4927/4927289.png",
  iconSize: [36, 36],        // større end før
  iconAnchor: [18, 36],
  popupAnchor: [0, -36],
});


// ---- Hook: værdi i localStorage ----
function usePersistedState(key, defaultValue) {
  const [value, setValue] = useState(() => {
    const saved = localStorage.getItem(key);
    return saved ? JSON.parse(saved) : defaultValue;
  });
  useEffect(() => {
    localStorage.setItem(key, JSON.stringify(value));
  }, [key, value]);
  return [value, setValue];
}

// --- Location component (ikke brugt i) UI) ---
function Location() {
  const state = useGeolocation();
  const [show, setShow] = useState(false);

  if (state.loading) {
    return <p>loading... (you may need to enable permissions)</p>;
  }

  if (state.error) {
    return <p>Enable permissions to access your location data</p>;
  }

  return (
    <div>
      <button onClick={() => setShow(!show)}>
        {show ? "Hide my location" : "Show my location"}
      </button>
      {show && state.latitude && state.longitude && (
        <p>
          Latitude: {state.latitude}, Longitude: {state.longitude}
        </p>
      )}
    </div>
  );
}

export default function App() {
  const [stalls, setStalls] = useState([]);
  const [stations, setStations] = useState([]);
  const [role, setRole] = usePersistedState("gronkilde_role", "guest");
  const [user, setUser] = usePersistedState("gronkilde_user", null);
  const [activeStall, setActiveStall] = useState(null);

  const [reports, setReports] = useState(loadReports());
  const [heatOn, setHeatOn] = useState(false);
  const [armingReport, setArmingReport] = useState(false);
  const [lastReport, setLastReport] = useState(null);
  const canMark = role === "volunteer";

  // Map backend-rolle til frontend-rolle (ensartet brug i app)
  function mapBackendRole(r) {
    const lower = (r || "").toLowerCase();
    if (lower === "frivillig") return "volunteer";
    if (lower === "arrangør" || lower === "arrangoer" || lower === "arrangor") return "organizer";
    return "guest";
  }

  // Load stalls.json
  useEffect(() => {
    fetch("/stalls.json")
      .then((r) => r.json())
      .then((data) => {
        const stallsArray = data.stalls || data;
        setStalls(stallsArray);
      })
      .catch((err) => console.error("Kunne ikke indlæse stalls.json:", err));
  }, []);

  // Hent stationer (read-only visning for alle roller)
  useEffect(() => {
    fetch("http://localhost:5013/api/Stations")
      .then((r) => (r.ok ? r.json() : []))
      .then((data) => {
        console.log("stations from backend:", data);
        if (Array.isArray(data)) {
          data.forEach((s, i) => console.log("station", i, s));
        }
        setStations(data || []);
      })
      .catch((err) => console.error("Kunne ikke hente stationer:", err));
  }, []);

  // Volunteer: opdater station status via backend
  const updateStationStatus = useCallback(
    async (stationId) => {
      if (role !== "volunteer" || !user) return;
      const statusType = window.prompt("Ny status for stationen (fx 'Fyldt', 'Mangler sække'):");
      if (!statusType || !statusType.trim()) return;
      try {
        const res = await fetch(`http://localhost:5013/api/Stations/${stationId}/status`, {
          method: "PUT",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ userId: user.userId, statusType }),
        });
        if (res.ok) {
          // Hent stationer igen for at afspejle seneste status
          fetch("http://localhost:5013/api/Stations")
            .then((r) => (r.ok ? r.json() : []))
            .then((data) => setStations(data || []))
            .catch((err) => console.error("Kunne ikke hente stationer:", err));
        } else {
          console.warn("Station status PUT fejlede", await res.text());
        }
      } catch (e) {
        console.error("Fejl ved opdater station status", e);
      }
    },
    [role, user]
  );

  // Stop markering hvis rollen ikke må markere
  useEffect(() => {
    if (!canMark && armingReport) {
      setArmingReport(false);
    }
  }, [canMark, armingReport]);

  // Hold role og user i sync: ingen user => gæst; user bestemmer rollen
  useEffect(() => {
    if (user) {
      setRole(mapBackendRole(user.rolle));
    } else if (role !== "guest") {
      setRole("guest");
    }
  }, [user]);

  // Like funktion
  function handleLike(stallId) {
    setStalls((prev) =>
      prev.map((s) =>
        s.id === stallId ? { ...s, likes: s.likes + 1, _liked: true } : s
      )
    );
    setActiveStall((prev) =>
      prev ? { ...prev, likes: prev.likes + 1, _liked: true } : prev
    );
  }

  function clearAllReports() {
    if (window.confirm("Er du sikker på, at du vil rydde alle markeringer?")) {
      saveReports([]);
      setReports([]);
      setLastReport(null);
    }
  }

  // Eksporter rapporter (lokal fil)
  function exportReports() {
    const data = JSON.stringify(reports, null, 2);
    const blob = new Blob([data], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "rapporter.json";
    a.click();
    URL.revokeObjectURL(url);
  }

  // Dashboard totals
  function computeTotals() {
    const count = stalls.length;
    if (count === 0) return null;
    const sumCo2 = stalls.reduce((a, s) => a + s.co2PerMealKg, 0);
    const sumOrganic = stalls.reduce((a, s) => a + s.organicPct, 0);
    const avgCo2 = (sumCo2 / count).toFixed(2);
    const avgOrganic = Math.round(sumOrganic / count);
    const underGoal = stalls.filter((s) => s.co2PerMealKg <= 0.75).length;
    return { avgCo2, avgOrganic, underGoal, count, reports: reports.length };
  }

  const totals = computeTotals();

  return (
    <div>
      {/* Topbar */}
      <Topbar
        role={role}
        setRole={setRole}
        user={user}
        setUser={setUser}
        heatOn={heatOn}
        setHeatOn={setHeatOn}
        armingReport={armingReport}
        setArmingReport={setArmingReport}
        lastReport={lastReport}
        removeLastReport={() => {
          if (lastReport) {
            removeReportById(lastReport.id);
            setReports(loadReports());
            setLastReport(null);
          }
        }}
        exportReports={exportReports}
        clearAllReports={clearAllReports}
        canMark={canMark}
      />

      {/* Kort */}
      <MapContainer
        center={FESTIVAL_CENTER}
        zoom={FESTIVAL_ZOOM}
        minZoom={MIN_ZOOM}
        maxZoom={MAX_ZOOM}
        maxBounds={FESTIVAL_BOUNDS}
        maxBoundsViscosity={BOUNDS_VISCOUS}
        style={{ height: "calc(100vh - 56px)" }} // 56px ≈ højden på topbaren
        scrollWheelZoom={"center"}
        dragging={true}
        doubleClickZoom={true}
        touchZoom={true}
        boxZoom={false}
      >
        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />


        {/* Boder med blå ikoner */}
        {stalls.map((s) => (
          <Marker
            key={s.id}
            position={s.coords}
            icon={stallIcon}
            eventHandlers={{ click: () => setActiveStall(s) }}
          />
        ))}

        {/* Stationer (alle roller, read-only) */}
        {stations.map((s) => {
          if (s.latitude == null || s.longitude == null) return null;
          return (
            <Marker
              key={s.stationId || s.id}
              position={[s.latitude, s.longitude]}
              icon={stationIcon}
              eventHandlers={
                role === "volunteer"
                  ? {
                      click: () => updateStationStatus(s.stationId || s.id),
                    }
                  : undefined
              }
            >
              <Popup>{s.navn || "Affaldsstation"}</Popup>
            </Marker>
          );
        })}

        {/* Rapporter som røde cirkler */}
        {reports.map((r) => (
          <CircleMarker
            key={r.id}
            center={[r.lat, r.lng]}
            radius={10}
            color="red"
            fillColor="red"
            fillOpacity={0.5}
          />
        ))}

        {/* Klik handler */}
        <ReportHandler
          armingReport={armingReport}
          setArmingReport={setArmingReport}
          addReport={(r) => setReports([...reports, r])}
          setLastReport={setLastReport}
          canMark={canMark}
        />

        {/* Heatmap */}
        <HeatmapLayer reports={reports} enabled={heatOn} />
      </MapContainer>

      {/* Modal */}
      <Modal
        stall={activeStall}
        onClose={() => setActiveStall(null)}
        onLike={() => activeStall && handleLike(activeStall.id)}
      />

      {/* Dashboard */}
      {role === "organizer" && <Dashboard totals={totals} />}

      {/* Geolocation test section (hidden) */}
      {/*
      <section>
        <h1>useGeolocation</h1>
        <Location />
      </section>
      */}
    </div>
  );
}
