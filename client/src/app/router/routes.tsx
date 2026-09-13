import RequireSession from '../../features/management/RequireSession';
import PatientsPage from '../../features/management/PatientsPage';
import PractitionersPage from '../../features/management/PractitionersPage';
import UsersPage from '../../features/management/UsersPage';
import MedBookingPage from '../../features/med/MedBookingPage';
import { createBrowserRouter, Navigate } from "react-router";
import App from "../layout/App";
import ActivityDashboard from "../../features/activities/dashboard/ActivityDashboard";
import ActivityForm from "../../features/activities/form/ActivitityForm";
import HomePage from "../../features/home/HomePage";
import ActivityDetailsPage from "../../features/activities/details/ActivityDetailsPage";
import Counter from "../../features/counter/Counter";
import TestErrors from "../../features/errors/TestError";
import NotFound from "../../features/errors/NotFound";
import ServerError from "../../features/errors/ServerError";

export const router = createBrowserRouter([
    {
        path: "/",
        element: <App />,
        children: [
            { path: '', element: <HomePage /> },
            { path: 'booking', element: <RequireSession><MedBookingPage /></RequireSession> },
            { path: 'activities', element: <RequireSession><ActivityDashboard /></RequireSession> },
            { path: 'activities/:id', element: <RequireSession><ActivityDetailsPage /></RequireSession> },
            { path: 'createActivity', element: <RequireSession><ActivityForm key='create' /></RequireSession> },
            { path: 'manage/:id', element: <RequireSession><ActivityForm /></RequireSession> },
            { path: 'patients', element: <RequireSession roles={['Admin','AdmissionsOffice']}><PatientsPage /></RequireSession> },
            { path: 'practitioners', element: <RequireSession roles={['Admin','AdmissionsOffice']}><PractitionersPage /></RequireSession> },
            { path: 'users', element: <RequireSession roles={['Admin']}><UsersPage /></RequireSession> },
            { path: 'counter', element: <Counter /> },
            { path: 'errors', element: <TestErrors /> },
            { path: 'not-found', element: <NotFound /> },
            { path: 'server-error', element: <ServerError /> },
            { path: '*', element: <Navigate replace to='/not-found' /> }
        ]
    },
]);
