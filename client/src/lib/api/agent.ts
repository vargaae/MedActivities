import { getActivityToken, setActivityToken } from "./activitySession";
import axios from "axios";
import { store } from "../stores/store";
import { toast } from "react-toastify";
import { router } from "../../app/router/routes";


const agent = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
});

agent.interceptors.request.use((config) => {
  store.uiStore.isBusy();
  const token = getActivityToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

agent.interceptors.response.use(
  async (response) => {
    store.uiStore.isIdle();
    return response;
  },
  async (error) => {
    store.uiStore.isIdle();

    // Az oldal- vagy szerepkörváltás miatt megszakított kérés nem API-hiba.
    if (axios.isCancel(error) || error.code === "ERR_CANCELED") {
      return Promise.reject(error);
    }

    // Nincs HTTP-válasz: ne jelenjen meg globális toast.
    if (!error.response) {
      return Promise.reject(error);
    }

    const { data, status } = error.response;
    switch (status) {
      case 400:
        if (data.errors) {
          const modalStateErrors = [];
          for (const key in data.errors) {
            if (data.errors[key]) {
              modalStateErrors.push(data.errors[key]);
            }
          }
          throw modalStateErrors.flat();
        } else {
          toast.error(
            typeof data === "string"
              ? data
              : (data.message ?? data.title ?? "Hibás kérés."),
          );
        }
        break;
      case 409:
        toast.error(data.message ?? "Ütközés az adatokban.");
        break;
      case 401:
        if (getActivityToken()) setActivityToken('');
        toast.error("A belépési adatok vagy a munkamenet nem érvényesek.");
        break;
      case 403:
        toast.error("Nem engedélyezett hozzáférés.");
        break;
      case 404:
        await router.navigate("/not-found");
        break;
      case 500:
        router.navigate("/server-error", { state: { error: data } });
        break;
    }

    return Promise.reject(error);
  },
);

export default agent;
