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
          Completed: "Teljesült",
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
        <Box sx={{ px: { xs: 2, sm: 3 }, py: 2, display: 'flex', flexDirection: 'column', color: 'white', backgroundImage: `linear-gradient(0deg, rgba(14,53,49,.86), rgba(14,53,49,.12)), url(${image.src})`, backgroundSize: 'cover', backgroundPosition: 'center' }}>
        <Chip label={image.label} sx={{ alignSelf: 'flex-start', mb: 1, bgcolor: 'rgba(255,255,255,.9)' }} />
        <Typography variant="h4" sx={{ fontSize: { xs: "1.55rem", sm: "2.125rem" }, overflowWrap: "anywhere" }}>{activity.title}</Typography>
        <Typography sx={{ my: 1 }}>{formatDate(activity.date)}</Typography>
        {!!activity.practitioners?.length && <Typography>Kezelőorvos: {activity.practitioners.map(p => p.name).join(', ')}</Typography>}
        </Box>
        <Box sx={{ px: { xs: 2, sm: 3 }, py: 1.5, display: 'flex', alignItems: 'center', gap: 1.5, flexWrap: 'wrap' }}>
        <Chip label={activity.isCancelled ? 'Lemondva' : status} />
        <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', width: '100%' }}>
            {activity.canEditFields && <Button component={Link} to={`/manage/${activity.id}`} variant="contained" sx={{ maxWidth: '100%' }}>Esemény szerkesztése</Button>}
            {activity.canDelete && <Button color="error" variant="outlined" onClick={() => setConfirm(true)} sx={{ maxWidth: '100%' }}>Esemény törlése</Button>}
            {activity.isAppointment && <Button component={Link} to="/booking" sx={{ maxWidth: '100%' }}>Foglalások / lemondás</Button>}
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
