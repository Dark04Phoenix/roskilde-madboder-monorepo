export default function Topbar({
  role,
  setRole,
  heatOn,
  setHeatOn,
  armingReport,
  setArmingReport,
  lastReport,
  removeLastReport,
  exportReports,
  clearAllReports,
  canMark,
}) {
  return (
    <header className="topbar">
      <strong>ReCirkle</strong>
      <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
        <label>Rolle:</label>
        <select value={role} onChange={(e) => setRole(e.target.value)}>
          <option value="guest">Gæst</option>
          <option value="volunteer">Frivillig</option>
          <option value="organizer">Arrangør</option>
        </select>

        {role === "volunteer" && (
          <button onClick={() => setHeatOn(!heatOn)}>
            Heatmap: {heatOn ? "On" : "Off"}
          </button>
        )}

        {canMark && (
          <button onClick={() => setArmingReport(true)}>Markér skrald</button>
        )}

        {lastReport && canMark && (
          <button onClick={removeLastReport}>Fortryd markering</button>
        )}

        {canMark && <button onClick={clearAllReports}>Ryd alle markeringer</button>}

        {role === "organizer" && (
          <button onClick={exportReports}>Eksportér rapporter</button>
        )}
      </div>
    </header>
  );
}
