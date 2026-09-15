import { Autocomplete, TextField } from '@mui/material';

type Person = { id: string; name: string };
export default function PersonSearch({ label, options, value, onChange, loading = false }: {
    label: string; options: Person[]; value: string; onChange: (id: string) => void; loading?: boolean;
}) {
    return <Autocomplete fullWidth autoHighlight autoComplete options={options}
        value={options.find(p => p.id === value) ?? null}
        getOptionLabel={p => p.name} getOptionKey={p => p.id}
        isOptionEqualToValue={(a, b) => a.id === b.id}
        onChange={(_, person) => onChange(person?.id ?? '')}
        loading={loading} loadingText="Betöltés…" noOptionsText="Nincs találat"
        clearText="Törlés" openText="Találatok" closeText="Bezárás"
        renderInput={params => <TextField {...params} label={label} placeholder="Kezdj el beírni egy nevet…" />} />;
}
