import { getActivityToken } from './activitySession';
import axios from 'axios';
import { store } from '../stores/store';
import { toast } from 'react-toastify';
import { router } from '../../app/router/routes';

const sleep = (delay: number) => {
    return new Promise((resolve) => {
        setTimeout(resolve, delay);
    });
}

const agent = axios.create({
    baseURL: import.meta.env.VITE_API_URL
});

agent.interceptors.request.use(config => {
    store.uiStore.isBusy();
    const token = getActivityToken();
    if (token) config.headers.Authorization = `Bearer ${token}`;
    return config;
})

agent.interceptors.response.use(
    async response => {
        await sleep(1000);
        store.uiStore.isIdle();
        return response;
    },
    async error => {
        await sleep(1000);
        store.uiStore.isIdle(); // Ensure the busy state is reset on error
        if (!error.response) { toast.error('Az API nem érhető el.'); return Promise.reject(error); }
        const {data, status} = error.response;
        switch (status) {
            case 400:
                if (data.errors) {
                const modalStateErrors = [];
                for (const key in data.errors) {
                    if (data.errors[key]) {
                        modalStateErrors.push(data.errors[key])
                    }
                }
                throw modalStateErrors.flat();
            } else {
                toast.error(typeof data === 'string' ? data : data.message ?? data.title ?? 'Hibás kérés.');
            }
                break;
            case 409:
                toast.error(data.message ?? 'Ütközés az adatokban.');
                break;
            case 401:
                toast.error('unauthorised');
                break;
            case 403:
                toast.error('forbidden');
                break;
            case 404:
                await router.navigate('/not-found');
                break;
            case 500:
                router.navigate('/server-error', {state: {error: data}})
                break;
        }

        return Promise.reject(error);
    }
);

export default agent;