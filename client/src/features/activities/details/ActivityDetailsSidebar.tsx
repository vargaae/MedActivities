import { Paper, Typography, List, ListItem, ListItemText } from '@mui/material';

export default function ActivityDetailsSidebar({ activity }: { activity: Activity }) {
    const patients = activity.patients ?? [];
    const practitioners = activity.practitioners ?? [];
    return <Paper sx={{ p: 2 }}>
        <Typography variant="h6" sx={{ overflowWrap: 'anywhere' }}>Páciensek ({patients.length})</Typography>
        {patients.length === 0 && <Typography>Nincs hozzárendelve</Typography>}
        <List>{patients.map(p => <ListItem key={p.id} disableGutters>
            <ListItemText primary={p.name} sx={{ overflowWrap: 'anywhere' }} />
        </ListItem>)}</List>
        {practitioners.length > 0 && <>
        <Typography variant="h6" sx={{ overflowWrap: 'anywhere' }}>Kezelőorvosok ({practitioners.length})</Typography>
        <List>{practitioners.map(p => <ListItem key={p.id} disableGutters>
            <ListItemText primary={p.name} sx={{ overflowWrap: 'anywhere' }} />
        </ListItem>)}</List>
        </>}
    </Paper>;
}
