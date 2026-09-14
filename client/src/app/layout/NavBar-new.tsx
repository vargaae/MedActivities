import { useRef, useState } from "react";
import { Link, NavLink, useNavigate } from "react-router";
import { useQuery, useQueryClient } from "@tanstack/react-query";
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
import agent from "../../lib/api/agent";
import AuthDialog from "../../features/home/AuthDialog";

import "./navbar-buttons.css";

const labels: Record<string, string> = {
  Admin: "ADMIN",
  AdmissionsOffice: "Felvételi iroda",
  Practitioner: "Kezelő",
  Patient: "Páciens",
};

export default function NavBar() {
  const { uiStore } = useStore();

  const session = useActivityAccess();
  const cache = useQueryClient();
  const navigate = useNavigate();

  const [menu, setMenu] = useState(false);
  const [authOpen, setAuthOpen] = useState(false);

  const [anchor, setAnchor] = useState<HTMLButtonElement | null>(null);

  const [loggingOut, setLoggingOut] = useState(false);

  /*
   * Annak jelzése, hogy a Popover bezárása után
   * az AuthDialogot kell megnyitni.
   */
  const [openAuthAfterPopoverClose, setOpenAuthAfterPopoverClose] =
    useState(false);

  /*
   * A Popovert megnyitó gomb.
   *
   * A Popover bezárása után ide adjuk vissza
   * kézzel a fókuszt.
   */
  const popoverTrigger = useRef<HTMLButtonElement | null>(null);

  /*
   * A Popover saját tartalma.
   *
   * Nyitáskor ebben keressük meg azt az elemet,
   * amely megkapja az első fókuszt.
   */
  const popoverContent = useRef<HTMLDivElement | null>(null);

  const roles = session.data?.roles ?? [];

  const staff = roles.includes("Admin") || roles.includes("AdmissionsOffice");

  const admin = roles.includes("Admin");

  const me = useQuery({
    queryKey: ["session-me", session.version],

    enabled: session.authenticated && !session.isError,

    retry: false,

    queryFn: async ({ signal }) =>
      (
        await agent.get<{
          userName: string;
        }>("/session/me", {
          signal,
        })
      ).data,
  });

  const roleLabel =
    roles.map((role) => labels[role] ?? role).join(", ") ||
    (session.isError ? "Lejárt munkamenet" : "Belépve");

  const links = [
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

  /*
   * A Popover belépési animációjának kezdetén
   * a fókuszt átvisszük a Popover egyik
   * tényleges vezérlőelemére.
   *
   * Így nem marad fókusz a háttérben lévő
   * #eu-user-button elemen, miközben a MUI
   * aria-hidden="true"-t állít a háttérre.
   */
  function focusPopoverOnEnter() {
    const initialFocus = popoverContent.current?.querySelector<HTMLElement>(
      "[data-popover-initial-focus]",
    );

    initialFocus?.focus({
      preventScroll: true,
    });
  }

  function openUserPopover(event: React.MouseEvent<HTMLButtonElement>) {
    popoverTrigger.current = event.currentTarget;

    /*
     * Levesszük a fókuszt a háttérben maradó
     * trigger gombról még a Popover megnyitása előtt.
     */
    event.currentTarget.blur();

    setAnchor(event.currentTarget);
  }

  function closeUserPopover() {
    setAnchor(null);
  }

  /*
   * Az AuthDialogot nem nyitjuk meg azonnal.
   *
   * Először bezárjuk a Popovert, majd annak
   * onExited eseménye fogja megnyitni a Dialogot.
   */
  function openAuthDialogFromPopover(
    event: React.MouseEvent<HTMLButtonElement>,
  ) {
    event.currentTarget.blur();

    setOpenAuthAfterPopoverClose(true);

    setAnchor(null);
    setMenu(false);
  }

  async function logout() {
    if (loggingOut) return;

    setLoggingOut(true);

    try {
      await cache.cancelQueries();

      cache.clear();

      setActivityToken("");

      setAnchor(null);
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
            onClick={() => setMenu((current) => !current)}
          >
            {menu ? <CloseRounded /> : <MenuRounded />}
          </button>

          <nav
            id="eu-nav-links"
            className={`eu-nav-links ${menu ? "is-open" : ""}`}
            aria-label="Fő navigáció"
            onKeyDown={(event) => {
              if (event.key === "Escape") {
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
                aria-label="Felhasználói menü"
                aria-haspopup="dialog"
                aria-expanded={!!anchor}
                aria-controls={anchor ? "eu-user-popover" : undefined}
                onClick={openUserPopover}
              >
                <AccountCircleRounded fontSize="small" />

                <span className="eu-user-copy">
                  <strong>User menu</strong>

                  {session.authenticated && (
                    <small>{me.data?.userName ?? roleLabel}</small>
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
        /*
         * Ugyanaz az elv, mint a javított
         * Dialogoknál:
         *
         * a MUI ne próbálja meg túl korán
         * visszaállítani a fókuszt, miközben
         * a háttér még aria-hidden.
         */
        disableRestoreFocus
        anchorOrigin={{
          vertical: "bottom",
          horizontal: "right",
        }}
        transformOrigin={{
          vertical: "top",
          horizontal: "right",
        }}
        slotProps={{
          transition: {
            /*
             * A Popover megjelenésekor rögtön
             * saját, látható vezérlőelemre kerül
             * a fókusz.
             */
            onEnter: focusPopoverOnEnter,

            /*
             * Csak a kilépési animáció teljes
             * befejezése után állítjuk vissza
             * a fókuszt.
             */
            onExited: () => {
              /*
               * Ha a Popoverből a login/regisztráció
               * Dialogot nyitjuk meg, akkor NEM
               * adjuk vissza a fókuszt a navbar
               * gombjára.
               *
               * Ehelyett most nyitjuk meg a Dialogot.
               */
              if (openAuthAfterPopoverClose) {
                setOpenAuthAfterPopoverClose(false);

                popoverTrigger.current = null;

                setAuthOpen(true);

                return;
              }

              /*
               * Normál Popover bezárás:
               * visszaállítjuk a fókuszt az azt
               * megnyitó gombra.
               */
              const trigger = popoverTrigger.current;

              popoverTrigger.current = null;

              if (trigger?.isConnected && !trigger.disabled) {
                trigger.focus({
                  preventScroll: true,
                });
              }
            },
          },

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
          ref={popoverContent}
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
                primary={me.data?.userName ?? "Felhasználó"}
                secondary={roleLabel}
              />
            </Box>
          )}

          {admin && (
            <Link
              to="/users"
              className="eu-user-popover-item"
              /*
               * Ha admin van belépve,
               * ez lesz a Popover első
               * fókuszálható eleme.
               */
              data-popover-initial-focus
              onClick={() => {
                setAnchor(null);
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
              /*
               * Kijelentkezett állapotban
               * ez kapja meg a nyitáskori
               * fókuszt.
               */
              data-popover-initial-focus
              onClick={openAuthDialogFromPopover}
            >
              <ArrowOutwardRounded fontSize="small" />

              <span>Belépés / regisztráció</span>
            </button>
          )}

          {session.authenticated && (
            <button
              type="button"
              className="eu-user-popover-item"
              /*
               * Nem admin felhasználónál
               * ez lesz az első fókuszálható
               * elem.
               */
              {...(!admin
                ? {
                    "data-popover-initial-focus": true,
                  }
                : {})}
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
