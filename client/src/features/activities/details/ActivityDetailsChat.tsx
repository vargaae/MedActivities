import { useEffect, useRef, useState, useSyncExternalStore } from "react";
import { Alert, Box, Button, Card, CardContent, TextField, Typography } from "@mui/material";
import { HubConnectionBuilder, HubConnectionState, LogLevel, type HubConnection } from "@microsoft/signalr";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useSearchParams } from "react-router";
import { getActivitySessionVersion, getActivityToken, subscribeActivitySession } from "../../../lib/api/activitySession";
import agent from "../../../lib/api/agent";
import ListPagination from "../../../app/shared/components/ListPagination";

type Comment = { id: string; displayName: string; body: string; createdAt: string };
export default function ActivityDetailsChat({ activityId }: { activityId: string }) {
  const sessionVersion = useSyncExternalStore(subscribeActivitySession, getActivitySessionVersion);
  const token = getActivityToken();
  const cache = useQueryClient();
  const [params, setParams] = useSearchParams();
  const target = params.get("comment");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [body, setBody] = useState("");
  const [error, setError] = useState("");
  const [connected, setConnected] = useState(false);
  const [sending, setSending] = useState(false);
  const connection = useRef<HubConnection | null>(null);
  const chatRef = useRef<HTMLDivElement | null>(null);
  const comments = useQuery({
    queryKey: ["comments", activityId, sessionVersion, page, pageSize, target],
    queryFn: async ({ signal }) => (await agent.get<{ items: Comment[]; page: number; totalCount: number }>(
      "/activities/" + activityId + "/comments", { signal, params: { page, pageSize, commentId: target || undefined } })).data,
    enabled: !!token,
  });
  useEffect(() => {
    if (target && comments.data) chatRef.current?.scrollIntoView({ block: "center" });
  }, [target, comments.data]);
  useEffect(() => {
    if (!token) return;
    let active = true;
    const base = (import.meta.env.VITE_API_URL ?? "/api").replace(/\/$/, "");
    const hub = new HubConnectionBuilder()
      .withUrl(base + "/chat?activityId=" + encodeURIComponent(activityId), { accessTokenFactory: getActivityToken, withCredentials: false })
      .withAutomaticReconnect().configureLogging(LogLevel.None).build();
    connection.current = hub;
    const reload = () => { void cache.invalidateQueries({ queryKey: ["comments", activityId] }); };
    hub.on("CommentAdded", reload);
    hub.onreconnecting(() => { if (active) setConnected(false); });
    hub.onreconnected(() => { if (active) { setConnected(true); setError(""); reload(); } });
    hub.onclose(() => { if (active) { setConnected(false); setError("A chatkapcsolat megszakadt. Frissítsd az oldalt."); } });
    const timer = window.setTimeout(() => {
      void hub.start().then(async () => {
        if (!active) { await hub.stop(); return; }
        setConnected(true); setError(""); reload();
      }).catch(() => { if (active) setError("A chat nem kapcsolódott. Ellenőrizd a belépést és az API-t."); });
    }, 0);
    return () => { active = false; window.clearTimeout(timer); connection.current = null; void hub.stop(); };
  }, [activityId, sessionVersion, token, cache]);
  function changePage(value: number) {
    setPage(value);
    if (target) { const next = new URLSearchParams(params); next.delete("comment"); setParams(next, { replace: true }); }
  }
  return <Card id="chat" ref={chatRef} sx={{ mt: 3 }}><CardContent>
    <Typography variant="h6">Eseményhez tartozó beszélgetés</Typography>
    <Typography variant="body2" color="text.secondary">Legújabb üzenetek elöl. A korábbi üzenetek a lapozóval érhetők el.</Typography>
    {error && <Alert severity="error" sx={{ my: 1 }}>{error}</Alert>}
    {comments.isError && <Alert severity="error">Az üzenetek nem tölthetők be.</Alert>}
    <ListPagination label="Üzenetek" total={comments.data?.totalCount ?? 0} page={comments.data?.page ?? page}
      pageSize={pageSize} onPageChange={changePage} onPageSizeChange={size => { setPageSize(size); changePage(1); }} disabled={comments.isFetching} />
    <Box role="log" aria-label="Chatüzenetek" aria-live="polite" sx={{ maxHeight: 480, overflowY: "auto", my: 2 }}>
      {comments.data?.items.map(c => <Box key={c.id} sx={{ p: 1.5, borderBottom: "1px solid", borderColor: "divider", bgcolor: c.id === target ? "action.selected" : undefined }}>
        <Typography variant="subtitle2">{c.displayName} · {new Date(c.createdAt.endsWith("Z") ? c.createdAt : c.createdAt + "Z").toLocaleString("hu-HU")}</Typography>
        <Typography sx={{ whiteSpace: "pre-wrap", overflowWrap: "anywhere" }}>{c.body}</Typography>
      </Box>)}
      {comments.data?.items.length === 0 && <Typography>Még nincs üzenet. Indíts beszélgetést.</Typography>}
    </Box>
    <Box component="form" sx={{ display: "grid", gap: 1 }} onSubmit={async e => {
      e.preventDefault(); const hub = connection.current;
      if (!body.trim() || sending || hub?.state !== HubConnectionState.Connected) return;
      setSending(true); setError("");
      try { await hub.invoke("SendComment", body); setBody(""); changePage(1); await cache.invalidateQueries({ queryKey: ["comments", activityId] }); }
      catch { setError("Az üzenet nem küldhető el. Ellenőrizd a kapcsolatot és a jogosultságodat."); }
      finally { setSending(false); }
    }}>
      <TextField label="Üzenet" multiline minRows={2} value={body} onChange={e => setBody(e.target.value)} disabled={!connected || sending}
        slotProps={{ htmlInput: { maxLength: 2000 } }} helperText={body.length + "/2000 karakter"} />
      <Button variant="contained" type="submit" disabled={!connected || sending || !body.trim()}>{sending ? "Küldés…" : "Küldés"}</Button>
    </Box>
  </CardContent></Card>;
}
