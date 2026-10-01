import { useState } from "react";
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogContent,
  DialogTitle,
  MenuItem,
  Tab,
  Tabs,
  TextField,
} from "@mui/material";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router";
import axios from "axios";
import { changeSession } from "../../lib/api/changeSession";
import DeclarationFields, { type DeclarationAcceptance, type DeclarationPolicy } from "./DeclarationFields";
const emptyAcceptance: DeclarationAcceptance = { accepted: false, version: "", signedName: "" };
export default function AuthDialog({
  open,
  onClose,
}: {
  open: boolean;
  onClose: () => void;
}) {
  const [mode, setMode] = useState(0);
  const [form, setForm] = useState({
    userName: "",
    password: "",
    email: "",
    name: "",
    role: "Patient",
    tajNumber: "",
    birthDate: "",
    specialty: "Orvos",
  });
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState("");
  const [success, setSuccess] = useState(false);
  const [declaration, setDeclaration] = useState(emptyAcceptance);
  const [loginPolicy, setLoginPolicy] = useState<DeclarationPolicy | null>(null);
  const base = (import.meta.env.VITE_API_URL ?? "/api").replace(/\/$/, "");
  const policyQuery = useQuery({
    queryKey: ["registration-declaration", form.role],
    enabled: open && mode === 1,
    queryFn: async () => (await axios.get<DeclarationPolicy>(`${base}/session/declaration`, { params: { role: form.role } })).data,
  });
  const policy = mode === 1 ? policyQuery.data : loginPolicy;
  const declarationReady = !!policy && declaration.accepted && declaration.version === policy.version &&
    (!policy.requiresSignature || declaration.signedName.trim().length >= 3);
  const cache = useQueryClient();
  const navigate = useNavigate();
  const field = (key: keyof typeof form) => ({
    value: form[key],
    onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
      setForm({ ...form, [key]: e.target.value });
      if (key === "role" || key === "userName") {
        setDeclaration(emptyAcceptance);
        setLoginPolicy(null);
      }
    },
    disabled: busy,
  });
  function close() {
    if (!busy) {
      setForm({ ...form, password: "" });
      setMessage("");
      setDeclaration(emptyAcceptance);
      setLoginPolicy(null);
      onClose();
    }
  }
  return (
    <Dialog open={open} onClose={close} fullWidth maxWidth="sm">
      <DialogTitle>Belépés az EgészségÚtba</DialogTitle>
      <DialogContent>
        <Tabs
          value={mode}
          onChange={(_, value: number) => {
            setMode(value);
            setMessage("");
            setDeclaration(emptyAcceptance);
            setLoginPolicy(null);
          }}
          sx={{ mb: 3 }}
        >
          <Tab label="Belépés" disabled={busy} />
          <Tab label="Regisztráció" disabled={busy} />
        </Tabs>
        <Box
          component="form"
          sx={{ display: "grid", gap: 2 }}
          onSubmit={async (e) => {
            e.preventDefault();
            if ((mode === 1 || loginPolicy) && !declarationReady) {
              setSuccess(false);
              setMessage("Olvasd el és fogadd el a nyilatkozatot; dolgozóként a teljes nevedet is add meg.");
              return;
            }
            setBusy(true);
            setMessage("");
            setSuccess(false);
            try {
              const base = (import.meta.env.VITE_API_URL ?? "/api").replace(
                /\/$/,
                "",
              );
              if (mode === 1) {
                await axios.post(`${base}/session/register`, {
                  ...form,
                  declaration,
                  birthDate: form.birthDate || "2000-01-01",
                });
                setMode(0);
                setDeclaration(emptyAcceptance);
                setLoginPolicy(null);
                setForm({ ...form, password: "" });
                setSuccess(true);
                setMessage("A fiók és a profil elkészült. Jelentkezz be.");
              } else {
                await changeSession(
                  cache,
                  async () =>
                    (
                      await axios.post<{ accessToken: string }>(
                        `${base}/session/login`,
                        { userName: form.userName, password: form.password, declaration },
                      )
                    ).data.accessToken,
                );
                setForm({ ...form, password: "" });
                setDeclaration(emptyAcceptance);
                setLoginPolicy(null);
                onClose();
                await navigate("/activities");
                window.location.reload();
              }
            } catch (error) {
              const data = axios.isAxiosError(error)
                ? error.response?.data
                : null;
              if (data?.code === "declaration_required" && data.declaration) {
                setLoginPolicy(data.declaration);
                setDeclaration(emptyAcceptance);
              }
              setMessage(
                data?.message ??
                  (data?.errors
                    ? Object.values(data.errors).flat().join(" ")
                    : "Sikertelen művelet. Ellenőrizd az adatokat."),
              );
            } finally {
              setBusy(false);
            }
          }}
        >
          {message && (
            <Alert severity={success ? "success" : "error"}>{message}</Alert>
          )}
          <TextField
            id="auth-username"
            name="userName"
            label={mode ? "Felhasználónév" : "Felhasználónév vagy email"}
            autoComplete="username"
            required
            {...field("userName")}
          />
          <TextField
            id="auth-password"
            name="password"
            label="Jelszó"
            type="password"
            autoComplete={mode ? "new-password" : "current-password"}
            required
            {...field("password")}
            helperText={
              mode
                ? "Legalább 6 karakter, kis- és nagybetű, szám és speciális karakter."
                : undefined
            }
          />
          {mode === 1 && (
            <>
              <TextField
                id="auth-name"
                name="name"
                label="Teljes név"
                required
                {...field("name")}
              />
              <TextField
                id="auth-email"
                name="email"
                label="Email"
                type="email"
                required
                {...field("email")}
              />
              <TextField
                id="auth-role"
                name="role"
                select
                label="Profil"
                {...field("role")}
              >
                <MenuItem value="Patient">Páciens</MenuItem>
                <MenuItem value="Practitioner">
                  Kezelő (orvos / gyógytornász)
                </MenuItem>
              </TextField>
              <TextField
                id="auth-taj"
                name="tajNumber"
                label="TAJ-szám"
                required
                {...field("tajNumber")}
                helperText="9 számjegy"
              />
              {form.role === "Patient" ? (
                <TextField
                  id="auth-birth-date"
                  name="birthDate"
                  label="Születési dátum"
                  type="date"
                  required
                  slotProps={{ inputLabel: { shrink: true } }}
                  {...field("birthDate")}
                />
              ) : (
                <TextField
                  id="auth-specialty"
                  name="specialty"
                  select
                  label="Szakterület"
                  {...field("specialty")}
                >
                  <MenuItem value="Orvos">Orvos</MenuItem>
                  <MenuItem value="Gyógytornász">Gyógytornász</MenuItem>
                </TextField>
              )}
              <Alert severity="info">
                Admin és felvételi irodai fiókot csak admin hozhat létre.
                Meglévő páciensprofil esetén az admin kapcsolhatja hozzá a
                fiókot.
              </Alert>
            </>
          )}
          {mode === 1 && policyQuery.isPending && <Alert severity="info">Nyilatkozat betöltése…</Alert>}
          {mode === 1 && policyQuery.isError && <Alert severity="error"
            action={<Button onClick={() => void policyQuery.refetch()}>Újra</Button>}>
            A nyilatkozat nem tölthető be. Elfogadása nélkül nem lehet regisztrálni.
          </Alert>}
          {policy && <DeclarationFields key={policy.version} policy={policy} value={declaration}
            onChange={setDeclaration} disabled={busy} />}
          <Button type="submit" variant="contained" disabled={busy || ((mode === 1 || !!loginPolicy) && !declarationReady)}>
            {busy ? "Folyamatban…" : mode ? "Regisztráció" : "Belépés"}
          </Button>
          <Button onClick={close} disabled={busy}>
            Bezárás
          </Button>
        </Box>
      </DialogContent>
    </Dialog>
  );
}
