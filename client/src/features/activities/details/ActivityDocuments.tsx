import { useEffect, useRef, useState } from 'react';
import { Alert, Box, Button, Paper, TextField, Typography } from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useSearchParams } from 'react-router';
import agent from '../../../lib/api/agent';
import { useActivityAccess } from '../../../lib/hooks/useActivityAccess';
import PaginatedList from '../../../app/shared/components/PaginatedList';
import { errorText } from '../../management/shared';

type EventDocument = { id: string; title: string; fileName: string; size: number; uploadedByName: string; createdAt: string };

export default function ActivityDocuments({ activityId }: { activityId: string }) {
    const session = useActivityAccess();
    const cache = useQueryClient();
    const [params] = useSearchParams();
    const target = params.get('document');
    const panel = useRef<HTMLDivElement>(null);
    const [title, setTitle] = useState('');
    const [file, setFile] = useState<File | null>(null);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);
    const url = `/activities/${activityId}/documents`;
    const documents = useQuery({
        queryKey: ['activity-documents', session.version, activityId],
        queryFn: async ({ signal }) => (await agent.get<EventDocument[]>(url, { signal })).data,
        enabled: session.authenticated,
        refetchInterval: 30000,
    });
    useEffect(() => {
        if (target && documents.data) panel.current?.scrollIntoView({ block: 'start' });
    }, [target, documents.data]);
    async function upload() {
        setBusy(true); setError(''); setSuccess(false);
        try {
            if (!file || !file.size || file.size > 5 * 1024 * 1024) throw new Error('Válassz egy nem üres, legfeljebb 5 MB-os fájlt.');
            const data = new FormData(); data.append('title', title); data.append('file', file);
            await agent.post(url, data);
            setTitle(''); setFile(null); setSuccess(true);
            await cache.invalidateQueries({ queryKey: ['activity-documents'] });
            await cache.invalidateQueries({ queryKey: ['notifications'] });
        } catch (e) { setError(errorText(e)); }
        finally { setBusy(false); }
    }
    async function download(doc: EventDocument) {
        setBusy(true); setError('');
        try {
            const response = await agent.get<Blob>(`${url}/${doc.id}/content`, { responseType: 'blob' });
            const blob = URL.createObjectURL(response.data);
            const link = document.createElement('a'); link.href = blob; link.download = doc.fileName; link.click();
            setTimeout(() => URL.revokeObjectURL(blob), 1000);
        } catch (e) { setError(errorText(e)); }
        finally { setBusy(false); }
    }
    return <Paper ref={panel} id="documents" sx={{ p: { xs: 2, sm: 3 }, mb: 2, scrollMarginTop: 100 }}>
        <Typography variant="h6" gutterBottom>Esemény dokumentumai</Typography>
        <Typography variant="body2" sx={{ mb: 2 }}>Az eseményhez hozzáférő felhasználók tölthetnek fel dokumentumot. PDF, PNG, JPEG vagy UTF-8 szöveg, legfeljebb 5 MB.</Typography>
        <Box component="form" onSubmit={e => { e.preventDefault(); void upload(); }} sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
            <TextField label="Dokumentum címe" required value={title} onChange={e => setTitle(e.target.value)} disabled={busy}
                slotProps={{ htmlInput: { maxLength: 200 } }} sx={{ flex: '1 1 220px', minWidth: 0 }} />
            <Button component="label" disabled={busy}>Fájl kiválasztása<input hidden type="file" accept=".pdf,.png,.jpg,.jpeg,.txt"
                onChange={e => { setFile(e.target.files?.[0] ?? null); e.target.value = ''; }} /></Button>
            <Button type="submit" variant="contained" disabled={busy || !file || !title.trim()}>Feltöltés</Button>
        </Box>
        {file && <Typography sx={{ overflowWrap: 'anywhere' }}>{file.name}</Typography>}
        {error && <Alert severity="error">{error}</Alert>}
        {success && <Alert severity="success">A dokumentum feltöltve, az érintettek értesítést kaptak.</Alert>}
        {documents.isPending && <Typography>Dokumentumok betöltése…</Typography>}
        {documents.isError && <Alert severity="error">A dokumentumok nem tölthetők be. <Button onClick={() => void documents.refetch()}>Újrapróbálás</Button></Alert>}
        {documents.data?.length === 0 && <Typography>Még nincs dokumentum.</Typography>}
        <PaginatedList items={documents.data ?? []} label="Dokumentumok" focusId={target}>
            {doc => <Box key={doc.id} sx={{ p: 2, my: 1, border: 1, borderColor: 'divider', borderRadius: 2, overflowWrap: 'anywhere', bgcolor: target === doc.id ? 'action.selected' : undefined }}>
                <Typography fontWeight={600}>{doc.title}</Typography>
                <Typography variant="body2">Feltöltötte: {doc.uploadedByName}</Typography>
                <Typography variant="body2">{doc.fileName} · {Math.ceil(doc.size / 1024)} KB · {new Date(doc.createdAt.endsWith('Z') ? doc.createdAt : doc.createdAt + 'Z').toLocaleString('hu-HU')}</Typography>
                <Button disabled={busy} onClick={() => void download(doc)}>Letöltés</Button>
            </Box>}
        </PaginatedList>
    </Paper>;
}
