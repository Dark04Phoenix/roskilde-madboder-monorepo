export function loadReports() {
  try {
    return JSON.parse(localStorage.getItem("gronkilde_reports") || "[]");
  } catch {
    return [];
  }
}

export function saveReports(arr) {
  localStorage.setItem("gronkilde_reports", JSON.stringify(arr));
}

export function addLocalReport(lat, lng) {
  const id = `${Date.now()}-${Math.random().toString(36).slice(2, 7)}`;
  const arr = loadReports();
  const rpt = { id, lat, lng, t: Date.now() };
  arr.push(rpt);
  saveReports(arr);
  return rpt;
}

export function removeReportById(id) {
  const arr = loadReports().filter((r) => r.id !== id);
  saveReports(arr);
}
