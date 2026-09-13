import { useState } from 'react';
import { Alert, Box, Button, Dialog, DialogContent, DialogTitle, MenuItem, Tab, Tabs, TextField } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router';
import axios from 'axios';
import { setActivityToken } from '../../lib/api/activitySession';
export default function AuthDialog({ open, onClose }: { open: boolean; onClose: () => void }) {
    const [mode, setMode] = useState(0);
    const [form, setForm] = useState({ userName: '', password: '', email: '', name: '', role: 'Patient', tajNumber: '', birthDate: '', specialty: 'Orvos' });
    const [busy, setBusy] = useState(false); const [message, setMessage] = useState(''); const [success, setSuccess] = useState(false);
    const cache = useQueryClient(); const navigate = useNavigate();
    const field = (key: keyof typeof form) => ({ value: form[key], onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => setForm({ ...form, [key]: e.target.value }), disabled: busy });
    function close() { if (!busy) { setForm({ ...form, password: '' }); setMessage(''); onClose(); } }
    return <Dialog open={open} onClose={close} fullWidth maxWidth="sm">
        <DialogTitle>Belépés az EgészségÚtba</DialogTitle><DialogContent>
            <Tabs value={mode} onChange={(_, value: number) => { setMode(value); setMessage(''); }} sx={{ mb: 3 }}><Tab label="Belépés" disabled={busy} /><Tab label="Regisztráció" disabled={busy} /></Tabs>
            <Box component="form" sx={{ display: 'grid', gap: 2 }} onSubmit={async e => {
                e.preventDefault(); setBusy(true); setMessage(''); setSuccess(false);
                try {
                    const base = (import.meta.env.VITE_API_URL ?? '/api').replace(/\/$/, '');
                    if (mode === 1) {
                        await axios.post(`${base}/session/register`, { ...form, birthDate: form.birthDate || '2000-01-01' });
                        setMode(0); setForm({ ...form, password: '' }); setSuccess(true); setMessage('A fiók és a profil elkészült. Jelentkezz be.');
                    } else {
                        const { data } = await axios.post<{ accessToken: string }>(`${base}/session/login`, { userName: form.userName, password: form.password });
                        await cache.cancelQueries(); cache.clear(); setActivityToken(data.accessToken);
                        setForm({ ...form, password: '' }); onClose(); await navigate('/activities');
                    }
                } catch (error) {
                    const data = axios.isAxiosError(error) ? error.response?.data : null;
                    setMessage(data?.message ?? (data?.errors ? Object.values(data.errors).flat().join(' ') : 'Sikertelen művelet. Ellenőrizd az adatokat.'));
                } finally { setBusy(false); }
            }}>
                {message && <Alert severity={success ? 'success' : 'error'}>{message}</Alert>}
                <TextField id="auth-username" name="userName" label={mode ? 'Felhasználónév' : 'Felhasználónév vagy email'} autoComplete="username" required {...field('userName')} />
                <TextField id="auth-password" name="password" label="Jelszó" type="password" autoComplete={mode ? 'new-password' : 'current-password'} required {...field('password')} helperText={mode ? 'Legalább 6 karakter, kis- és nagybetű, szám és speciális karakter.' : undefined} />
                {mode === 1 && <>
                    <TextField id="auth-name" name="name" label="Teljes név" required {...field('name')} /><TextField id="auth-email" name="email" label="Email" type="email" required {...field('email')} />
                    <TextField id="auth-role" name="role" select label="Profil" {...field('role')}><MenuItem value="Patient">Páciens</MenuItem><MenuItem value="Practitioner">Kezelő (orvos / gyógytornász)</MenuItem></TextField>
                    <TextField id="auth-taj" name="tajNumber" label="TAJ-szám" required {...field('tajNumber')} helperText="9 számjegy" />
                    {form.role === 'Patient' ? <TextField id="auth-birth-date" name="birthDate" label="Születési dátum" type="date" required slotProps={{ inputLabel: { shrink: true } }} {...field('birthDate')} /> : <TextField id="auth-specialty" name="specialty" select label="Szakterület" {...field('specialty')}><MenuItem value="Orvos">Orvos</MenuItem><MenuItem value="Gyógytornász">Gyógytornász</MenuItem></TextField>}
                    <Alert severity="info">Admin és felvételi irodai fiókot csak admin hozhat létre. Meglévő páciensprofil esetén az admin kapcsolhatja hozzá a fiókot.</Alert>
                </>}
                <Button type="submit" variant="contained" disabled={busy}>{busy ? 'Folyamatban…' : mode ? 'Regisztráció' : 'Belépés'}</Button><Button onClick={close} disabled={busy}>Bezárás</Button>
            </Box>
        </DialogContent>
    </Dialog>;
}

