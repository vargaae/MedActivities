import { Alert, Autocomplete, Box, Button, Paper, TextField, Typography } from '@mui/material';
import { useActivities } from '../../../lib/hooks/useActivities';
import { useActivityAccess } from '../../../lib/hooks/useActivityAccess';
import { useNavigate, useParams } from 'react-router';
import { useForm } from 'react-hook-form';
import { useEffect, useState } from 'react';
import { activitySchema, type ActivitySchema } from '../../../lib/schemas/activitySchema';
import { zodResolver } from '@hookform/resolvers/zod';
import TextInput from '../../../app/shared/components/TextInput';
import SelectInput from '../../../app/shared/components/SelectInput';
import DateTimeInput from '../../../app/shared/components/DateTimeInput';
import LocationInput from '../../../app/shared/components/LocationInput';
import { categoryOptions } from './categoryOptions';
import ActivityPeople from '../details/ActivityPeople';
import ActivityLogin from './ActivityLogin';

export default function ActivityForm() {
    const { id } = useParams();
    const access = useActivityAccess();
    return <Paper sx={{ borderRadius: 3, padding: 3 }}>
        <ActivityLogin />
        {access.authenticated && access.isLoading && <Typography>Jogosultságok betöltése…</Typography>}
        {access.authenticated && access.data && <ActivityEditor key={`${id ?? 'new'}-${access.version}`} />}
    </Paper>;
}

function ActivityEditor() {
    const { id } = useParams();
    const navigate = useNavigate();
    const { data: options } = useActivityAccess();
    const { updateActivity, createActivity, activity, isLoadingActivity } = useActivities(id);
    const { control, reset, handleSubmit } = useForm<ActivitySchema>({ mode: 'onTouched', resolver: zodResolver(activitySchema) });
    const [patient, setPatient] = useState<ActivityPerson | null>(null);
    const [doctors, setDoctors] = useState<ActivityPerson[]>([]);
    const [error, setError] = useState('');
    const [status, setStatus] = useState('Scheduled');
    const canAssign = !!options?.canAssign;
    const choosePatient = canAssign || (!id && !!options?.isPractitioner);
    const canSave = id ? !!activity?.canEditFields : !!options?.canCreate;

    useEffect(() => {
        if (activity) {
            reset({ ...activity, location: { city: activity.city, venue: activity.venue, latitude: activity.latitude, longitude: activity.longitude } });
            setPatient(activity.patients.length === 1 ? activity.patients[0] : null);
            setDoctors(activity.practitioners); setStatus(activity.status);
        }
    }, [activity, reset]);

    async function submit(data: ActivitySchema) {
        setError('');
        if (choosePatient && !patient) { setError('Válassz pontosan egy pácienst.'); return; }
        if (canAssign && doctors.length === 0) { setError('Válassz legalább egy kezelőorvost.'); return; }
        const { location, ...rest } = data;
        const body: ActivityWrite = {
            title: rest.title, description: rest.description, category: rest.category,
            date: new Date(rest.date as string | number | Date), status,
            city: location.city ?? '', venue: location.venue,
            latitude: Number(location.latitude), longitude: Number(location.longitude),
            ...(id ? { id } : {}),
            ...(choosePatient ? { patientId: patient!.id } : {}),
            ...(canAssign ? { practitionerIds: doctors.map(d => d.id) } : {})
        };
        try {
            if (id) { await updateActivity.mutateAsync(body); await navigate(`/activities/${id}`); }
            else { const createdId = await createActivity.mutateAsync(body); await navigate(`/activities/${createdId}`); }
        } catch { setError('A mentés nem sikerült. Ellenőrizd a mezőket és a jogosultságot.'); }
    }

    if (id && isLoadingActivity) return <Typography>Esemény betöltése…</Typography>;
    if (id && !activity) return <Alert severity="error">Az esemény nem elérhető.</Alert>;
    if (!id && !options?.canCreate) return <Alert severity="info">Páciens szerepkörrel itt nem hozható létre esemény. Időpontot a foglalóban kérhetsz.</Alert>;
    return <>
        <Typography variant="h5" sx={{ mb: 2 }}>{id ? 'Esemény módosítása' : 'Új esemény'}</Typography>
        {activity?.isAppointment && <Alert severity="info">Foglalási esemény: a dátum és státusz az időpontfoglalással együtt változik. Egész órát válassz, 08:00–19:00 kezdettel; a rendszer ellenőrzi a munkaidőt és az ütközéseket.</Alert>}
        {!canAssign && activity && <ActivityPeople activity={activity} />}
        {canAssign && activity && activity.patients.length > 1 && <Alert severity="warning">A régi eseményhez több páciens tartozik. Mentés előtt válassz ki egyet.</Alert>}
        {error && <Alert severity="error">{error}</Alert>}
        <Box component="form" onSubmit={handleSubmit(submit)} sx={{ mt: 2 }}>
            <Box component="fieldset" disabled={!canSave || updateActivity.isPending || createActivity.isPending} sx={{ border: 0, p: 0, m: 0, display: 'grid', gap: 3 }}>
                {choosePatient && <Autocomplete
                    options={options?.patients ?? []} value={patient} onChange={(_, value) => setPatient(value)}
                    getOptionLabel={p => p.name} isOptionEqualToValue={(a, b) => a.id === b.id}
                    renderInput={params => <TextField {...params} label="Páciens (kötelező)" />}
                />}
                {choosePatient && options?.patients.length === 0 && <Alert severity="info">Nincs választható páciens. Az admin vagy a felvételi iroda adhat hozzáférést.</Alert>}
                {canAssign ? <Autocomplete multiple
                    options={options?.practitioners ?? []} value={doctors} onChange={(_, value) => setDoctors(value)}
                    getOptionLabel={p => p.name} isOptionEqualToValue={(a, b) => a.id === b.id}
                    renderInput={params => <TextField {...params} label="Kezelőorvosok (legalább egy)" />}
                /> : !id && <Typography>Kezelőorvos: {options?.ownPractitioner?.name} (automatikus hozzárendelés)</Typography>}
                <TextInput label="Cím" control={control} name="title" />
                <TextInput label="Leírás" control={control} name="description" multiline rows={3} />
                <SelectInput items={categoryOptions} label="Kategória" name="category" control={control} />
                <DateTimeInput label="Dátum" name="date" control={control} />
                <TextField select label="Státusz" value={status} onChange={e => setStatus(e.target.value)} slotProps={{ select: { native: true } }}><option value="Scheduled">Tervezett</option><option value="Cancelled">Lemondva</option><option value="Completed">Befejezett</option><option value="NoShow">Nem jelent meg</option></TextField>
                <LocationInput label="Helyszín" name="location" control={control} />
                {canSave && <Button type="submit" variant="contained" loading={updateActivity.isPending || createActivity.isPending}>Mentés</Button>}
            </Box>
            <Button sx={{ mt: 2 }} onClick={() => void navigate(id ? `/activities/${id}` : '/activities')}>Vissza</Button>
        </Box>
    </>;
}
