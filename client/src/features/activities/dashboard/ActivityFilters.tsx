import { Box, Button, MenuItem, MenuList, Paper, TextField, Typography } from '@mui/material';
import Calendar from 'react-calendar';
import 'react-calendar/dist/Calendar.css';
import { categoryOptions } from '../form/categoryOptions';
export type ActivityFilter = 'all' | 'future' | 'history';
type Props = { filter: ActivityFilter; date: Date | null; category: string; onFilter: (value: ActivityFilter) => void; onDate: (value: Date | null) => void; onCategory: (value: string) => void };
export default function ActivityFilters({ filter, date, category, onFilter, onDate, onCategory }: Props) {
    return <Box sx={{ display: 'grid', gap: 3 }}>
        <Paper sx={{ p: 3, borderRadius: 3 }}><Typography variant="h6">Szűrők</Typography>
            <MenuList>{([{ value: 'all', label: 'Összes esemény' }, { value: 'future', label: 'Közelgő események' }, { value: 'history', label: 'Korábbi események' }] as const).map(item => <MenuItem key={item.value} selected={filter === item.value} onClick={() => onFilter(item.value)}>{item.label}</MenuItem>)}</MenuList>
            <TextField fullWidth select label="Kategória" value={category} onChange={e => onCategory(e.target.value)}>
                <MenuItem value="">Minden kategória</MenuItem>{categoryOptions.map(c => <MenuItem key={c.value} value={c.value}>{c.text}</MenuItem>)}
            </TextField>
        </Paper>
        <Paper sx={{ p: 3, borderRadius: 3 }}><Typography variant="h6" sx={{ mb: 2 }}>Dátum kiválasztása</Typography>
            <Calendar locale="hu-HU" value={date} onChange={value => onDate(value instanceof Date ? value : null)} />
            {date && <Button onClick={() => onDate(null)}>Dátumszűrés törlése</Button>}
            <Button onClick={() => { onFilter('all'); onCategory(''); onDate(null); }}>Összes szűrő törlése</Button>
        </Paper>
    </Box>;
}
