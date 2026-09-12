import { useState } from "react";
type Patient = { id: string; name: string };
type Practitioner = { id: string; name: string; bookingEnabled: boolean };
type Appointment = { id: string; startTime: string; activityId: string; status: number };
export default function MedBookingPage({ apiBase = "" }: { apiBase?: string }) {
  const [token, setToken] = useState(""); const [email, setEmail] = useState(""); const [password, setPassword] = useState("");
  const [patients, setPatients] = useState<Patient[]>([]); const [doctors, setDoctors] = useState<Practitioner[]>([]);
  const [patientId, setPatientId] = useState(""); const [practitionerId, setPractitionerId] = useState("");
  const [date, setDate] = useState(""); const [slots, setSlots] = useState<number[]>([]); const [items, setItems] = useState<Appointment[]>([]);
  const [message, setMessage] = useState(""); const [busy, setBusy] = useState(false);
  async function request(path: string, method = "GET", body?: unknown, auth = token) {
    const response = await fetch(apiBase + path, { method, headers: { "Content-Type": "application/json", ...(auth ? { Authorization: `Bearer ${auth}` } : {}) }, ...(body === undefined ? {} : { body: JSON.stringify(body) }) });
    if (!response.ok) throw new Error(`${response.status}: ${await response.text()}`);
    return response.status === 204 ? undefined : response.json();
  }
  async function run(action: () => Promise<void>) { setBusy(true); setMessage(""); try { await action(); } catch (error) { setMessage(error instanceof Error ? error.message : "Hiba történt."); } finally { setBusy(false); } }
  async function load(auth = token) { setPatients(await request("/api/patients", "GET", undefined, auth)); setDoctors(await request("/api/practitioners", "GET", undefined, auth)); setItems(await request("/api/appointments", "GET", undefined, auth)); }
  async function refreshSlots() { setSlots(await request(`/api/appointments/slots?practitionerId=${encodeURIComponent(practitionerId)}&date=${date}`)); }
  return <main style={{ maxWidth: 760, margin: "2rem auto", fontFamily: "sans-serif" }}>
    <h1>EgészségÚt – időpontfoglalás</h1><p role="status">{message}</p>
    {!token ? <form onSubmit={e => { e.preventDefault(); void run(async () => { const result = await request("/api/auth/login?useCookies=false", "POST", { email, password }); setToken(result.accessToken); setPassword(""); await load(result.accessToken); }); }}>
      <label>Email <input type="email" required value={email} onChange={e => setEmail(e.target.value)} /></label>{" "}
      <label>Jelszó <input type="password" required value={password} onChange={e => setPassword(e.target.value)} /></label><button disabled={busy}>Belépés</button>
    </form> : <>
      <button onClick={() => { setToken(""); setPatients([]); setDoctors([]); setItems([]); setSlots([]); setPatientId(""); setPractitionerId(""); }}>Kilépés az oldalról</button>
      <fieldset disabled={busy}><legend>Új időpont</legend>
        <label>Páciens <select value={patientId} onChange={e => setPatientId(e.target.value)}><option value="">Válassz</option>{patients.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}</select></label>{" "}
        <label>Kezelő <select value={practitionerId} onChange={e => { setPractitionerId(e.target.value); setSlots([]); }}><option value="">Válassz</option>{doctors.filter(d => d.bookingEnabled).map(d => <option key={d.id} value={d.id}>{d.name}</option>)}</select></label>{" "}
        <label>Dátum <input type="date" value={date} onChange={e => { setDate(e.target.value); setSlots([]); }} /></label>
        <button disabled={!date || !practitionerId} onClick={() => void run(async () => { await refreshSlots(); setMessage("A választható órák lent jelennek meg. Üres lista: nincs szabad időpont."); })}>Szabad időpontok</button>
        <div>{slots.map(hour => <button key={hour} disabled={!patientId} onClick={() => void run(async () => { await request("/api/appointments", "POST", { patientId, practitionerId, date, hour, note: null }); await load(); await refreshSlots(); setMessage("Sikeres foglalás."); })}>{hour}:00</button>)}</div>
      </fieldset>
      <h2>Foglalásaim</h2><button disabled={busy} onClick={() => void run(() => load())}>Frissítés</button>
      <ul>{items.map(a => <li key={a.id}>{a.startTime.replace("T", " ")} – {a.status === 1 ? "Lemondva" : "Rögzítve"} {a.status === 0 && <button disabled={busy} onClick={() => void run(async () => { await request(`/api/appointments/${a.id}/cancel`, "POST"); await load(); if (date && practitionerId) await refreshSlots(); })}>Lemondás</button>}</li>)}</ul>
    </>}
  </main>;
}