import { Alert, Box, Button, Chip, Dialog, DialogActions, DialogContent, DialogTitle, Paper, Typography } from '@mui/material';
import { Link, useNavigate } from 'react-router';
import { useState } from 'react';
import { useActivities } from '../../../lib/hooks/useActivities';
import { formatDate } from '../../../lib/util/util';

export default function ActivityDetailsHeader({ activity }: { activity: Activity }) {
    const { deleteActivity } = useActivities(activity.id);
    const navigate = useNavigate();
    const [confirm, setConfirm] = useState(false);
    const [error, setError] = useState('');
    async function remove() {
        setError('');
        try { await deleteActivity.mutateAsync(activity.id); await navigate('/activities', { replace: true }); }
        catch { setError('A törlés nem sikerült. Ellenőrizd a jogosultságot és próbáld újra.'); }
    }
    return <Paper sx={{ p: 3, mb: 2, borderRadius: 3 }}>
        <Typography variant="h4">{activity.title}</Typography>
        <Typography sx={{ my: 1 }}>{formatDate(activity.date)}</Typography>
        <Typography>Kezelőorvos: {activity.practitioners.map(p => p.name).join(', ') || 'Nincs hozzárendelve'}</Typography>
        <Chip sx={{ my: 2 }} label={activity.isCancelled ? 'Lemondva' : activity.status} />
        <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            {activity.canEditFields && <Button component={Link} to={`/manage/${activity.id}`} variant="contained">Esemény szerkesztése</Button>}
            {activity.canDelete && <Button color="error" variant="outlined" onClick={() => setConfirm(true)}>Esemény törlése</Button>}
            {activity.isAppointment && <Button component={Link} to="/booking">Foglalások / lemondás</Button>}
        </Box>
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
