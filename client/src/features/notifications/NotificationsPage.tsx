import { useState } from "react";
import { Alert, Box, Button, Chip, CircularProgress, FormControlLabel, Paper, Switch, Typography } from "@mui/material";
import { ChatBubbleOutline, DescriptionOutlined, PersonOutline, NotificationsNone } from "@mui/icons-material";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router";
import agent from "../../lib/api/agent";
import { useActivityAccess } from "../../lib/hooks/useActivityAccess";
import { useUnreadNotifications } from "../../lib/hooks/useNotifications";
import ListPagination from "../../app/shared/components/ListPagination";
import { errorText } from "../management/shared";

type Notification = { id: string; message: string; kind: string; createdAtUtc: string; readAtUtc: string | null };
export default function NotificationsPage() {
  const session = useActivityAccess();
  const cache = useQueryClient();
  const navigate = useNavigate();
  const count = useUnreadNotifications();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [unreadOnly, setUnreadOnly] = useState(false);
  const [message, setMessage] = useState("");
  const query = useQuery({
    queryKey: ["notifications", session.version, "list", page, pageSize, unreadOnly],
    queryFn: async ({ signal }) => (await agent.get<{ items: Notification[]; totalCount: number }>("/notifications",
      { params: { page, pageSize, unreadOnly }, signal })).data,
    enabled: session.authenticated,
    refetchInterval: 15_000,
  });
  const read = useMutation({
    mutationFn: async (id: string) => (await agent.put<{ url: string | null; message: string | null }>(`/notifications/${id}/read`)).data,
    onSuccess: async result => {
      await cache.invalidateQueries({ queryKey: ["notifications"] });
      if (result.url) navigate(result.url);
      else { setMessage(result.message ?? "Az elem már nem érhető el."); setPage(1); }
    },
  });
  const readAll = useMutation({
    mutationFn: () => agent.put("/notifications/read-all"),
    onSuccess: async () => { setPage(1); await cache.invalidateQueries({ queryKey: ["notifications"] }); },
  });
  return <Box sx={{ display: "grid", gap: 2 }}>
    <Box sx={{ display: "flex", flexWrap: "wrap", alignItems: "center", gap: 2 }}>
      <NotificationsNone color="primary" fontSize="large" />
      <Typography variant="h4">Értesítések</Typography>
      <Chip color="primary" label={`${count.data?.unreadCount ?? 0} olvasatlan`} />
    </Box>
    <Box sx={{ display: "flex", flexWrap: "wrap", justifyContent: "space-between" }}>
      <FormControlLabel label="Csak olvasatlan értesítések" control={<Switch checked={unreadOnly}
        onChange={(_, value) => { setUnreadOnly(value); setPage(1); }} />} />
      <Button onClick={() => readAll.mutate()} disabled={readAll.isPending || !count.data?.unreadCount}>Összes megjelölése olvasottként</Button>
    </Box>
    {message && <Alert severity="info">{message}</Alert>}
    {(read.isError || readAll.isError) && <Alert severity="error">{errorText(read.error ?? readAll.error)}</Alert>}
    {query.isError && <Alert severity="error">Az értesítések nem tölthetők be. <Button onClick={() => void query.refetch()}>Újrapróbálás</Button></Alert>}
    {query.isLoading && <CircularProgress aria-label="Értesítések betöltése" />}
    <ListPagination label="Értesítések" total={query.data?.totalCount ?? 0} page={page} pageSize={pageSize}
      disabled={query.isFetching} onPageChange={setPage} onPageSizeChange={size => { setPageSize(size); setPage(1); }} />
    {query.data?.items.length === 0 && <Alert severity="info">{unreadOnly ? "Minden értesítést elolvastál." : "Még nincs értesítésed."}</Alert>}
    {query.data?.items.map(item => <Paper key={item.id} variant="outlined" sx={{ borderRadius: 3, overflow: "hidden",
      borderLeft: "4px solid", borderLeftColor: item.readAtUtc ? "divider" : "primary.main", bgcolor: item.readAtUtc ? "background.paper" : "action.hover" }}>
      <Button fullWidth disabled={read.isPending} onClick={() => read.mutate(item.id)}
        sx={{ p: 2.5, gap: 2, textAlign: "left", justifyContent: "flex-start", textTransform: "none", color: "text.primary" }}>
        {item.kind === "comment" ? <ChatBubbleOutline color="primary" /> : item.kind === "document" ? <DescriptionOutlined color="primary" /> : <PersonOutline color="primary" />}
        <Box sx={{ flex: 1 }}>
          <Typography fontWeight={item.readAtUtc ? 400 : 700}>{item.message}</Typography>
          <Typography variant="caption" color="text.secondary">{new Date(item.createdAtUtc.endsWith("Z") ? item.createdAtUtc : item.createdAtUtc + "Z").toLocaleString("hu-HU")}</Typography>
        </Box>
        {!item.readAtUtc && <Chip label="Új" color="primary" size="small" />}
      </Button>
    </Paper>)}
  </Box>;
}
