import { Box, Typography } from '@mui/material';

export default function ActivityPeople({ activity }: { activity: Activity }) {
    return <Box sx={{ py: 1, overflowWrap: 'anywhere' }}>
        <Typography variant="body2">
            <strong>Páciens: </strong>
            {activity.patients?.map(p => p.name).join(', ') || 'Nincs hozzárendelve'}
        </Typography>
        {!!activity.practitioners?.length && <Typography variant="body2" sx={{ mt: 0.5 }}>
            <strong>Kezelőorvosok: </strong>
            {activity.practitioners.map(p => p.name).join(', ')}
        </Typography>}
    </Box>;
}
