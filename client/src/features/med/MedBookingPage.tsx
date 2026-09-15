import { useState } from 'react';
import { Alert, Box, Button, Paper, TextField, Typography } from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router';
import agent from '../../lib/api/agent';
import { useActivityAccess } from '../../lib/hooks/useActivityAccess';
import ActivityLogin from '../activities/form/ActivityLogin';
import AppointmentActions from './AppointmentActions';
import { errorText } from '../management/shared';
import PersonSearch from '../../app/shared/components/PersonSearch';
type Person = { id: string; name: string; bookingEnabled?: boolean };
type Appointment = { id: string; startTime: string; activityId: string; status: number; patientId: string; practitionerId: string };
export default function MedBookingPage() {
    const session = useActivityAccess();
    return <Paper sx={{ p: 3, borderRadius: 3 }}>
        <Typography variant="h4" sx={{ mb: 2 }}>Időpontfoglalás</Typography>
        <ActivityLogin />
        {session.authenticated && <BookingEditor key={session.version} version={session.version} />}
    </Paper>;
}
function BookingEditor({ version }: { version: number }) {
    const session = useActivityAccess();
    const cache = useQueryClient();
    const [patientId, setPatientId] = useState('');
    const [practitionerId, setPractitionerId] = useState('');
    const [date, setDate] = useState('');
    const [message, setMessage] = useState('');
    const [now] = useState(() => Date.now());
    const canBook = !!session.data && (session.data.canAssign || session.data.roles.includes('Patient') || !!session.data.ownPractitioner);
    const people = useQuery({ queryKey: ['booking', 'people', version], enabled: session.authenticated, queryFn: async ({ signal }) => {
        const [patients, doctors] = await Promise.all([agent.get<Person[]>('/patients', { signal }), agent.get<Person[]>('/practitioners', { signal })]);
        return { patients: patients.data, doctors: doctors.data };
    }});
    const appointments = useQuery({ queryKey: ['booking', 'appointments', version], enabled: session.authenticated, queryFn: async ({ signal }) => (await agent.get<Appointment[]>('/appointments', { signal })).data });
    const validAppointments = (appointments.data ?? []).filter(
        (appointment): appointment is Appointment => Boolean(appointment?.id && appointment.startTime)
    );
    const statusLabels = ['Rögzítve', 'Lemondva', 'Befejezett', 'Nem jelent meg'];
    const slots = useQuery({ queryKey: ['booking', 'slots', version, practitionerId, date], enabled: session.authenticated && !!date && !!practitionerId && canBook,
        queryFn: async ({ signal }) => (await agent.get<number[]>('/appointments/slots', { params: { practitionerId, date }, signal })).data });
    const selectedPatient = people.data?.patients.some(p => p.id === patientId) ? patientId : (people.data?.patients.length === 1 ? people.data.patients[0].id : '');
    const refresh = async () => { await Promise.all([cache.invalidateQueries({ queryKey: ['booking'] }), cache.invalidateQueries({ queryKey: ['activities'] })]); };
    const book = useMutation({ mutationFn: async (hour: number) => {
        await agent.post('/appointments', { patientId: selectedPatient, practitionerId, date, hour, note: null });
    }, onSuccess: async () => { setMessage('Sikeres foglalás. Az esemény a páciensnél és a kezelőorvosnál is megjelenik.'); await refresh(); }, onError: async (error) => { setMessage(errorText(error)); await slots.refetch(); }});
    const cancel = useMutation({ mutationFn: async (id: string) => { await agent.post(`/appointments/${id}/cancel`); }, onSuccess: refresh });
    const busy = book.isPending || cancel.isPending;
    return <Box sx={{ display: 'grid', gap: 2 }}>
        {people.isError || appointments.isError ? <Alert severity="error">Az adatok nem tölthetők be. Ellenőrizd a belépést.</Alert> : null}
        {message && <Alert severity={book.isError ? 'error' : 'info'}>{message}</Alert>}
        {cancel.isError && <Alert severity="error">A lemondás nem sikerült. Csak jövőbeli foglalás mondható le.</Alert>}
        {canBook ? <Box component="fieldset" disabled={busy || people.isPending} sx={{ display: 'grid', gap: 2, p: 2, border: '1px solid #ddd', borderRadius: 2 }}>
            <legend>Új időpont</legend>
            {session.data?.canAssign || session.data?.isPractitioner ? <PersonSearch label="Páciens teljes neve" options={people.data?.patients ?? []} value={selectedPatient} onChange={setPatientId} /> : <TextField label="Páciens teljes neve" value={people.data?.patients.find(p => p.id === selectedPatient)?.name ?? ''} slotProps={{ input: { readOnly: true } }} />}
            <PersonSearch label="Kezelőorvos keresése" options={people.data?.doctors.map(d => ({ ...d, name: d.name + (d.bookingEnabled ? '' : ' (foglalás nincs engedélyezve)') })) ?? []} value={practitionerId} onChange={id => { setPractitionerId(id); setMessage(''); }} />
            {session.data?.isPractitioner && people.data?.patients.length === 0 && <Alert severity="info">Előbb rendelj magadhoz pácienst a Páciensek kezelése oldalon.</Alert>}
            {people.data && !people.data.doctors.some(d => d.bookingEnabled) && <Alert severity="info">Nincs engedélyezett kezelőorvos. Az adminnak engedélyeznie kell a foglalást, és munkaidőt kell beállítani.</Alert>}
            <TextField id="booking-date" name="date" type="date" label="Dátum" slotProps={{ inputLabel: { shrink: true } }} value={date} onChange={e => { setDate(e.target.value); setMessage(''); }} />
            {slots.isFetching && <Typography>Szabad időpontok betöltése…</Typography>}
            {slots.isError && <Alert severity="error">A szabad időpontok nem tölthetők be.</Alert>}
            {date && practitionerId && slots.data?.length === 0 && <Alert severity="info">Erre a napra nincs szabad, foglalható időpont.</Alert>}
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>{slots.data?.map(hour => <Button key={hour} variant="outlined" disabled={busy || !selectedPatient || slots.isFetching} onClick={() => { setMessage(''); book.mutate(hour); }}>{hour}:00–{hour + 1}:00</Button>)}</Box>
        </Box> : <Alert severity="info">A foglaláshoz megfelelő szerepkör és összekapcsolt páciens- vagy kezelőorvosi profil szükséges.</Alert>}
        <Typography variant="h5">Foglalások</Typography>
        <Button onClick={() => void refresh()} disabled={busy}>Frissítés</Button>
        {appointments.isPending && <Typography>Betöltés…</Typography>}
        {validAppointments.length === 0 && <Typography>Nincs foglalás.</Typography>}
        {validAppointments.map(a => <Paper variant="outlined" key={a.id} sx={{ p: 2 }}>
            <Typography>{a.startTime.replace('T', ' ')} – {statusLabels[a.status] ?? 'Ismeretlen állapot'}</Typography>
            <Typography>{people.data?.patients.find(p => p.id === a.patientId)?.name} · {people.data?.doctors.find(p => p.id === a.practitionerId)?.name}</Typography>
            <Button component={Link} to={`/activities/${a.activityId}`}>Esemény megnyitása</Button>
            <AppointmentActions appointment={a} canMove={!!session.data?.canAssign || !!session.data?.roles.includes('Patient')} canManage={!!session.data && (session.data.canAssign || session.data.isPractitioner)} canDelete={!!session.data?.canAssign} refresh={refresh} />
            {a.status === 0 && new Date(a.startTime).getTime() > now && <Button color="warning" disabled={busy} onClick={() => cancel.mutate(a.id)}>Foglalás lemondása</Button>}
        </Paper>)}
    </Box>;
}

