import { Box, Typography } from '@mui/material';

export default function ActivityPeople({ activity }: { activity: Activity }) {
    return <Box sx={{ py: 1, overflowWrap: 'anywhere' }}>
        <Typography variant="body2">
            <strong>Páciensek: </strong>
            {activity.patients?.map(p => p.name).join(', ') || 'Nincs hozzárendelve'}
        </Typography>
        <Typography variant="body2" sx={{ mt: 0.5 }}>
            <strong>Kezelőorvosok: </strong>
            {activity.practitioners?.map(p => p.name).join(', ') || 'Nincs hozzárendelve'}
        </Typography>
    </Box>;
}