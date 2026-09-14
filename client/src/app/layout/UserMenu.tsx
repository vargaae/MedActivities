import { useState } from "react";
import { Link, useNavigate } from "react-router";
import { useQueryClient } from "@tanstack/react-query";
import { Avatar, Box, Button, CircularProgress, Divider, ListItemIcon, ListItemText, Menu, MenuItem } from "@mui/material";
import { AccountCircleRounded, AddRounded, LogoutRounded, ManageAccountsRounded, MedicalServicesRounded, PersonRounded } from "@mui/icons-material";
import { useActivityAccess } from "../../lib/hooks/useActivityAccess";
import { changeSession } from "../../lib/api/changeSession";
import { toast } from "react-toastify";

const roleLabels: Record<string, string> = { Admin: "ADMIN", AdmissionsOffice: "Felvételi iroda", Practitioner: "Kezelőorvos", Patient: "Páciens" };

export default function UserMenu() {
  const session = useActivityAccess();
  const cache = useQueryClient();
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [loggingOut, setLoggingOut] = useState(false);
  const roles = session.data?.roles ?? [];
  const label = roles.map((role) => roleLabels[role] ?? role).join(", ");

  async function logout() {
    if (loggingOut) return;
    setLoggingOut(true);
    try {
      await changeSession(cache);
      setAnchorEl(null);
      await navigate("/");
    } catch {
      toast.error("A helyi munkamenet törölve, de a szerveroldali kiléptetés nem volt elérhető.");
    } finally {
      setLoggingOut(false);
    }
  }

  return <>
    <Button id="user-menu-button" aria-controls={anchorEl ? "user-menu" : undefined} aria-haspopup="true" aria-expanded={anchorEl ? "true" : undefined} onClick={(event) => setAnchorEl(event.currentTarget)} color="inherit" size="large" sx={{ fontSize: "0.9rem", textTransform: "none", minWidth: 0 }}>
      <Box display="flex" alignItems="center" gap={1.25}>
        <Avatar sx={{ width: 34, height: 34 }}><AccountCircleRounded /></Avatar>
        <Box component="span" sx={{ display: { xs: "none", sm: "block" }, textAlign: "left" }}>
          <Box component="strong" sx={{ display: "block", fontSize: "0.8rem" }}>{session.data?.userName ?? "Felhasználó"}</Box>
          <Box component="small" sx={{ display: "block", opacity: 0.85 }}>{label}</Box>
        </Box>
      </Box>
    </Button>
    <Menu id="user-menu" anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={() => setAnchorEl(null)} MenuListProps={{ "aria-labelledby": "user-menu-button" }}>
      {session.data?.canCreate && <MenuItem component={Link} to="/createActivity" onClick={() => setAnchorEl(null)}><ListItemIcon><AddRounded fontSize="small" /></ListItemIcon><ListItemText>Új esemény</ListItemText></MenuItem>}
      <MenuItem component={Link} to="/health-records" onClick={() => setAnchorEl(null)}><ListItemIcon><PersonRounded fontSize="small" /></ListItemIcon><ListItemText>Adatlapkezelő</ListItemText></MenuItem>
      <MenuItem component={Link} to="/practitioners" onClick={() => setAnchorEl(null)}><ListItemIcon><MedicalServicesRounded fontSize="small" /></ListItemIcon><ListItemText>Kezelők</ListItemText></MenuItem>
      {roles.includes("Admin") && <MenuItem component={Link} to="/users" onClick={() => setAnchorEl(null)}><ListItemIcon><ManageAccountsRounded fontSize="small" /></ListItemIcon><ListItemText>Felhasználók kezelése</ListItemText></MenuItem>}
      <Divider />
      <MenuItem disabled={loggingOut} onClick={() => void logout()}><ListItemIcon>{loggingOut ? <CircularProgress size={20} /> : <LogoutRounded fontSize="small" />}</ListItemIcon><ListItemText>{loggingOut ? "Kijelentkezés…" : "Kijelentkezés"}</ListItemText></MenuItem>
    </Menu>
  </>;
}
