import { useState } from "react";
import { Link, NavLink, useNavigate } from "react-router";
import { useQueryClient } from "@tanstack/react-query";
import { Observer } from "mobx-react-lite";
import {
  Box,
  LinearProgress,
  ListItemIcon,
  ListItemText,
  Popover,
} from "@mui/material";
import {
  AddRounded,
  ArrowOutwardRounded,
  LogoutRounded,
  MenuRounded,
  CloseRounded,
  ShieldOutlined,
  HomeRounded,
  EventRounded,
  CalendarMonthRounded,
  PeopleAltRounded,
  MedicalServicesRounded,
  ManageAccountsRounded,
  AccountCircleRounded,
  ExpandMoreRounded,
} from "@mui/icons-material";
import { useStore } from "../../lib/hooks/useStore";
import { useActivityAccess } from "../../lib/hooks/useActivityAccess";
import { setActivityToken } from "../../lib/api/activitySession";
import AuthDialog from "../../features/home/AuthDialog";
import "./navbar-buttons.css";

const labels: Record<string, string> = {
  Admin: "ADMIN",
  AdmissionsOffice: "Felvételi iroda",
  Practitioner: "Kezelő",
  Patient: "Páciens",
};

function displayRole(role: string) {
  const normalized = role.trim().toLowerCase();
  if (normalized === "admin") return "ADMIN";
  if (normalized === "admissionsoffice" || normalized === "admissions office")
    return "Felvételi iroda";
  if (normalized === "practitioner") return "Kezelőorvos";
  if (normalized === "patient") return "Páciens";
  return labels[role] ?? role;
}

