import { useState } from "react";
import {
  Alert,
  Autocomplete,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  Paper,
  TextField,
  Typography,
} from "@mui/material";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import agent from "../../lib/api/agent";
import { useActivityAccess } from "../../lib/hooks/useActivityAccess";
import { errorText, roleLabels, type ManagedUser } from "./shared";
const blank = {
  id: "",
  userName: "",
  email: "",
  name: "",
  role: "Patient",
  password: "",
  disabled: false,
};
export default function UsersPage() {
  const session = useActivityAccess();
  const cache = useQueryClient();
  const [form, setForm] = useState<typeof blank | null>(null);
  const [remove, setRemove] = useState<ManagedUser | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [search, setSearch] = useState<ManagedUser | null>(null);
  const list = useQuery({
    queryKey: ["users", session.version],
    queryFn: async ({ signal }) =>
      (await agent.get<ManagedUser[]>("/user-management", { signal })).data,
  });
  async function run(action: () => Promise<void>) {
    setBusy(true);
    setError("");
    try {
      await action();
      await cache.invalidateQueries({ queryKey: ["users"] });
    } catch (e) {
      setError(errorText(e));
    } finally {
      setBusy(false);
    }
  }
  return (
    <Box sx={{ display: "grid", gap: 2 }}>
      <Typography variant="h4">Felhasználók kezelése</Typography>
      <Alert severity="info">
        A páciens- és kezelőfiókokhoz a megfelelő kezelőoldalon profilt kell
        kapcsolni. A letiltás és a fiókadatok változtatása érvényteleníti a
        korábbi belépést.
      </Alert>
      {error && <Alert severity="error">{error}</Alert>}
      <Button
        variant="contained"
        onClick={() => {
          setError("");
          setForm({ ...blank });
        }}
      >
        Új felhasználó
      </Button>
      {list.isPending && <Typography>Betöltés…</Typography>}
      {list.isError && (
        <Alert severity="error">A felhasználók nem tölthetők be.</Alert>
      )}
      <Autocomplete
        options={list.data ?? []}
        value={search}
        onChange={(_, value) => setSearch(value)}
        getOptionLabel={(u) => `${u.userName} – ${u.name}`}
        isOptionEqualToValue={(a, b) => a.id === b.id}
        sx={{ maxWidth: 520 }}
        renderInput={(params) => (
          <TextField
            {...params}
            label="Felhasználó keresése"
            placeholder="Név vagy felhasználónév"
          />
        )}
      />
      {list.data
        ?.filter((u) => !search || u.id === search.id)
        .map((u) => (
          <Paper sx={{ p: 2 }} key={u.id}>
            <Typography variant="h6">{u.userName}</Typography>
            <Typography>
              {u.email} · {u.roles.map((r) => roleLabels[r]).join(", ")} ·{" "}
              {u.disabled ? "Letiltva" : "Aktív"}
            </Typography>
            <Button
              onClick={() => {
                setError("");
                setForm({
                  id: u.id,
                  userName: u.userName,
                  email: u.email ?? "",
                  name: u.name ?? u.userName,
                  role: u.roles[0] ?? "Patient",
                  password: "",
                  disabled: u.disabled,
                });
              }}
            >
              Szerkesztés
            </Button>
            <Button color="error" onClick={() => setRemove(u)}>
              Törlés
            </Button>
          </Paper>
        ))}
      <Dialog
        open={!!form}
        onClose={() => {
          if (!busy) setForm(null);
        }}
        fullWidth
      >
        <DialogTitle>
          {form?.id ? "Felhasználó szerkesztése" : "Új felhasználó"}
        </DialogTitle>
        <DialogContent>
          {error && <Alert severity="error">{error}</Alert>}
          <Box
            component="form"
            id="user-form"
            sx={{ display: "grid", gap: 2, pt: 1 }}
            onSubmit={(e) => {
              e.preventDefault();
              void run(async () => {
                if (form?.id)
                  await agent.put(`/user-management/${form.id}`, form);
                else await agent.post("/user-management", form);
                setForm(null);
              });
            }}
          >
            {(["userName", "email", "name", "password"] as const).map((key) => (
              <TextField
                key={key}
                label={
                  {
                    userName: "Felhasználónév",
                    email: "Email",
                    name: "Megjelenítési név",
                    password: form?.id
                      ? "Új jelszó (üresen változatlan)"
                      : "Jelszó",
                  }[key]
                }
                required={key !== "password" || !form?.id}
                type={
                  key === "password"
                    ? "password"
                    : key === "email"
                      ? "email"
                      : "text"
                }
                autoComplete={key === "password" ? "new-password" : "off"}
                value={form?.[key] ?? ""}
                onChange={(e) => setForm({ ...form!, [key]: e.target.value })}
              />
            ))}
            <TextField
              select
              label="Jogosultság"
              value={form?.role ?? "Patient"}
              onChange={(e) => setForm({ ...form!, role: e.target.value })}
            >
              {Object.entries(roleLabels).map(([value, label]) => (
                <MenuItem key={value} value={value}>
                  {label}
                </MenuItem>
              ))}
            </TextField>
            <TextField
              select
              label="Fiókállapot"
              value={String(form?.disabled ?? false)}
              onChange={(e) =>
                setForm({ ...form!, disabled: e.target.value === "true" })
              }
            >
              <MenuItem value="false">Aktív</MenuItem>
              <MenuItem value="true">Letiltva</MenuItem>
            </TextField>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button disabled={busy} onClick={() => setForm(null)}>
            Mégse
          </Button>
          <Button disabled={busy} type="submit" form="user-form">
            Mentés
          </Button>
        </DialogActions>
      </Dialog>
      <Dialog
        open={!!remove}
        onClose={() => {
          if (!busy) setRemove(null);
        }}
      >
        <DialogTitle>Felhasználó törlése</DialogTitle>
        <DialogContent>
          {remove?.userName} végleges törlése? Kapcsolt profillal rendelkező
          fióknál használd a letiltást.
          {error && <Alert severity="error">{error}</Alert>}
        </DialogContent>
        <DialogActions>
          <Button disabled={busy} onClick={() => setRemove(null)}>
            Mégse
          </Button>
          <Button
            color="error"
            disabled={busy}
            onClick={() =>
              void run(async () => {
                await agent.delete(`/user-management/${remove!.id}`);
                setRemove(null);
              })
            }
          >
            Törlés
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
