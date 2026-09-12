import { Alert, Box, Typography } from '@mui/material';
import ActivityCard from './ActivityCard';
import { useActivities } from '../../../lib/hooks/useActivities';
import type { ActivityFilter } from './ActivityFilters';
export default function ActivityList({ filter, date }: { filter: ActivityFilter; date: Date | null }) {
    const { activities, isPending, isError } = useActivities();
    if (isPending) return <Typography>Betöltés…</Typography>;
    if (isError) return <Alert severity="error">Az eseménylista nem tölthető be.</Alert>;
    const now = Date.now();
    const filtered = (activities ?? []).filter(activity => {
        const start = new Date(activity.date);
        if (date && (start.getFullYear() !== date.getFullYear() || start.getMonth() !== date.getMonth() || start.getDate() !== date.getDate())) return false;
        if (filter === 'future') return start.getTime() >= now;
        if (filter === 'history') return start.getTime() < now;
        if (filter === 'examination' || filter === 'treatment') return activity.category.toLowerCase() === filter;
        return true;
    });
    return <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
        <Typography role="status">{filtered.length} esemény</Typography>
        {filtered.length === 0 && <Alert severity="info">Nincs a szűrésnek megfelelő esemény.</Alert>}
        {filtered.map(activity => <ActivityCard key={activity.id} activity={activity} />)}
    </Box>;
}
