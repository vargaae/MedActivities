import axios from 'axios';
import type { QueryClient } from '@tanstack/react-query';
import { getActivityToken, setActivityToken } from './activitySession';

let changing = false;
export async function changeSession(cache: QueryClient, login?: () => Promise<string>) {
    if (changing) throw new Error('Már folyamatban van egy be- vagy kijelentkezés.');
    changing = true;
    const old = getActivityToken();
    setActivityToken('');
    try {
        await cache.cancelQueries(); cache.clear();
        if (old) {
            try {
                await axios.post(`${(import.meta.env.VITE_API_URL ?? '/api').replace(/\/$/, '')}/session/logout`, {},
                    { headers: { Authorization: `Bearer ${old}` }, timeout: 15000 });
            } catch (error) {
                // An already expired/revoked session is also safely logged out.
                if (!axios.isAxiosError(error) || error.response?.status !== 401) throw error;
            }
        }
        if (login) setActivityToken(await login());
    } finally { changing = false; }
}
