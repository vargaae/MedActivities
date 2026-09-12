// A token csak memóriában él; frissítés után új belépés szükséges.
let token = '';
let version = 0;
const listeners = new Set<() => void>();
export const getActivityToken = () => token;
export const getActivitySessionVersion = () => version;
export function setActivityToken(value: string) {
    token = value; version++; listeners.forEach(listener => listener());
}
export function subscribeActivitySession(listener: () => void) {
    listeners.add(listener); return () => { listeners.delete(listener); };
}