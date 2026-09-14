import { useEffect, useRef, useState, useSyncExternalStore } from 'react';
import { Alert, Box, Button, Card, CardContent, TextField, Typography } from '@mui/material';
import { HubConnectionBuilder, HubConnectionState, LogLevel, type HubConnection } from '@microsoft/signalr';
import { getActivitySessionVersion, getActivityToken, subscribeActivitySession } from '../../../lib/api/activitySession';

type Comment = { id: string; displayName: string; body: string; createdAt: string };
export default function ActivityDetailsChat({ activityId }: { activityId: string }) {
    const sessionVersion = useSyncExternalStore(subscribeActivitySession, getActivitySessionVersion);
    const token = getActivityToken();
    const [comments, setComments] = useState<Comment[]>([]);
    const [body, setBody] = useState('');
    const [error, setError] = useState('');
    const [connected, setConnected] = useState(false);
    const [sending, setSending] = useState(false);
    const connection = useRef<HubConnection | null>(null);
    useEffect(() => {
        let active = true;
        if (!token) {
            setConnected(false);
            setComments([]);
            setError('A chat használatához be kell jelentkezni.');
            return;
        }
        const base = (import.meta.env.VITE_API_URL ?? '/api').replace(/\/$/, '');
        const hub = new HubConnectionBuilder()
            .withUrl(base + '/chat?activityId=' + encodeURIComponent(activityId), { accessTokenFactory: getActivityToken, withCredentials: false })
            .withAutomaticReconnect().configureLogging(LogLevel.None).build();
        connection.current = hub;
        const reload = async () => {
            try {
                const items = await hub.invoke<Comment[]>('LoadComments');
                if (active) { setComments(items); setError(''); setConnected(true); }
            } catch {
                if (active) { setConnected(false); setComments([]); setError('A chat nem érhető el, vagy megváltozott a jogosultságod.'); }
            }
        };
        hub.on('CommentAdded', () => { void reload(); });
        hub.onreconnecting(() => { if (active) setConnected(false); });
        hub.onreconnected(() => { void reload(); });
        hub.onclose(() => { if (active) { setConnected(false); setError('A chatkapcsolat megszakadt. Frissítsd az oldalt.'); } });
        // React StrictMode fejlesztői módban rögtön újramountolja a komponenst.
        // A késleltetett indítás megakadályozza, hogy az első, már megszüntetett
        // kapcsolat a negotiation közben leálljon.
        const startTimer = window.setTimeout(() => {
            void hub.start().then(async () => { if (!active) { await hub.stop(); return; } await reload(); })
            .catch(() => { if (active) setError('A chat nem kapcsolódott. Ellenőrizd a belépést és az API-t.'); });
        }, 0);
        return () => { active = false; window.clearTimeout(startTimer); connection.current = null; void hub.stop(); };
    }, [activityId, sessionVersion, token]);
    return <Card sx={{ mt: 3 }}><CardContent>
        <Typography variant="h6">Eseményhez tartozó beszélgetés</Typography>
        <Typography variant="body2" color="text.secondary">Az eseményhez hozzáférők látják. Az utolsó 200 üzenet jelenik meg.</Typography>
        {error && <Alert severity="error" sx={{ my: 1 }}>{error}</Alert>}
        <Box role="log" aria-label="Chatüzenetek" aria-live="polite" sx={{ maxHeight: 360, overflowY: 'auto', my: 2 }}>
            {comments.map(c => <Box key={c.id} sx={{ py: 1, borderBottom: '1px solid #eee' }}>
                <Typography variant="subtitle2">{c.displayName} · {new Date(c.createdAt.endsWith('Z') ? c.createdAt : c.createdAt + 'Z').toLocaleString('hu-HU')}</Typography>
                <Typography sx={{ whiteSpace: 'pre-wrap', overflowWrap: 'anywhere' }}>{c.body}</Typography>
            </Box>)}
            {connected && comments.length === 0 && <Typography>Még nincs üzenet. Indíts beszélgetést.</Typography>}
        </Box>
        <Box component="form" sx={{ display: 'grid', gap: 1 }} onSubmit={async e => {
            e.preventDefault(); const hub = connection.current;
            if (!body.trim() || sending || hub?.state !== HubConnectionState.Connected) return;
            setSending(true); setError('');
            try { await hub.invoke('SendComment', body); setBody(''); }
            catch { setError('Az üzenet nem küldhető el. Ellenőrizd a kapcsolatot és a jogosultságodat.'); }
            finally { setSending(false); }
        }}>
            <TextField label="Üzenet" multiline minRows={2} value={body} onChange={e => setBody(e.target.value)} disabled={!connected || sending} slotProps={{ htmlInput: { maxLength: 2000 } }} helperText={body.length + '/2000 karakter'} />
            <Button variant="contained" type="submit" disabled={!connected || sending || !body.trim()}>{sending ? 'Küldés…' : 'Küldés'}</Button>
        </Box>
    </CardContent></Card>;
}
