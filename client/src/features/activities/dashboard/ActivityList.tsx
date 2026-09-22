import { Alert, Box, Button, CircularProgress } from "@mui/material";
import { useState } from "react";
import { format } from "date-fns";
import ActivityCard from "./ActivityCard";
import { useActivityPages } from "../../../lib/hooks/useActivityPages";
import type { ActivityFilter } from "./ActivityFilters";
import ListPagination from "../../../app/shared/components/ListPagination";

type Props = { filter: ActivityFilter; date: [Date, Date] | null; category: string; patientId: string; practitionerId: string };
export default function ActivityList(props: Props) {
  return <ActivityPage key={JSON.stringify(props)} {...props} />;
}
function ActivityPage({ filter, date, category, patientId, practitionerId }: Props) {
  const [now] = useState(() => new Date());
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(30);
  let from = date ? new Date(date[0]) : null;
  let to = date ? new Date(date[1]) : null;
  if (from) from.setHours(0, 0, 0, 0);
  if (to) { to.setHours(0, 0, 0, 0); to.setDate(to.getDate() + 1); }
  if (filter === "future" && (!from || from < now)) from = now;
  if (filter === "history" && (!to || to > now)) to = now;
  const emptyRange = !!from && !!to && from >= to;
  const query = useActivityPages({ patientId: patientId || undefined, practitionerId: practitionerId || undefined,
    category: category || undefined, from: from ? format(from, "yyyy-MM-dd'T'HH:mm:ss") : undefined,
    to: to ? format(to, "yyyy-MM-dd'T'HH:mm:ss") : undefined }, page, pageSize, !emptyRange);
  const activities = emptyRange ? [] : query.data?.items ?? [];
  return <Box sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
    <ListPagination label="Események" total={emptyRange ? 0 : query.data?.totalCount ?? 0}
      page={query.data?.page ?? page} pageSize={pageSize} disabled={query.isFetching}
      onPageChange={setPage} onPageSizeChange={size => { setPageSize(size); setPage(1); }} />
    {query.isLoading && <CircularProgress aria-label="Események betöltése" />}
    {query.isError && !emptyRange && <Alert severity="error">Az eseménylista nem tölthető be.
      <Button onClick={() => void query.refetch()}>Újrapróbálás</Button></Alert>}
    {!query.isLoading && !query.isError && activities.length === 0 && <Alert severity="info">Nincs a szűrésnek megfelelő esemény.</Alert>}
    {activities.map(activity => <ActivityCard key={activity.id} activity={activity} />)}
  </Box>;
}
