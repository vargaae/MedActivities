import { useState } from "react";
import {
  Avatar,
  Box,
  Button,
  Divider,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
} from "@mui/material";
import {
  Add,
  AccountCircleRounded,
  Logout,
  ManageAccountsRounded,
} from "@mui/icons-material";
import { Link } from "react-router";
import { useAccount } from "../../lib/hooks/useAccount";

const roleLabels: Record<string, string> = {
  Admin: "ADMIN",
  AdmissionsOffice: "Felvételi iroda",
  Practitioner: "Kezelőorvos",
  Patient: "Páciens",
};

export default function UserMenu() {
  const { currentUser, logoutUser } = useAccount();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const roles = currentUser?.roles ?? [];
  const roleLabel = roles.map((role) => roleLabels[role] ?? role).join(", ");
  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) =>
    setAnchorEl(event.currentTarget);
  const handleClose = () => setAnchorEl(null);
  const handleLogout = () => {
    if (logoutUser.isPending) return;
    logoutUser.mutate();
    handleClose();
  };

  return (
    <>
      <Button
        id="basic-button"
        onClick={handleClick}
        color="inherit"
        size="large"
        aria-controls={open ? "basic-menu" : undefined}
        aria-haspopup="true"
        aria-expanded={open ? "true" : undefined}
        sx={{ fontSize: "0.9rem", textTransform: "none", minWidth: 0 }}
      >
        <Box display="flex" alignItems="center" gap={1.25}>
          <Avatar
            src={currentUser?.imageUrl}
            alt="Bejelentkezett felhasználó képe"
            sx={{ width: 34, height: 34 }}
          >
            <AccountCircleRounded />
          </Avatar>
          <Box
            component="span"
            sx={{ display: { xs: "none", sm: "block" }, textAlign: "left" }}
          >
            <Box
              component="strong"
              sx={{ display: "block", fontSize: "0.8rem" }}
            >
              {currentUser?.displayName}
            </Box>
            <Box component="small" sx={{ display: "block", opacity: 0.85 }}>
              {roleLabel}
            </Box>
          </Box>
        </Box>
      </Button>
      <Menu
        id="basic-menu"
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        slotProps={{ list: { "aria-labelledby": "basic-button" } }}
      >
        {currentUser?.canCreate && (
          <MenuItem component={Link} to="/createActivity" onClick={handleClose}>
            <ListItemIcon>
              <Add />
            </ListItemIcon>
            <ListItemText>Új esemény</ListItemText>
          </MenuItem>
        )}
        {roles.includes("Admin") && (
          <MenuItem component={Link} to="/users" onClick={handleClose}>
            <ListItemIcon>
              <ManageAccountsRounded />
            </ListItemIcon>
            <ListItemText>Felhasználók kezelése</ListItemText>
          </MenuItem>
        )}
        <Divider />
        <MenuItem onClick={handleLogout} disabled={logoutUser.isPending}>
          <ListItemIcon>
            <Logout />
          </ListItemIcon>
          <ListItemText>
            {logoutUser.isPending ? "Kijelentkezés…" : "Kijelentkezés"}
          </ListItemText>
        </MenuItem>
      </Menu>
    </>
  );
}
