import { useState } from 'react';
import { Alert, Box, Button, Dialog, DialogContent, DialogTitle, IconButton, Tab, Tabs, TextField } from '@mui/material';
import { CloseRounded } from '@mui/icons-material';
import { useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router';
import axios from 'axios';
import { setActivityToken } from '../../lib/api/activitySession';

export default function AuthDialog({ open, onClose }: { open: boolean; onClose: () => void }) {
    const [mode, setMode] = useState(0);
    const [email, setEmail] = useState(''); const [password, setPassword] = useState('');
    const [busy, setBusy] = useState(false); const [message, setMessage] = useState('');
    const [success, setSuccess] = useState(false);
    const cache = useQueryClient(); const navigate = useNavigate();
    function close() { if (!busy) { setPassword(''); setMessage(''); onClose(); } }
    return <Dialog open={open} onClose={close} fullWidth maxWidth="xs" PaperProps={{ sx: { borderRadius: 4 } }}>
        <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>Üdvözlünk az EgészségÚton<IconButton aria-label="Bezárás" onClick={close} disabled={busy}><CloseRounded /></IconButton></DialogTitle>
        <DialogContent>
            <Tabs value={mode} onChange={(_, value: number) => { setMode(value); setMessage(''); }} aria-label="Belépés vagy regisztráció" sx={{ mb: 3 }}>
                <Tab label="Belépés" disabled={busy} /><Tab label="Regisztráció" disabled={busy} />
            </Tabs>
            <Box component="form" sx={{ display: 'grid', gap: 2 }} onSubmit={async event => {
                event.preventDefault(); setBusy(true); setMessage(''); setSuccess(false);
                try {
                    const base = (import.meta.env.VITE_API_URL ?? '/api').replace(/\/$/, '');
                    if (mode === 1) {
                        await axios.post(`${base}/auth/register`, { email, password });
                        setMode(0); setPassword(''); setSuccess(true);
                        setMessage('A fiók elkészült. A páciens- vagy kezelőprofil összekapcsolását az admin végzi. Most bejelentkezhetsz.');
                    } else {
                        const { data } = await axios.post<{ accessToken: string }>(`${base}/auth/login?useCookies=false`, { email, password });
                        await cache.cancelQueries(); cache.clear(); setActivityToken(data.accessToken);
                        setPassword(''); onClose(); await navigate('/activities');
                    }
                } catch (error) {
                    const errors = axios.isAxiosError(error) ? error.response?.data?.errors : undefined;
                    setMessage(errors ? Object.values(errors).flat().join(' ') : mode === 0 ? 'Sikertelen belépés. Ellenőrizd az emailt és a jelszót.' : 'A regisztráció nem sikerült. Próbálj másik emailt vagy erősebb jelszót.');
                } finally { setBusy(false); }
            }}>
                {message && <Alert severity={success ? 'success' : 'error'}>{message}</Alert>}
                <TextField label="Email-cím" type="email" autoComplete="username" required value={email} onChange={e => setEmail(e.target.value)} disabled={busy} />
                <TextField label="Jelszó" type="password" autoComplete={mode === 0 ? 'current-password' : 'new-password'} required value={password} onChange={e => setPassword(e.target.value)} disabled={busy} helperText={mode === 1 ? 'Legalább 6 karakter, kis- és nagybetű, szám és speciális karakter.' : undefined} />
                <Button type="submit" variant="contained" size="large" disabled={busy} sx={{ borderRadius: 2, py: 1.4 }}>{busy ? 'Egy pillanat…' : mode === 0 ? 'Belépés' : 'Fiók létrehozása'}</Button>
            </Box>
        </DialogContent>
    </Dialog>;
}