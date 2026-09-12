import { Box, Button, MenuItem, MenuList, Paper, Typography } from '@mui/material';
import Calendar from 'react-calendar';
import 'react-calendar/dist/Calendar.css';
export type ActivityFilter = 'all' | 'future' | 'history' | 'examination' | 'treatment';
type Props = { filter: ActivityFilter; date: Date | null; onFilter: (value: ActivityFilter) => void; onDate: (value: Date | null) => void };
const filters: { value: ActivityFilter; label: string }[] = [
    { value: 'all', label: 'All Events' }, { value: 'future', label: 'Future Events' },
    { value: 'history', label: 'History' }, { value: 'examination', label: 'Examinations' }, { value: 'treatment', label: 'Treatments' }
];
export default function ActivityFilters({ filter, date, onFilter, onDate }: Props) {
    return <Box sx={{ display: 'grid', gap: 3 }}>
        <Paper sx={{ p: 3, borderRadius: 3 }}><Typography variant="h6">Filters</Typography>
            <MenuList>{filters.map(item => <MenuItem key={item.value} selected={filter === item.value} onClick={() => onFilter(item.value)}>{item.label}</MenuItem>)}</MenuList>
        </Paper>
        <Paper sx={{ p: 3, borderRadius: 3 }}><Typography variant="h6" sx={{ mb: 2 }}>Select date</Typography>
            <Calendar value={date} onChange={value => onDate(value instanceof Date ? value : null)} />
            {date && <Button onClick={() => onDate(null)}>Dátumszűrés törlése</Button>}
        </Paper>
    </Box>;
}
