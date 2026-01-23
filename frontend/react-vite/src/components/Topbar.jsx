import { useState } from "react";

export default function Topbar({
  role,
  setRole,
  user,
  setUser,
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
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const roleLabel = role === "volunteer" ? "Frivillig" : role === "organizer" ? "Arrangør" : "Gæst";

  async function handleLogin(e) {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      const res = await fetch("http://localhost:5013/api/Login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
      });
      if (res.status === 401) {
        setError("Forkert email eller password.");
        setUser(null);
        setRole("guest");
        return;
      }
      if (!res.ok) {
        setError("Login fejlede. Prøv igen.");
        setUser(null);
        setRole("guest");
        return;
      }
      const data = await res.json();
      setUser(data);
      setRole(roleFromBackend(data.rolle));
      setPassword("");
    } catch (err) {
      setError("Netværksfejl. Prøv igen.");
      setUser(null);
      setRole("guest");
    } finally {
      setLoading(false);
    }
  }

  function roleFromBackend(r) {
    const lower = (r || "").toLowerCase();
    if (lower === "frivillig") return "volunteer";
    if (lower === "arrangør" || lower === "arrangoer" || lower === "arrangor") return "organizer";
    return "guest";
  }

  function handleLogout() {
    setUser(null);
    setRole("guest");
    setEmail("");
    setPassword("");
    setError("");
  }

  return (
    <header className="topbar">
      <strong>ReCirkle</strong>
      <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
        {!user ? (
          <>
            <form onSubmit={handleLogin} style={{ display: "flex", gap: "6px", alignItems: "center" }}>
              <input
                type="email"
                placeholder="Email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                style={{ padding: "4px 8px" }}
              />
              <input
                type="password"
                placeholder="Password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                style={{ padding: "4px 8px" }}
              />
              <button type="submit" disabled={loading}>
                {loading ? "Logger in..." : "Login"}
              </button>
            </form>
            {error && <span style={{ color: "red", fontSize: "0.9em" }}>{error}</span>}
          </>
        ) : (
          <div style={{ display: "flex", gap: "8px", alignItems: "center" }}>
            <span>
              {user.navn} ({roleLabel})
            </span>
            <button onClick={handleLogout}>Logout</button>
          </div>
        )}

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
