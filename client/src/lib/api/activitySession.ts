// Tab-scoped storage survives reload, but is discarded when the tab closes.
// A session change is synchronized to other open tabs of the same origin.
const key = "medactivities.session";
const channel =
  typeof BroadcastChannel !== "undefined" ? new BroadcastChannel(key) : null;
let token = sessionStorage.getItem(key) ?? "";
let version = 0;
const listeners = new Set<() => void>();
export const getActivityToken = () => token;
export const getActivitySessionVersion = () => version;
export function setActivityToken(value: string, broadcast = true) {
  token = value;
  if (value) sessionStorage.setItem(key, value);
  else sessionStorage.removeItem(key);
  version++;
  listeners.forEach((listener) => listener());
  if (broadcast) channel?.postMessage(value);
}
if (channel)
  channel.onmessage = (event) => {
    if (typeof event.data === "string") setActivityToken(event.data, false);
  };
export function subscribeActivitySession(listener: () => void) {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}
