import { useState } from 'react';
import { Alert, Box, Button, TextField, Typography } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import agent from '../../../lib/api/agent';
import { setActivityToken } from '../../../lib/api/activitySession';
import { useActivityAccess } from '../../../lib/hooks/useActivityAccess';

export default function ActivityLogin() {
    const session = useActivityAccess();
    const cache = useQueryClient();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState('');
    async function logout() {
        await cache.cancelQueries(); setActivityToken(''); cache.clear();
    }
    if (session.authenticated) return <Box sx={{ mb: 2 }}>
        <Typography>{session.isError ? 'A munkamenet nem érvényes. Lépj ki, majd jelentkezz be újra.' :
            session.data?.canAssign ? 'Admin / felvételi iroda: hozzárendelések szerkeszthetők.' :
            session.data?.isPractitioner ? 'Kezelőorvosi munkamenet' : 'Páciens / korlátozott munkamenet'}</Typography>
        <Button onClick={() => void logout()}>Kilépés</Button>
    </Box>;
    return <Box component="form" sx={{ display: 'grid', gap: 2, mb: 2 }} onSubmit={async event => {
        event.preventDefault(); setBusy(true); setError('');
        try {
            await cache.cancelQueries(); cache.clear();
            const { data } = await agent.post<{ accessToken: string }>('/auth/login?useCookies=false', { email, password });
            setActivityToken(data.accessToken); setPassword('');
            await cache.invalidateQueries({ queryKey: ['activities'] });
        } catch { setError('Sikertelen belépés. Ellenőrizd az emailt és a jelszót.'); }
        finally { setBusy(false); }
    }}>
        <Typography>Az esemény szerkesztéséhez jelentkezz be.</Typography>
        {error && <Alert severity="error">{error}</Alert>}
        <TextField label="Email" type="email" autoComplete="username" required value={email} onChange={e => setEmail(e.target.value)} />
        <TextField label="Jelszó" type="password" autoComplete="current-password" required value={password} onChange={e => setPassword(e.target.value)} />
        <Button type="submit" variant="contained" disabled={busy}>Belépés</Button>
    </Box>;
}