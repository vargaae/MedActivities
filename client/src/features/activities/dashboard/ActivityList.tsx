import { Alert, Box, Button, CircularProgress, Typography } from '@mui/material';
import { useEffect, useRef, useState } from 'react';
import { format } from 'date-fns';
import ActivityCard from './ActivityCard';
import { useActivityPages } from '../../../lib/hooks/useActivityPages';
import type { ActivityFilter } from './ActivityFilters';
export default function ActivityList({ filter, date, category, patientId, practitionerId }: { filter: ActivityFilter; date: [Date, Date] | null; category: string; patientId: string; practitionerId: string }) {
    const [now] = useState(() => new Date());
    let from = date ? new Date(date[0]) : null;
    let to = date ? new Date(date[1]) : null;
    if (from) from.setHours(0, 0, 0, 0);
    if (to) { to.setHours(0, 0, 0, 0); to.setDate(to.getDate() + 1); }
    if (filter === 'future' && (!from || from < now)) from = now;
    if (filter === 'history' && (!to || to > now)) to = now;
    const emptyRange = !!from && !!to && from >= to;
    const query = useActivityPages({ patientId: patientId || undefined, practitionerId: practitionerId || undefined,
        category: category || undefined, from: from ? format(from, "yyyy-MM-dd'T'HH:mm:ss") : undefined,
        to: to ? format(to, "yyyy-MM-dd'T'HH:mm:ss") : undefined }, !emptyRange);
    const sentinel = useRef<HTMLDivElement | null>(null);
    const { fetchNextPage, hasNextPage, isFetching, isFetchNextPageError } = query;
    useEffect(() => {
        const target = sentinel.current;
        if (!target || !hasNextPage || isFetching || isFetchNextPageError || emptyRange) return;
        const observer = new IntersectionObserver(entries => {
            if (entries[0].isIntersecting) void fetchNextPage();
        }, { rootMargin: '200px' });
        observer.observe(target);
        return () => observer.disconnect();
    }, [fetchNextPage, hasNextPage, isFetching, isFetchNextPageError, emptyRange]);
    const activities = emptyRange ? [] : query.data?.pages.flatMap(p => p.items) ?? [];
    return <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
        <Typography role="status">{activities.length} betöltött esemény</Typography>
        {query.isLoading && <CircularProgress aria-label="Események betöltése" />}
        {query.isError && !emptyRange && <Alert severity="error">Az eseménylista nem tölthető be. <Button onClick={() => void query.refetch()}>Újrapróbálás</Button></Alert>}
        {!query.isLoading && !query.isError && activities.length === 0 && <Alert severity="info">Nincs a szűrésnek megfelelő esemény.</Alert>}
        {activities.map(activity => <ActivityCard key={activity.id} activity={activity} />)}
        <Box ref={sentinel} sx={{ textAlign: 'center', py: 2 }}>
            {!emptyRange && hasNextPage && <Button disabled={isFetching} onClick={() => void fetchNextPage()}>{isFetching ? 'Betöltés…' : 'További 30 esemény'}</Button>}
            {!hasNextPage && activities.length > 0 && <Typography color="text.secondary">Az összes találat betöltve.</Typography>}
        </Box>
    </Box>;
}

