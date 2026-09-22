import { Fragment, useState, type ReactNode } from "react";
import ListPagination from "./ListPagination";

export default function PaginatedList<T extends { id: string }>({ items, children, label, focusId, initialPageSize = 10 }: {
  items: T[]; children: (item: T) => ReactNode; label: string; focusId?: string | null; initialPageSize?: number;
}) {
  const [pageSize, setPageSize] = useState(initialPageSize);
  const [selection, setSelection] = useState<{ page: number; focusId?: string | null } | null>(null);
  const focusIndex = focusId ? items.findIndex(item => item.id === focusId) : -1;
  const requested = selection && selection.focusId === focusId ? selection.page : focusIndex < 0 ? 1 : Math.floor(focusIndex / pageSize) + 1;
  const page = Math.min(Math.max(1, requested), Math.max(1, Math.ceil(items.length / pageSize)));
  return <>
    <ListPagination label={label} total={items.length} page={page} pageSize={pageSize}
      onPageChange={value => setSelection({ page: value, focusId })}
      onPageSizeChange={size => { setPageSize(size); setSelection({ page: 1, focusId }); }} />
    {items.slice((page - 1) * pageSize, page * pageSize).map(item => <Fragment key={item.id}>{children(item)}</Fragment>)}
  </>;
}
