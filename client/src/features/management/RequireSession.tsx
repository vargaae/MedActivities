import type { ReactNode } from "react";
import { Alert, Button } from "@mui/material";
import { useActivityAccess } from "../../lib/hooks/useActivityAccess";
import ActivityLogin from "../activities/form/ActivityLogin";
import { Link } from "react-router";
export default function RequireSession({
  children,
  roles,
}: {
  children: ReactNode;
  roles?: string[];
}) {
  const session = useActivityAccess();
  if (!session.authenticated) return <ActivityLogin />;
  if (session.isLoading) return <p>Jogosultságok betöltése…</p>;
  if (session.isError)
    return (
      <Alert severity="error">
        A munkamenet nem érvényes. A felhasználói menüben lépj ki és jelentkezz
        be újra.
      </Alert>
    );
  if (roles && !session.data?.roles.some((role) => roles.includes(role)))
    return (
      <Alert severity="warning">
        Ehhez az oldalhoz nincs jogosultságod.{" "}
        <Button component={Link} to="/activities">
          Események
        </Button>
      </Alert>
    );
  return children;
}
