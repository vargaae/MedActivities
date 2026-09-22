import { Box, MenuItem, Pagination, TextField, Typography } from "@mui/material";

export default function ListPagination({ total, page, pageSize, onPageChange, onPageSizeChange, label = "tételek", disabled = false }: {
  total: number; page: number; pageSize: number; onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void; label?: string; disabled?: boolean;
}) {
  const count = Math.max(1, Math.ceil(total / pageSize));
  return <Box component="nav" aria-label={`${label} lapozása`} sx={{ display: "flex", flexWrap: "wrap", gap: 2,
    alignItems: "center", justifyContent: "space-between", p: 2, my: 1, borderRadius: 3,
    bgcolor: "background.paper", border: "1px solid", borderColor: "divider", boxShadow: "0 6px 24px rgba(30,70,100,.07)" }}>
    <Typography variant="body2" color="text.secondary" role="status">
      {total === 0 ? "0 találat" : `${(page - 1) * pageSize + 1}–${Math.min(page * pageSize, total)} / ${total} találat`}
    </Typography>
    <Pagination count={count} page={Math.min(page, count)} onChange={(_, value) => onPageChange(value)}
      disabled={disabled || total === 0} color="primary" shape="rounded" showFirstButton showLastButton siblingCount={0}
      getItemAriaLabel={(type, value, selected) => type === "page" ? `${value}. oldal${selected ? ", jelenlegi oldal" : ""}` :
        ({ first: "Első oldal", last: "Utolsó oldal", next: "Következő oldal", previous: "Előző oldal", "start-ellipsis": "További oldalak", "end-ellipsis": "További oldalak" }[type] ?? "Oldalak")}
      sx={{ "& .Mui-selected": { boxShadow: "0 3px 10px rgba(25,118,210,.25)" } }} />
    <TextField select size="small" label="Tétel / oldal" value={pageSize} disabled={disabled}
      onChange={e => onPageSizeChange(Number(e.target.value))} sx={{ minWidth: 120 }}>
      {[10, 20, 30, 50, 100].map(size => <MenuItem key={size} value={size}>{size}</MenuItem>)}
    </TextField>
  </Box>;
}
