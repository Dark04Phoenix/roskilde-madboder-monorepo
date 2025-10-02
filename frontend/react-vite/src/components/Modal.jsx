// ---- Modal til stall ----
function Modal({ stall, onClose, onLike }) {
  if (!stall) return null;

  const organicWidth = Math.min(100, Math.max(0, stall.organicPct));
  const localWidth = Math.min(100, Math.max(0, stall.localPct));
  const meetsCo2 = stall.co2PerMealKg <= 0.75;

  return (
    <div className="modal">
      <div className="modal-body">
        <button className="close" onClick={onClose}>×</button>
        <h2>{stall.name}</h2>

        <div className="info-grid">
          <div><b>Madtype:</b> {stall.foodType}</div>
          <div><b>Energi:</b> {stall.energy}</div>
          <div>
            <b>CO₂ pr. måltid:</b> {stall.co2PerMealKg} kg 
            <span className="small"> (mål: 0.75 kg {meetsCo2 ? "✅" : "⚠️"})</span>
          </div>
          <div><b>Økologi:</b>
            <div className="progress"><span style={{width: organicWidth + "%"}}></span></div>
            <div className="small">{stall.organicPct}%</div>
          </div>
          <div><b>Lokal andel:</b>
            <div className="progress"><span style={{width: localWidth + "%"}}></span></div>
            <div className="small">{stall.localPct}%</div>
          </div>
        </div>

        <b>Affaldssortering:</b>
        <div className="badges">
          {stall.wasteFractions.map((f, i) => (
            <span key={i} className="badge-chip">{f}</span>
          ))}
        </div>

        <div className="flow">
          <div className="step">🍔 Mad</div><div className="sep">➜</div>
          <div className="step">🗑️ Affald</div><div className="sep">➜</div>
          <div className="step">🔄 Biogas/Genbrug</div><div className="sep">➜</div>
          <div className="step">🔋 Energi</div><div className="sep">➜</div>
          <div className="step">🎶 Festival</div>
        </div>

        <div className="actions">
          <button 
            className="like" 
            onClick={onLike} 
            disabled={stall._liked}
          >
            {stall._liked ? "👍 Liked" : "👍 Like"}
          </button>
          <span>{stall.likes}</span>
        </div>
      </div>
    </div>
  );
}

// 👇 DETTE manglede
export default Modal;