export default function NavBar() {
  const { uiStore } = useStore();
  const session = useActivityAccess();
  const cache = useQueryClient();
  const navigate = useNavigate();

  const [menu, setMenu] = useState(false);
  const [authOpen, setAuthOpen] = useState(false);

  const [anchor, setAnchor] = useState<HTMLButtonElement | null>(null);

  const [loggingOut, setLoggingOut] = useState(false);

  const roles = session.data?.roles ?? [];

  const effectiveRoles = roles.length > 0 ? roles : (session.data?.roles ?? []);
  const staff = effectiveRoles.some((role) =>
    ["admin", "admissionsoffice", "admissions office"].includes(
      role.trim().toLowerCase(),
    ),
  );
  const admin = effectiveRoles.some(
    (role) => role.trim().toLowerCase() === "admin",
  );
  const roleLabel =
    effectiveRoles.map(displayRole).join(", ") ||
    (session.isError ? "" : "Sikeres bejelentkezés");

  const links = [
    ...(session.authenticated ? [{ to: "/health-records", label: "Betegadatlapok", Icon: AccountCircleRounded }] : []),
    {
      to: "/",
      label: "Kezdőlap",
      Icon: HomeRounded,
    },
    {
      to: "/activities",
      label: "Események",
      Icon: EventRounded,
    },
    {
      to: "/booking",
      label: "Időpontfoglalás",
      Icon: CalendarMonthRounded,
    },

    ...(session.data?.canCreate
      ? [
          {
            to: "/createActivity",
            label: "Új esemény",
            Icon: AddRounded,
          },
        ]
      : []),

    ...(staff
      ? [
          {
            to: "/patients",
            label: "Páciensek",
            Icon: PeopleAltRounded,
          },
          {
            to: "/practitioners",
            label: "Kezelők",
            Icon: MedicalServicesRounded,
          },
        ]
      : []),
  ];

  function closeUserPopover() {
    setAnchor(null);
  }

  function openAuthDialog() {
    closeUserPopover();
    setMenu(false);

    const activeElement = document.activeElement;

    if (activeElement instanceof HTMLElement) {
      activeElement.blur();
    }

    requestAnimationFrame(() => {
      setAuthOpen(true);
    });
  }

  async function logout() {
    if (loggingOut) return;

    setLoggingOut(true);

    try {
      await cache.cancelQueries();
      cache.clear();

      setActivityToken("");

      closeUserPopover();
      setMenu(false);

      await navigate("/");
    } finally {
      setLoggingOut(false);
    }
  }

  return (
    <>
      <header className="eu-nav eu-nav-enhanced">
        <div className="eu-nav-inner">
          <Link
            to="/"
            className="eu-brand"
            aria-label="EgészségÚt kezdőlap"
            onClick={() => setMenu(false)}
          >
            <img
              src="/brand/EgeszsegUt_logo_light.svg"
              alt="EgészségÚt"
              width="231"
              height="45"
            />
          </Link>

          <button
            type="button"
            className="eu-mobile-toggle"
            aria-label={menu ? "Menü bezárása" : "Menü megnyitása"}
            aria-expanded={menu}
            aria-controls="eu-nav-links"
            onClick={() => setMenu(!menu)}
          >
            {menu ? <CloseRounded /> : <MenuRounded />}
          </button>

          <nav
            id="eu-nav-links"
            className={`eu-nav-links ${menu ? "is-open" : ""}`}
            aria-label="Fő navigáció"
            onKeyDown={(e) => {
              if (e.key === "Escape") {
                setMenu(false);
              }
            }}
          >
            {links.map(({ to, label, Icon }) => (
              <NavLink
                key={to}
                to={to}
                end={to === "/"}
                className="eu-nav-action"
                onClick={() => setMenu(false)}
              >
                <Icon fontSize="small" />
                <span>{label}</span>
              </NavLink>
            ))}

            <div className="eu-nav-session">
              <button
                type="button"
                id="eu-user-button"
                className="eu-user-trigger eu-role-pill"
                aria-label="User menu"
                aria-haspopup="dialog"
                aria-expanded={!!anchor}
                aria-controls={anchor ? "eu-user-popover" : undefined}
                onClick={(e) => setAnchor(e.currentTarget)}
              >
                <AccountCircleRounded fontSize="small" />

                <span className="eu-user-copy">
                  {session.authenticated ? (
                    <>
                      <strong>{session.data?.userName || "Felhasználó"}</strong>
                      <small>{roleLabel}</small>
                    </>
                  ) : (
                    <span>Bejelentkezés</span>
                  )}
                </span>

                <ExpandMoreRounded
                  fontSize="small"
                  className={
                    anchor ? "eu-user-chevron is-open" : "eu-user-chevron"
                  }
                />
              </button>
            </div>
          </nav>
        </div>

        <Observer>
          {() =>
            uiStore.isLoading ? (
              <LinearProgress
                sx={{
                  position: "absolute",
                  left: 0,
                  right: 0,
                  bottom: 0,
                  height: 2,
                }}
              />
            ) : null
          }
        </Observer>
      </header>

      <Popover
        id="eu-user-popover"
        open={!!anchor}
        anchorEl={anchor}
        onClose={closeUserPopover}
        disableScrollLock
        anchorOrigin={{
          vertical: "bottom",
          horizontal: "right",
        }}
        transformOrigin={{
          vertical: "top",
          horizontal: "right",
        }}
        slotProps={{
          paper: {
            sx: {
              mt: 1,
              minWidth: 230,
              borderRadius: 3,
              overflow: "hidden",
            },
          },
        }}
      >
        <Box
          role="group"
          aria-labelledby="eu-user-button"
          sx={{
            py: 0.5,
          }}
        >
          {session.authenticated && (
            <Box
              sx={{
                display: "flex",
                alignItems: "center",
                gap: 1.25,
                px: 2,
                py: 1.25,
                color: "text.secondary",
              }}
            >
              <ListItemIcon
                sx={{
                  minWidth: 32,
                }}
              >
                <ShieldOutlined fontSize="small" />
              </ListItemIcon>

              <ListItemText
                primary={session.data?.userName || "Felhasználó"}
                secondary={roleLabel}
              />
            </Box>
          )}

          {admin && (
            <Link
              to="/users"
              className="eu-user-popover-item"
              onClick={() => {
                closeUserPopover();
                setMenu(false);
              }}
            >
              <ManageAccountsRounded fontSize="small" />
              <span>Felhasználók kezelése</span>
            </Link>
          )}

          {!session.authenticated && (
            <button
              type="button"
              className="eu-user-popover-item"
              onClick={openAuthDialog}
            >
              <ArrowOutwardRounded fontSize="small" />
              <span>Belépés / regisztráció</span>
            </button>
          )}

          {session.authenticated && (
            <button
              type="button"
              className="eu-user-popover-item"
              disabled={loggingOut}
              onClick={() => void logout()}
            >
              <LogoutRounded fontSize="small" />
              <span>{loggingOut ? "Kijelentkezés..." : "Kijelentkezés"}</span>
            </button>
          )}
        </Box>
      </Popover>

      <AuthDialog open={authOpen} onClose={() => setAuthOpen(false)} />
    </>
  );
}
