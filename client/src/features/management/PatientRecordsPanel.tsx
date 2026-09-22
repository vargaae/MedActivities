import { useState } from "react";
import { useSearchParams } from "react-router";
import PaginatedList from "../../app/shared/components/PaginatedList";
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Paper,
  TextField,
  Typography,
} from "@mui/material";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import agent from "../../lib/api/agent";
import { useActivityAccess } from "../../lib/hooks/useActivityAccess";
import { errorText } from "./shared";

type Note = {
  id: string;
  text: string;
  author: string;
  createdAt: string;
  canEdit: boolean;
};
type Document = {
  id: string;
  title: string;
  fileName: string;
  size: number;
  canEdit: boolean;
};
export default function PatientRecordsPanel({
  patientId,
}: {
  patientId: string;
}) {
  const session = useActivityAccess();
  const [params] = useSearchParams();
  const target = params.get("target");
  const cache = useQueryClient();
  const url = "/patients/" + patientId + "/records";
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const [note, setNote] = useState({ id: "", text: "" });
  const [title, setTitle] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [rename, setRename] = useState<Document | null>(null);
  const [remove, setRemove] = useState<{
    kind: "notes" | "documents";
    id: string;
  } | null>(null);
  const notes = useQuery({
    queryKey: ["patient-records", session.version, patientId, "notes"],
    queryFn: async ({ signal }) =>
      (await agent.get<Note[]>(url + "/notes", { signal })).data,
  });
  const documents = useQuery({
    queryKey: ["patient-records", session.version, patientId, "documents"],
    queryFn: async ({ signal }) =>
      (await agent.get<Document[]>(url + "/documents", { signal })).data,
  });
  async function run(action: () => Promise<void>) {
    setBusy(true);
    setError("");
    try {
      await action();
      await cache.invalidateQueries({ queryKey: ["patient-records"] });
    } catch (e) {
      setError(errorText(e));
    } finally {
      setBusy(false);
    }
  }
  async function download(d: Document) {
    await run(async () => {
      const response = await agent.get<Blob>(
        url + "/documents/" + d.id + "/content",
        { responseType: "blob" },
      );
      const blobUrl = URL.createObjectURL(response.data);
      const link = window.document.createElement("a");
      link.href = blobUrl;
      link.download = d.fileName;
      link.click();
      setTimeout(() => URL.revokeObjectURL(blobUrl), 1000);
    });
  }
  return (
    <Box sx={{ display: "grid", gap: 2, mt: 2 }}>
      {error && <Alert severity="error">{error}</Alert>}
      <Typography id="documents" variant="h6">Dokumentumok</Typography>
      <Box
        component="form"
        sx={{ display: "flex", flexWrap: "wrap", gap: 1 }}
        onSubmit={(e) => {
          e.preventDefault();
          void run(async () => {
            if (!file) throw new Error("Válassz fájlt.");
            if (file.size === 0 || file.size > 5 * 1024 * 1024)
              throw new Error(
                "A fájl legfeljebb 5 MB lehet, és nem lehet üres.",
              );
            const data = new FormData();
            data.append("title", title);
            data.append("file", file);
            await agent.post(url + "/documents", data);
            setFile(null);
            setTitle("");
          });
        }}
      >
        <TextField
          label="Dokumentum címe"
          required
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          slotProps={{ htmlInput: { maxLength: 200 } }}
          disabled={busy}
        />
        <Button component="label" disabled={busy}>
          Fájl kiválasztása
          <input
            hidden
            type="file"
            accept=".pdf,.png,.jpg,.jpeg,.txt"
            onChange={(e) => {
              setFile(e.target.files?.[0] ?? null);
              e.target.value = "";
            }}
          />
        </Button>
        <Button type="submit" variant="contained" disabled={busy || !file}>
          Feltöltés
        </Button>
      </Box>
      <Typography variant="body2">
        {file?.name ?? "PDF, PNG, JPEG vagy UTF-8 szöveg; legfeljebb 5 MB."}
      </Typography>
      {documents.isPending && <Typography>Dokumentumok betöltése…</Typography>}
      {documents.isError && (
        <Alert severity="error">A dokumentumok nem tölthetők be.</Alert>
      )}
      {documents.data?.length === 0 && (
        <Typography>Nincs dokumentum.</Typography>
      )}
      <PaginatedList key={`documents-${patientId}`} label="Dokumentumok" items={documents.data ?? []} focusId={target}>
      {(d) => (
        <Paper variant="outlined" key={d.id} sx={{ p: 2, bgcolor: d.id === target ? "action.selected" : undefined }}>
          <Typography fontWeight={600}>{d.title}</Typography>
          <Typography variant="body2">
            {d.fileName} · {Math.ceil(d.size / 1024)} KB
          </Typography>
          <Button disabled={busy} onClick={() => void download(d)}>
            Letöltés
          </Button>
          {d.canEdit && (
            <>
              <Button disabled={busy} onClick={() => setRename(d)}>
                Cím módosítása
              </Button>
              <Button
                color="error"
                disabled={busy}
                onClick={() => setRemove({ kind: "documents", id: d.id })}
              >
                Törlés
              </Button>
            </>
          )}
        </Paper>
      )}
      </PaginatedList>
      <Typography id="notes" variant="h6">Megjegyzések</Typography>
      <Box
        component="form"
        onSubmit={(e) => {
          e.preventDefault();
          void run(async () => {
            if (note.id)
              await agent.put(url + "/notes/" + note.id, { text: note.text });
            else await agent.post(url + "/notes", { text: note.text });
            setNote({ id: "", text: "" });
          });
        }}
      >
        <TextField
          fullWidth
          multiline
          minRows={3}
          label={note.id ? "Megjegyzés módosítása" : "Új megjegyzés"}
          required
          value={note.text}
          onChange={(e) => setNote({ ...note, text: e.target.value })}
          slotProps={{ htmlInput: { maxLength: 2000 } }}
          disabled={busy}
        />
        <Button type="submit" disabled={busy || !note.text.trim()}>
          Mentés
        </Button>
        {note.id && (
          <Button disabled={busy} onClick={() => setNote({ id: "", text: "" })}>
            Mégse
          </Button>
        )}
      </Box>
      {notes.isPending && <Typography>Megjegyzések betöltése…</Typography>}
      {notes.isError && (
        <Alert severity="error">A megjegyzések nem tölthetők be.</Alert>
      )}
      {notes.data?.length === 0 && <Typography>Nincs megjegyzés.</Typography>}
      <PaginatedList key={`notes-${patientId}`} label="Megjegyzések" items={notes.data ?? []} focusId={target}>
      {(n) => (
        <Paper variant="outlined" key={n.id} sx={{ p: 2, bgcolor: n.id === target ? "action.selected" : undefined }}>
          <Typography sx={{ whiteSpace: "pre-wrap" }}>{n.text}</Typography>
          <Typography variant="caption">
            {n.author} · {new Date(n.createdAt).toLocaleString("hu-HU")}
          </Typography>
          {n.canEdit && (
            <Box>
              <Button
                disabled={busy}
                onClick={() => setNote({ id: n.id, text: n.text })}
              >
                Szerkesztés
              </Button>
              <Button
                color="error"
                disabled={busy}
                onClick={() => setRemove({ kind: "notes", id: n.id })}
              >
                Törlés
              </Button>
            </Box>
          )}
        </Paper>
      )}
      </PaginatedList>
      <Dialog
        open={!!rename}
        onClose={() => {
          if (!busy) setRename(null);
        }}
      >
        <DialogTitle>Dokumentum címének módosítása</DialogTitle>
        <DialogContent>
          <TextField
            sx={{ mt: 1 }}
            fullWidth
            label="Cím"
            value={rename?.title ?? ""}
            onChange={(e) => setRename({ ...rename!, title: e.target.value })}
          />
          {error && <Alert severity="error">{error}</Alert>}
        </DialogContent>
        <DialogActions>
          <Button disabled={busy} onClick={() => setRename(null)}>
            Mégse
          </Button>
          <Button
            disabled={busy || !rename?.title.trim()}
            onClick={() =>
              void run(async () => {
                await agent.put(url + "/documents/" + rename!.id, {
                  title: rename!.title,
                });
                setRename(null);
              })
            }
          >
            Mentés
          </Button>
        </DialogActions>
      </Dialog>
      <Dialog
        open={!!remove}
        onClose={() => {
          if (!busy) setRemove(null);
        }}
      >
        <DialogTitle>Törlés megerősítése</DialogTitle>
        <DialogContent>
          Véglegesen törlöd a kiválasztott elemet?
          {error && <Alert severity="error">{error}</Alert>}
        </DialogContent>
        <DialogActions>
          <Button disabled={busy} onClick={() => setRemove(null)}>
            Mégse
          </Button>
          <Button
            color="error"
            disabled={busy}
            onClick={() =>
              void run(async () => {
                await agent.delete(url + "/" + remove!.kind + "/" + remove!.id);
                setRemove(null);
              })
            }
          >
            Törlés
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
