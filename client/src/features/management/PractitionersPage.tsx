import { useState } from 'react';
import { Alert, Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Paper, TextField, Typography } from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router';
import agent from '../../lib/api/agent';
import { useActivityAccess } from '../../lib/hooks/useActivityAccess';
import { errorText } from './shared';
type Doctor = { id: string; name: string; tajNumber: string; userId: string; specialty: string; city: string; venue: string; bookingEnabled: boolean };
type Hours = { dayOfWeek: number; isWorkingDay: boolean; startTime: string; endTime: string };
const days = ['Vasárnap','Hétfő','Kedd','Szerda','Csütörtök','Péntek','Szombat'];
const blank: Doctor = { id: '', name: '', tajNumber: '', userId: '', specialty: 'Orvos', city: '', venue: '', bookingEnabled: false };
export default function PractitionersPage() {
    const session = useActivityAccess(); const admin = session.data?.roles.includes('Admin'); const cache = useQueryClient();
    const [form, setForm] = useState<Doctor | null>(null); const [selected, setSelected] = useState<Doctor | null>(null); const [remove, setRemove] = useState<Doctor | null>(null);
    const [hours, setHours] = useState<Hours>({ dayOfWeek: 1, isWorkingDay: true, startTime: '08:00', endTime: '16:00' });
    const [error, setError] = useState(''); const [busy, setBusy] = useState(false); const [search, setSearch] = useState('');
    const list = useQuery({ queryKey: ['practitioners', session.version], queryFn: async ({ signal }) => (await agent.get<Doctor[]>('/practitioners', { signal })).data });
    const users = useQuery({ queryKey: ['practitioner-accounts', session.version], enabled: !!admin, queryFn: async ({ signal }) => (await agent.get<{ id: string; userName: string }[]>('/practitioners/available-accounts', { signal })).data });
    const schedule = useQuery({ queryKey: ['working-hours', selected?.id], enabled: !!selected, queryFn: async ({ signal }) => (await agent.get<Hours[]>(`/practitioners/${selected!.id}/working-hours`, { signal })).data });
    const canSeeEvents = session.data?.canAssign || selected?.id === session.data?.ownPractitioner?.id;
    const events = useQuery({ queryKey: ['doctor-events', session.version, selected?.id], enabled: !!selected && !!canSeeEvents, queryFn: async ({ signal }) => (await agent.get<{ id: string; title: string; date: string }[]>(`/practitioners/${selected!.id}/activities`, { signal })).data });
    async function run(action: () => Promise<void>) { setBusy(true); setError(''); try { await action(); await Promise.all(['practitioner-accounts','practitioners','working-hours','booking','activity-assignment-options','activities'].map(key => cache.invalidateQueries({ queryKey: [key] }))); } catch (e) { setError(errorText(e)); } finally { setBusy(false); } }
    const field = (key: keyof Doctor) => ({ value: form?.[key] ?? '', onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => setForm({ ...form!, [key]: e.target.value }) });
    return <Box sx={{ display: 'grid', gap: 2 }}><Typography variant="h4">Kezelők – orvosok és gyógytornászok</Typography>
        {error && <Alert severity="error">{error}</Alert>}<TextField id="practitioner-search" name="practitionerSearch" label="Keresés név vagy szakterület szerint" value={search} onChange={e => setSearch(e.target.value)} />
        {admin && <Button variant="contained" onClick={() => setForm({ ...blank })}>Új kezelő</Button>}
        {!admin && <Alert severity="info">A kezelők adatait és munkaidejét megtekintheted. Létrehozás, módosítás és törlés csak adminnak engedélyezett.</Alert>}
        {list.isPending && <Typography>Betöltés…</Typography>}{list.isError && <Alert severity="error">A kezelők nem tölthetők be.</Alert>}
        {list.data?.filter(d => `${d.name} ${d.specialty}`.toLocaleLowerCase('hu').includes(search.toLocaleLowerCase('hu'))).map(d => <Paper key={d.id} sx={{ p: 2 }}>
            <Typography variant="h6">{d.name}</Typography><Typography>{d.specialty} · {d.city} · {d.venue}</Typography><Typography>Időpontfoglalás: {d.bookingEnabled ? 'engedélyezett' : 'kikapcsolva'}</Typography>
            <Button onClick={() => { setSelected(d); setError(''); }}>Munkaidő és események</Button>
            {admin && <><Button disabled={busy} onClick={() => void run(async () => { setForm((await agent.get<Doctor>(`/practitioners/${d.id}`)).data); })}>Szerkesztés</Button><Button color="error" onClick={() => setRemove(d)}>Törlés</Button></>}
        </Paper>)}
        <Dialog open={!!form} onClose={() => { if (!busy) setForm(null); }} fullWidth><DialogTitle>{form?.id ? 'Kezelő szerkesztése' : 'Új kezelő'}</DialogTitle><DialogContent>{error && <Alert severity="error">{error}</Alert>}
            <Box component="form" id="doctor-form" sx={{ display: 'grid', gap: 2, pt: 1 }} onSubmit={e => { e.preventDefault(); void run(async () => { if (form?.id) await agent.put(`/practitioners/${form.id}`, form); else await agent.post('/practitioners', form); setForm(null); }); }}>
                <TextField id="auth-name" name="name" label="Teljes név" required {...field('name')} /><TextField id="auth-taj" name="tajNumber" label="TAJ-szám" required {...field('tajNumber')} />
                <TextField select label="Szabad felhasználói fiók" required disabled={!!form?.id} {...field('userId')}>
                    {form?.id && <MenuItem value={form.userId}>A kezelő jelenlegi fiókja</MenuItem>}
                    {users.data?.map(u => <MenuItem key={u.id} value={u.id}>{u.userName}</MenuItem>)}
                </TextField>
                {!form?.id && users.data?.length === 0 && <Alert severity="info">Nincs szabad kezelői fiók. A Felhasználók menüben hozz létre Practitioner-fiókot. A regisztráció már automatikusan létrehozza a kezelőprofilt: azt a listában szerkesztheted.</Alert>}
                <TextField label="Szakterület (orvos / gyógytornász)" required {...field('specialty')} /><TextField label="Város" required {...field('city')} /><TextField label="Rendelő / helyszín" required {...field('venue')} />
            </Box></DialogContent><DialogActions><Button disabled={busy} onClick={() => setForm(null)}>Mégse</Button><Button type="submit" form="doctor-form" disabled={busy}>Mentés</Button></DialogActions></Dialog>
        <Dialog open={!!selected} onClose={() => { if (!busy) setSelected(null); }} fullWidth maxWidth="md"><DialogTitle>{selected?.name}</DialogTitle><DialogContent>
            {error && <Alert severity="error">{error}</Alert>}
            {admin && <Button disabled={busy} onClick={() => void run(async () => { const enabled = !selected!.bookingEnabled; await agent.put(`/practitioners/${selected!.id}/booking-enabled`, { bookingEnabled: enabled }); setSelected({ ...selected!, bookingEnabled: enabled }); })}>{selected?.bookingEnabled ? 'Foglalás kikapcsolása' : 'Foglalás engedélyezése'}</Button>}
            <Typography variant="h6">Heti munkaidő</Typography>
            {schedule.data?.map(w => <Typography key={w.dayOfWeek}>{days[w.dayOfWeek]}: {w.isWorkingDay ? `${w.startTime}–${w.endTime}` : 'Szabadnap'}</Typography>)}
            {schedule.isError && <Alert severity="error">A munkaidő nem tölthető be.</Alert>}
            {admin && <Box sx={{ display: 'grid', gap: 2, my: 2 }}>
                <TextField select label="Nap" value={hours.dayOfWeek} onChange={e => { const dayOfWeek = Number(e.target.value); setHours(schedule.data?.find(w => w.dayOfWeek === dayOfWeek) ?? { dayOfWeek, isWorkingDay: true, startTime: '08:00', endTime: '16:00' }); }}>{days.map((d, i) => <MenuItem key={d} value={i}>{d}</MenuItem>)}</TextField>
                <TextField select label="Munkanap" value={String(hours.isWorkingDay)} onChange={e => setHours({ ...hours, isWorkingDay: e.target.value === 'true' })}><MenuItem value="true">Munkanap</MenuItem><MenuItem value="false">Szabadnap</MenuItem></TextField>
                <TextField label="Kezdés" type="time" value={hours.startTime} onChange={e => setHours({ ...hours, startTime: e.target.value })} /><TextField label="Befejezés" type="time" value={hours.endTime} onChange={e => setHours({ ...hours, endTime: e.target.value })} />
                <Button disabled={busy} onClick={() => void run(async () => { await agent.put(`/practitioners/${selected!.id}/working-hours`, { ...hours, startTime: hours.startTime.length === 5 ? hours.startTime + ':00' : hours.startTime, endTime: hours.endTime.length === 5 ? hours.endTime + ':00' : hours.endTime }); })}>Munkaidő mentése</Button>
            </Box>}
            <Typography variant="h6">Események</Typography>{events.data?.map(a => <Box key={a.id}><Button component={Link} to={`/activities/${a.id}`}>{a.title}</Button><Typography>{a.date.replace('T',' ')}</Typography></Box>)}
            {events.data?.length === 0 && <Typography>Nincs kapcsolt esemény.</Typography>}{events.isError && <Alert severity="error">Az események nem tölthetők be.</Alert>}
        </DialogContent><DialogActions><Button disabled={busy} onClick={() => setSelected(null)}>Bezárás</Button></DialogActions></Dialog>
        <Dialog open={!!remove} onClose={() => { if (!busy) setRemove(null); }}><DialogTitle>Kezelő törlése</DialogTitle><DialogContent>{remove?.name} törlése? Kapcsolt esemény vagy foglalás esetén a rendszer elutasítja a törlést.{error && <Alert severity="error">{error}</Alert>}</DialogContent><DialogActions><Button disabled={busy} onClick={() => setRemove(null)}>Mégse</Button><Button disabled={busy} color="error" onClick={() => void run(async () => { await agent.delete(`/practitioners/${remove!.id}`); setRemove(null); })}>Törlés</Button></DialogActions></Dialog>
    </Box>;
}

