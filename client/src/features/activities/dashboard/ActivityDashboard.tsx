import { Grid } from '@mui/material';
import { useState } from 'react';
import ActivityList from './ActivityList';
import ActivityFilters, { type ActivityFilter } from './ActivityFilters';
export default function ActivityDashboard() {
    const [filter, setFilter] = useState<ActivityFilter>('all');
    const [date, setDate] = useState<Date | null>(null);
    return <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 8 }}><ActivityList filter={filter} date={date} /></Grid>
        <Grid size={{ xs: 12, md: 4 }}><ActivityFilters filter={filter} date={date} onDate={setDate} onFilter={value => { setFilter(value); if (value === 'all') setDate(null); }} /></Grid>
    </Grid>;
}
