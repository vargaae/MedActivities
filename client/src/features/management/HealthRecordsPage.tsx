import { useState } from "react";
import {
  Alert,
  Box,
  Button,
  MenuItem,
  Paper,
  TextField,
  Typography,
} from "@mui/material";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router";
import agent from "../../lib/api/agent";
import { useActivityAccess } from "../../lib/hooks/useActivityAccess";
import PatientRecordsPanel from "./PatientRecordsPanel";
import { errorText } from "./shared";
import PersonSearch from '../../app/shared/components/PersonSearch';
import PatientAvatar from '../../app/shared/components/PatientAvatar';

type Patient = {
  id: string;
  name: string;
  tajNumber: string;
  birthDate: string;
  email?: string;
  phone?: string;
  address?: string;
  userId?: string;
};
export default function HealthRecordsPage() {
  const session = useActivityAccess();
  const [id, setId] = useState("");
  const patients = useQuery({
    queryKey: ["health-patients", session.version],
    queryFn: async ({ signal }) =>
      (await agent.get<Patient[]>("/patients", { signal })).data,
  });
  const selected =
    patients.data?.find((p) => p.id === id) ?? patients.data?.[0];
  return (
    <Paper sx={{ p: 3, borderRadius: 3 }}>
      <Typography variant="h4" gutterBottom>
        Adatlapkezelő
      </Typography>
      {patients.isPending && <Typography>Betöltés…</Typography>}
      {patients.isError && (
        <Alert severity="error">Az adatlapok nem tölthetők be.</Alert>
      )}
      {patients.data?.length === 0 && (
        <Alert severity="info">
          Nincs a fiókodhoz kapcsolt vagy számodra hozzáférhető páciens.
        </Alert>
      )}
      {selected && (
        <>
          {session.data?.roles.includes('Admin') ? <Box sx={{ my: 2 }}><PersonSearch label="Páciens keresése" options={patients.data ?? []} value={selected.id} onChange={setId} /></Box> : <TextField
            fullWidth
            select
            label="Páciens"
            value={selected.id}
            onChange={(e) => setId(e.target.value)}
            sx={{ my: 2 }}
          >
            {patients.data?.map((p) => (
              <MenuItem key={p.id} value={p.id}>
                {p.name}
              </MenuItem>
            ))}
          </TextField>}
          <PatientDetails
            key={session.version + selected.id}
            patient={selected}
            editable={
              !!session.data?.canAssign ||
              selected.userId === session.data?.userId
            }
          />
        </>
      )}
    </Paper>
  );
}
function PatientDetails({
  patient,
  editable,
}: {
  patient: Patient;
  editable: boolean;
}) {
  const cache = useQueryClient();
  const [contact, setContact] = useState({
    email: patient.email ?? "",
    phone: patient.phone ?? "",
    address: patient.address ?? "",
  });
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const [saved, setSaved] = useState(false);
  const events = useQuery({
    queryKey: ["health-events", patient.id],
    queryFn: async ({ signal }) =>
      (
        await agent.get<
          { id: string; title: string; date: string; status: string }[]
        >("/patients/" + patient.id + "/activities", { signal })
      ).data,
  });
  return (
    <Box sx={{ display: "grid", gap: 2 }}>
      <PatientAvatar name={patient.name} />
      <Typography>
        {patient.name} · TAJ: {patient.tajNumber} · Született:{" "}
        {patient.birthDate}
      </Typography>
      {error && <Alert severity="error">{error}</Alert>}
      {saved && <Alert severity="success">Az elérhetőségek mentve.</Alert>}
      <Box
        component="form"
        sx={{ display: "grid", gap: 2 }}
        onSubmit={async (e) => {
          e.preventDefault();
          setBusy(true);
          setError("");
          setSaved(false);
          try {
            await agent.put("/patients/" + patient.id + "/contact", {
              email: contact.email || null,
              phone: contact.phone || null,
              address: contact.address || null,
            });
            setSaved(true);
            await cache.invalidateQueries({ queryKey: ["health-patients"] });
          } catch (ex) {
            setError(errorText(ex));
          } finally {
            setBusy(false);
          }
        }}
      >
        <TextField
          label="E-mail"
          type="email"
          value={contact.email}
          disabled={!editable || busy}
          onChange={(e) => setContact({ ...contact, email: e.target.value })}
        />
        <TextField
          label="Telefon"
          value={contact.phone}
          disabled={!editable || busy}
          onChange={(e) => setContact({ ...contact, phone: e.target.value })}
          slotProps={{ htmlInput: { maxLength: 40 } }}
        />
        <TextField
          label="Lakcím"
          value={contact.address}
          disabled={!editable || busy}
          onChange={(e) => setContact({ ...contact, address: e.target.value })}
          slotProps={{ htmlInput: { maxLength: 300 } }}
        />
        {editable && (
          <Button type="submit" disabled={busy}>
            Elérhetőségek mentése
          </Button>
        )}
      </Box>
      <Typography variant="h6">Betegút – események</Typography>
      {events.isError && (
        <Alert severity="error">Az események nem tölthetők be.</Alert>
      )}
      {events.data?.length === 0 && (
        <Typography>Nincs kapcsolt esemény.</Typography>
      )}
      {events.data?.map((a) => (
        <Button
          key={a.id}
          component={Link}
          to={"/activities/" + a.id}
          sx={{ justifyContent: "flex-start" }}
        >
          {a.date.replace("T", " ")} · {a.title}
        </Button>
      ))}
      <PatientRecordsPanel patientId={patient.id} />
    </Box>
  );
}
