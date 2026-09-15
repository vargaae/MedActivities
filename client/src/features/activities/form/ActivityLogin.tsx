import { Alert, Button } from "@mui/material";
import { useState } from "react";
import AuthDialog from "../../home/AuthDialog";
import { useActivityAccess } from "../../../lib/hooks/useActivityAccess";
export default function ActivityLogin() {
  const session = useActivityAccess();
  const [open, setOpen] = useState(false);
  if (session.authenticated) return null;
  return (
    <>
      <Alert severity="info">A folytatáshoz jelentkezzen be.</Alert>
      <Button onClick={() => setOpen(true)}>Belépés / regisztráció</Button>
      <AuthDialog open={open} onClose={() => setOpen(false)} />
    </>
  );
}
