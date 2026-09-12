import { useState } from 'react';
import { Link, NavLink, useNavigate } from 'react-router';
import { useQueryClient } from '@tanstack/react-query';
import { Observer } from 'mobx-react-lite';
import { LinearProgress } from '@mui/material';
import { AddRounded, ArrowOutwardRounded, LogoutRounded, MenuRounded, CloseRounded, ShieldOutlined } from '@mui/icons-material';
import { useStore } from '../../lib/hooks/useStore';
import { useActivityAccess } from '../../lib/hooks/useActivityAccess';
import { setActivityToken } from '../../lib/api/activitySession';
import AuthDialog from '../../features/home/AuthDialog';

const labels: Record<string, string> = { Admin: 'ADMIN', AdmissionsOffice: 'Felvételi iroda', Practitioner: 'Practitioner', Patient: 'Páciens' };
export default function NavBar() {
    const { uiStore } = useStore();
    const session = useActivityAccess();
    const cache = useQueryClient();
    const navigate = useNavigate();
    const [menu, setMenu] = useState(false);
    const [authOpen, setAuthOpen] = useState(false);
    const role = session.data?.roles?.find(r => labels[r]);
    async function logout() {
        await cache.cancelQueries(); cache.clear(); setActivityToken(''); setMenu(false); await navigate('/');
    }
    return <>
        <header className="eu-nav">
            <div className="eu-nav-inner">
                <Link to="/" className="eu-brand" aria-label="EgészségÚt kezdőlap" onClick={() => setMenu(false)}>
                    <img src="/brand/EgeszsegUt_logo_light.svg" alt="EgészségÚt" width="231" height="45" />
                </Link>
                <button className="eu-mobile-toggle" aria-label={menu ? 'Menü bezárása' : 'Menü megnyitása'} aria-expanded={menu} aria-controls="eu-nav-links" onClick={() => setMenu(!menu)}>
                    {menu ? <CloseRounded /> : <MenuRounded />}
                </button>
                <nav id="eu-nav-links" className={`eu-nav-links ${menu ? 'is-open' : ''}`} aria-label="Fő navigáció">
                    <NavLink to="/" end onClick={() => setMenu(false)}>Kezdőlap</NavLink>
                    <NavLink to="/activities" onClick={() => setMenu(false)}>Események</NavLink>
                    {session.data?.canCreate && <NavLink to="/createActivity" onClick={() => setMenu(false)}><AddRounded fontSize="small" /> Új esemény</NavLink>}
                    <div className="eu-nav-session">
                        {session.authenticated ? <>
                            <span className="eu-role-pill"><ShieldOutlined fontSize="small" />{role ? labels[role] : session.isError ? 'Lejárt munkamenet' : 'Belépve'}</span>
                            <button className="eu-icon-button" aria-label="Kijelentkezés" title="Kijelentkezés" onClick={() => void logout()}><LogoutRounded fontSize="small" /></button>
                        </> : <button className="eu-button eu-button-small" onClick={() => { setAuthOpen(true); setMenu(false); }}>Belépés <ArrowOutwardRounded fontSize="small" /></button>}
                    </div>
                </nav>
            </div>
            <Observer>{() => uiStore.isLoading ? <LinearProgress sx={{ position: 'absolute', left: 0, right: 0, bottom: 0, height: 2 }} /> : null}</Observer>
        </header>
        <AuthDialog open={authOpen} onClose={() => setAuthOpen(false)} />
    </>;
}