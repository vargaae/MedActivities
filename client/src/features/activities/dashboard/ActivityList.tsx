import { Alert, Box, Typography } from '@mui/material';
import { useState } from 'react';
import ActivityCard from './ActivityCard';
import { useActivities } from '../../../lib/hooks/useActivities';
import type { ActivityFilter } from './ActivityFilters';
export default function ActivityList({ filter, date, category, patientId, practitionerId }: { filter: ActivityFilter; date: Date | null; category: string; patientId: string; practitionerId: string }) {
    const { activities, isPending, isError } = useActivities();
    const [now] = useState(() => Date.now());
    if (isPending) return <Typography>Betöltés…</Typography>;
    if (isError) return <Alert severity="error">Az eseménylista nem tölthető be.</Alert>;
    const filtered = (activities ?? []).filter(activity => {
        if (patientId && !activity.patients?.some(p => p.id === patientId)) return false;
        if (practitionerId && !activity.practitioners?.some(p => p.id === practitionerId)) return false;
        if (category && activity.category !== category) return false;
        const start = new Date(activity.date);
        if (date && (start.getFullYear() !== date.getFullYear() || start.getMonth() !== date.getMonth() || start.getDate() !== date.getDate())) return false;
        if (filter === 'future') return start.getTime() >= now;
        if (filter === 'history') return start.getTime() < now;
        return true;
    });
    return <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
        <Typography role="status">{filtered.length} esemény</Typography>
        {filtered.length === 0 && <Alert severity="info">Nincs a szűrésnek megfelelő esemény.</Alert>}
        {filtered.map(activity => <ActivityCard key={activity.id} activity={activity} />)}
    </Box>;
}

