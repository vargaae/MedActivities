import { useState } from 'react';
import { Alert, Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Paper, TextField, Typography } from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router';
import agent from '../../lib/api/agent';
import { useActivityAccess } from '../../lib/hooks/useActivityAccess';
import { errorText, type ManagedUser } from './shared';
import PatientRecordsPanel from './PatientRecordsPanel';
import PatientAvatar from '../../app/shared/components/PatientAvatar';
type Patient = { id: string; name: string; tajNumber: string; birthDate: string; email: string; phone: string; address: string; notes: string; userId?: string };
type EventItem = { id: string; title: string; date: string; status: string };
const blank = { id: '', name: '', tajNumber: '', birthDate: '', email: '', phone: '', address: '', notes: '' };
export default function PatientsPage() {
    const session = useActivityAccess(); const cache = useQueryClient(); const admin = session.data?.roles.includes('Admin');
    const staff = !!session.data?.canAssign;
    const [form, setForm] = useState<Patient | null>(null); const [selected, setSelected] = useState<Patient | null>(null); const [remove, setRemove] = useState<Patient | null>(null);
    const [error, setError] = useState(''); const [busy, setBusy] = useState(false); const [search, setSearch] = useState(''); const [userId, setUserId] = useState(''); const [doctorId, setDoctorId] = useState('');
    const list = useQuery({ queryKey: ['patients', session.version], queryFn: async ({ signal }) => (await agent.get<Patient[]>('/patients', { signal })).data });
    const users = useQuery({ queryKey: ['users', session.version], enabled: !!admin, queryFn: async ({ signal }) => (await agent.get<ManagedUser[]>('/user-management', { signal })).data });
    const doctors = useQuery({ queryKey: ['practitioners', session.version], queryFn: async ({ signal }) => (await agent.get<{ id: string; name: string }[]>('/practitioners', { signal })).data });
    const grants = useQuery({ queryKey: ['patient-access', session.version, selected?.id], enabled: !!selected && staff, queryFn: async ({ signal }) => (await agent.get<{ id: string; name: string }[]>(`/patients/${selected!.id}/access`, { signal })).data });
    const events = useQuery({ queryKey: ['patient-events', session.version, selected?.id], enabled: !!selected, queryFn: async ({ signal }) => (await agent.get<EventItem[]>(`/patients/${selected!.id}/activities`, { signal })).data });
    async function refresh() { await Promise.all(['patients','patient-access','activities','activity-assignment-options','booking'].map(key => cache.invalidateQueries({ queryKey: [key] }))); }
    async function run(action: () => Promise<void>) { setBusy(true); setError(''); try { await action(); await refresh(); } catch (e) { setError(errorText(e)); } finally { setBusy(false); } }
    const field = (key: keyof Patient) => ({ value: form?.[key] ?? '', onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => setForm({ ...form!, [key]: e.target.value }) });
    return <Box sx={{ display: 'grid', gap: 2 }}>
        <Typography variant="h4">Páciensek kezelése</Typography>{error && <Alert severity="error">{error}</Alert>}
        <Box sx={{ display: 'flex', gap: 2 }}><TextField id="patient-search" name="patientSearch" label="Keresés név szerint" value={search} onChange={e => setSearch(e.target.value)} />{staff && <Button variant="contained" onClick={() => { setError(''); setForm({ ...blank }); }}>Új páciens</Button>}</Box>
        {!staff && <Alert severity="info">A hozzád rendelt páciensek adatait láthatod; megjegyzéseiket és dokumentumaikat az adatlapon kezelheted.</Alert>}
        {list.isPending && <Typography>Betöltés…</Typography>}{list.isError && <Alert severity="error">A pácienslista nem tölthető be.</Alert>}
        {list.data?.filter(p => p.name.toLocaleLowerCase('hu').includes(search.toLocaleLowerCase('hu'))).map(p => <Paper key={p.id} sx={{ p: 2 }}>
            <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 1 }}><PatientAvatar name={p.name} /><Typography variant="h6">{p.name}</Typography></Box><Typography>TAJ: {p.tajNumber} · Született: {p.birthDate}</Typography>
            <Button onClick={() => { setSelected(p); setUserId(p.userId ?? ''); }}>Adatlap és események</Button>{staff && <><Button onClick={() => { setError(''); setForm({ ...p }); }}>Szerkesztés</Button><Button color="error" onClick={() => setRemove(p)}>Törlés</Button></>}
        </Paper>)}
        {list.data?.length === 0 && <Typography>Nincs páciens.</Typography>}
        <Dialog open={!!form} onClose={() => { if (!busy) setForm(null); }} fullWidth><DialogTitle>{form?.id ? 'Páciens szerkesztése' : 'Új páciens'}</DialogTitle><DialogContent>
            {error && <Alert severity="error">{error}</Alert>}<Box component="form" id="patient-form" sx={{ display: 'grid', gap: 2, pt: 1 }} onSubmit={e => { e.preventDefault(); void run(async () => {
                const body = { ...form, email: form?.email || null, phone: form?.phone || null, address: form?.address || null, notes: form?.notes || null };
                if (form?.id) await agent.put(`/patients/${form.id}`, body); else await agent.post('/patients', body); setForm(null);
            }); }}>
                <TextField id="auth-name" name="name" label="Teljes név" required {...field('name')} /><TextField id="auth-taj" name="tajNumber" label="TAJ-szám" required {...field('tajNumber')} helperText="9 számjegy" />
                <TextField id="auth-birth-date" name="birthDate" label="Születési dátum" type="date" required slotProps={{ inputLabel: { shrink: true } }} {...field('birthDate')} />
                <TextField id="auth-email" name="email" label="Email" type="email" {...field('email')} /><TextField label="Telefon" {...field('phone')} /><TextField label="Lakcím" {...field('address')} /><TextField label="Megjegyzés" multiline {...field('notes')} />
            </Box></DialogContent><DialogActions><Button disabled={busy} onClick={() => setForm(null)}>Mégse</Button><Button type="submit" form="patient-form" disabled={busy}>Mentés</Button></DialogActions></Dialog>
        <Dialog open={!!selected} onClose={() => setSelected(null)} fullWidth maxWidth="md"><DialogTitle>{selected?.name} – adatlap</DialogTitle><DialogContent>
            <Typography>{selected?.email} · {selected?.phone}</Typography><Typography>{selected?.address}</Typography><Typography>{selected?.notes}</Typography>
            {admin && <Box sx={{ display: 'flex', gap: 2, my: 2 }}><TextField fullWidth select label="Páciens felhasználói fiókja" value={userId} onChange={e => setUserId(e.target.value)}><MenuItem value="">Válassz fiókot</MenuItem>{users.data?.filter(u => u.roles.includes('Patient')).map(u => <MenuItem key={u.id} value={u.id}>{u.userName}</MenuItem>)}</TextField><Button disabled={busy || !userId} onClick={() => void run(async () => { await agent.put(`/patients/${selected!.id}/user`, { userId }); setSelected({ ...selected!, userId }); })}>Összekapcsolás</Button></Box>}
            {error && <Alert severity="error">{error}</Alert>}{staff && <><Typography variant="h6">Hozzáférő kezelők</Typography>
            {grants.data?.map(d => <Box key={d.id}>{d.name}<Button disabled={busy} onClick={() => void run(async () => { await agent.delete(`/patients/${selected!.id}/access/${d.id}`); })}>Hozzáférés visszavonása</Button></Box>)}
            <Box sx={{ display: 'flex', gap: 2, my: 2 }}><TextField fullWidth select label="Kezelő hozzáférése" value={doctorId} onChange={e => setDoctorId(e.target.value)}>{doctors.data?.map(d => <MenuItem key={d.id} value={d.id}>{d.name}</MenuItem>)}</TextField><Button disabled={busy || !doctorId} onClick={() => void run(async () => { await agent.put(`/patients/${selected!.id}/access/${doctorId}`); setDoctorId(''); })}>Hozzárendelés</Button></Box>
            </>}<Typography variant="h6">Események</Typography>
            {events.isPending && <Typography>Betöltés…</Typography>}{events.isError && <Alert severity="error">Az események nem tölthetők be.</Alert>}
            {events.data?.map(a => <Box key={a.id} sx={{ py: 1 }}><Button component={Link} to={`/activities/${a.id}`}>{a.title}</Button><Typography>{a.date.replace('T', ' ')} · {a.status}</Typography></Box>)}
            {events.data?.length === 0 && <Typography>Nincs kapcsolt esemény.</Typography>}
            {selected && <PatientRecordsPanel key={session.version + selected.id} patientId={selected.id} />}
        </DialogContent><DialogActions><Button onClick={() => setSelected(null)}>Bezárás</Button></DialogActions></Dialog>
        <Dialog open={!!remove} onClose={() => { if (!busy) setRemove(null); }}><DialogTitle>Páciens törlése</DialogTitle><DialogContent>{remove?.name} törlése? Eseményhez vagy foglaláshoz kapcsolt páciens nem törölhető.{error && <Alert severity="error">{error}</Alert>}</DialogContent><DialogActions><Button disabled={busy} onClick={() => setRemove(null)}>Mégse</Button><Button color="error" disabled={busy} onClick={() => void run(async () => { await agent.delete(`/patients/${remove!.id}`); setRemove(null); })}>Törlés</Button></DialogActions></Dialog>
    </Box>;
}


