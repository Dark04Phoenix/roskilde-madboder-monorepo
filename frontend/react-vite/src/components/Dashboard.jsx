export default function Dashboard({ totals }) {
  if (!totals) return null;

  return (
    <div className="dash">
      <h3>📊 Festival status</h3>
      <div><b>Gns. CO₂/ret:</b> {totals.avgCo2} kg (mål 0.75 kg)</div>
      <div><b>Under CO₂-mål:</b> {totals.underGoal}/{totals.count}</div>
      <div><b>Øko (gns.):</b> {totals.avgOrganic}%</div>
      <div><b>Boder:</b> {totals.count}</div>
      <div><b>Rapporter:</b> {totals.reports}</div>
    </div>
  );
}
