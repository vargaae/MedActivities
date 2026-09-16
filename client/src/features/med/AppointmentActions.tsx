import { useState } from "react";
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  TextField,
} from "@mui/material";
import agent from "../../lib/api/agent";

type Props = {
  appointment: { id: string; startTime: string; status: number };
  canMove: boolean;
  canManage: boolean;
  canDelete: boolean;
  refresh: () => Promise<void>;
};
export default function AppointmentActions({
  appointment: a,
  canMove,
  canManage,
  canDelete,
  refresh,
}: Props) {
  const [mode, setMode] = useState("");
  const [date, setDate] = useState(a.startTime.slice(0, 10));
  const [hour, setHour] = useState(Number(a.startTime.slice(11, 13)));
  const [status, setStatus] = useState("Completed");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const open = (value: string) => {
    setMode(value);
    setError("");
  };
  const save = async () => {
    setBusy(true);
    setError("");
    try {
      if (mode === "move")
        await agent.put(`/appointments/${a.id}`, { date, hour });
      if (mode === "status")
        await agent.put(`/appointments/${a.id}/status`, { status });
      if (mode === "delete") await agent.delete(`/appointments/${a.id}`);
      await refresh();
      setMode("");
    } catch {
      setError(
        "A művelet nem sikerült. Ellenőrizd a jogosultságot, a munkaidőt és az időpontütközést.",
      );
    } finally {
      setBusy(false);
    }
  };
  return (
    <>
      {canMove &&
        a.status === 0 &&
        new Date(a.startTime).getTime() > Date.now() && (
          <Button onClick={() => open("move")}>Átfoglalás</Button>
        )}
      {canManage && <Button onClick={() => open("status")}>Státusz</Button>}
      {canDelete && (
        <Button color="error" onClick={() => open("delete")}>
          Törlés
        </Button>
      )}
      <Dialog
        open={!!mode}
        onClose={() => {
          if (!busy) setMode("");
        }}
        fullWidth
        maxWidth="xs"
      >
        <DialogTitle>
          {mode === "move"
            ? "Időpont módosítása"
            : mode === "status"
              ? "Foglalás státusza"
              : "Foglalás törlése"}
        </DialogTitle>
        <DialogContent sx={{ display: "grid", gap: 2, pt: "12px !important" }}>
          {error && <Alert severity="error">{error}</Alert>}
          {mode === "move" && (
            <>
              <TextField
                type="date"
                label="Új dátum"
                value={date}
                onChange={(e) => setDate(e.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
              />
              <TextField
                select
                label="Kezdő óra"
                value={hour}
                onChange={(e) => setHour(Number(e.target.value))}
              >
                {Array.from({ length: 12 }, (_, i) => i + 8).map((h) => (
                  <MenuItem key={h} value={h}>
                    {h}:00
                  </MenuItem>
                ))}
              </TextField>
              <Alert severity="info">
                Mentéskor a szerver újra ellenőrzi a szabad időpontot és a napi
                korlátot.
              </Alert>
            </>
          )}
          {mode === "status" && (
            <TextField
              select
              label="Állapot"
              value={status}
              onChange={(e) => setStatus(e.target.value)}
            >
              <MenuItem value="Scheduled">Rögzítve</MenuItem>
              <MenuItem value="Cancelled">Lemondva</MenuItem>
              <MenuItem value="Completed">Befejezett</MenuItem>
              <MenuItem value="NoShow">Nem jelent meg</MenuItem>
            </TextField>
          )}
          {mode === "delete" && (
            <Alert severity="warning">
              A foglalás és a hozzá tartozó esemény végleg törlődik. A lemondás
              megőrzi az előzményt.
            </Alert>
          )}
        </DialogContent>
        <DialogActions>
          <Button disabled={busy} onClick={() => setMode("")}>
            Mégse
          </Button>
          <Button
            disabled={busy || (mode === "move" && !date)}
            onClick={() => void save()}
          >
            Megerősítés
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
