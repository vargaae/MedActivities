import { Alert, Box, Button, Chip, Dialog, DialogActions, DialogContent, DialogTitle, Paper, Typography } from '@mui/material';
import { Link, useNavigate } from 'react-router';
import { useState } from 'react';
import { useActivities } from '../../../lib/hooks/useActivities';
import { formatDate } from '../../../lib/util/util';
import { categoryImage } from '../dashboard/categoryImage';

export default function ActivityDetailsHeader({ activity }: { activity: Activity }) {
    const status =
      (
        {
          Scheduled: "Tervezett",
          Completed: "Befejezett",
          Cancelled: "Lemondva",
          NoShow: "Nem jelent meg",
        } as Record<string, string>
      )[activity.status] ?? activity.status;
    const { deleteActivity } = useActivities(activity.id);
    const navigate = useNavigate();
    const [confirm, setConfirm] = useState(false);
    const [error, setError] = useState('');
    async function remove() {
        setError('');
        try { await deleteActivity.mutateAsync(activity.id); await navigate('/activities', { replace: true }); }
        catch { setError('A törlés nem sikerült. Ellenőrizd a jogosultságot és próbáld újra.'); }
    }
    const image = categoryImage(activity.category);
    return <Paper sx={{ mb: 2, borderRadius: 3, overflow: 'hidden' }}>
        <Box sx={{ minHeight: 220, p: 3, display: 'flex', flexDirection: 'column', justifyContent: 'flex-end', color: 'white', backgroundImage: `linear-gradient(0deg, rgba(14,53,49,.86), rgba(14,53,49,.12)), url(${image.src})`, backgroundSize: 'cover', backgroundPosition: 'center' }}>
        <Chip label={image.label} sx={{ alignSelf: 'flex-start', mb: 1, bgcolor: 'rgba(255,255,255,.9)' }} />
        <Typography variant="h4">{activity.title}</Typography>
        <Typography sx={{ my: 1 }}>{formatDate(activity.date)}</Typography>
        <Typography>Kezelőorvos: {activity.practitioners.map(p => p.name).join(', ') || 'Nincs hozzárendelve'}</Typography>
        </Box>
        <Box sx={{ p: 3 }}>
        <Chip sx={{ my: 2 }} label={activity.isCancelled ? 'Lemondva' : status} />
        <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            {activity.canEditFields && <Button component={Link} to={`/manage/${activity.id}`} variant="contained">Esemény szerkesztése</Button>}
            {activity.canDelete && <Button color="error" variant="outlined" onClick={() => setConfirm(true)}>Esemény törlése</Button>}
            {activity.isAppointment && <Button component={Link} to="/booking">Foglalások / lemondás</Button>}
        </Box></Box>
        <Dialog open={confirm} onClose={() => { if (!deleteActivity.isPending) setConfirm(false); }}>
            <DialogTitle>Esemény törlése</DialogTitle>
            <DialogContent>
                <Typography>Biztosan törlöd ezt az eseményt: {activity.title}?</Typography>
                {activity.isAppointment && <Typography>A kapcsolódó foglalás is törlődik, az időpont felszabadul.</Typography>}
                {error && <Alert severity="error">{error}</Alert>}
            </DialogContent>
            <DialogActions>
                <Button disabled={deleteActivity.isPending} onClick={() => setConfirm(false)}>Mégse</Button>
                <Button color="error" loading={deleteActivity.isPending} onClick={() => void remove()}>Törlés</Button>
            </DialogActions>
        </Dialog>
    </Paper>;
}
