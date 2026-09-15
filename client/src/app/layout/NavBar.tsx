import { useState } from "react";
import { useActivities } from "../../lib/hooks/useActivities";
import { Link, NavLink } from "react-router";
import { Observer } from "mobx-react-lite";
import { Box, CircularProgress, LinearProgress } from "@mui/material";
import {
  MenuRounded,
  CloseRounded,
  HomeRounded,
  EventRounded,
  CalendarMonthRounded,
  PeopleAltRounded,
  MedicalServicesRounded,
  AccountCircleRounded,
} from "@mui/icons-material";
import { useActivityAccess } from "../../lib/hooks/useActivityAccess";
import AuthDialog from "../../features/home/AuthDialog";
import UserMenu from "./UserMenu";
import "./navbar-buttons.css";

export default function NavBar() {
  const { isPending } = useActivities();
  const session = useActivityAccess();

  const [menu, setMenu] = useState(false);
  const [authOpen, setAuthOpen] = useState(false);

  const roles = session.data?.roles ?? [];

  const effectiveRoles = roles.length > 0 ? roles : (session.data?.roles ?? []);
  const staff = effectiveRoles.some((role) =>
    ["admin", "admissionsoffice", "admissions office"].includes(
      role.trim().toLowerCase(),
    ),
  );

  const links = [
    {
      to: "/",
      label: "Kezdőlap",
      Icon: HomeRounded,
    },
    ...(session.authenticated
      ? [
          {
            to: "/health-records",
            label: "Adatlapkezelő",
            Icon: AccountCircleRounded,
          },
        ]
      : []),
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

    ...(staff || effectiveRoles.includes("Practitioner")
      ? [
          {
            to: "/patients",
            label: "Páciensek",
            Icon: PeopleAltRounded,
          },
        ]
      : []),
    ...(session.authenticated
      ? [
          {
            to: "/practitioners",
            label: "Kezelőorvosok",
            Icon: MedicalServicesRounded,
          },
        ]
      : []),
  ];

  function openAuthDialog() {
    setMenu(false);

    const activeElement = document.activeElement;

    if (activeElement instanceof HTMLElement) {
      activeElement.blur();
    }

    requestAnimationFrame(() => {
      setAuthOpen(true);
    });
  }

  return (
    <>
      <header
        className={`eu-nav eu-nav-enhanced${session.data?.roles.includes("Admin") ? " eu-nav-admin" : session.data?.roles.includes("AdmissionsOffice") ? " eu-nav-admissions" : session.data?.roles.includes("Practitioner") ? " eu-nav-practitioner" : ""}`}
      >
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
            <Box
              component="span"
              sx={{
                position: "relative",
                display: { xs: "none", md: "inline-flex" },
                alignItems: "center",
              }}
            >
              <Observer>
                {() =>
                  isPending ? (
                    <CircularProgress
                      size={20}
                      thickness={7}
                      aria-label="Betöltés folyamatban"
                      sx={{
                        color: "black",
                        position: "absolute",
                        top: "30%",
                        left: "105%",
                      }}
                    />
                  ) : null
                }
              </Observer>
            </Box>
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
              {session.authenticated ? (
                <UserMenu />
              ) : (
                <button
                  type="button"
                  className="eu-user-trigger eu-role-pill"
                  onClick={openAuthDialog}
                >
                  <AccountCircleRounded fontSize="small" />
                  <span>Bejelentkezés</span>
                </button>
              )}
            </div>
          </nav>
        </div>
        <Observer>
          {() =>
            isPending ? (
              <LinearProgress
                aria-label="Betöltés folyamatban"
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

      <AuthDialog open={authOpen} onClose={() => setAuthOpen(false)} />
    </>
  );
}
