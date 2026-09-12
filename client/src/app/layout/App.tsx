import { Box, Container, CssBaseline } from '@mui/material';
import { Outlet, ScrollRestoration, useLocation } from 'react-router';
import NavBar from './NavBar';

export default function App() {
    const home = useLocation().pathname === '/';
    return <Box className="eu-app">
        <ScrollRestoration /><CssBaseline />
        <a className="eu-skip" href="#main-content">Ugrás a tartalomra</a>
        <NavBar />
        {home ? <main id="main-content"><Outlet /></main> :
            <Container component="main" id="main-content" maxWidth="xl" sx={{ py: { xs: 3, md: 5 } }}><Outlet /></Container>}
    </Box>;
}