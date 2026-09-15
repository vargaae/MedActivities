import { useState } from "react";
import { Link } from "react-router";
import {
  BadgeOutlined,
  MedicalServicesOutlined,
  PersonOutlineRounded,
  ArrowForwardRounded,
  ArrowOutwardRounded,
  CalendarMonthOutlined,
  CheckRounded,
  HubOutlined,
} from "@mui/icons-material";
import AuthDialog from "./AuthDialog";

export default function HomePage() {
  const [authOpen, setAuthOpen] = useState(false);
  return (
    <div className="eu-home">
      <section className="eu-hero" aria-labelledby="home-heading">
        <div className="eu-hero-copy">
          <div className="eu-eyebrow">
            <span /> EGY LÉPÉSSEL KÖZELEBB A JÖVŐ EGÉSZSÉGÜGYI ELLÁTÁSÁHOZ
          </div>
          <h1 id="home-heading">
            Kövesd és kezeld
            <br />
            <span>az egészségügyi eseményeket</span>
          </h1>
          <p className="eu-lead">
            Páciensek, kezelőorvosok és egészségügyi szakdolgozók számára,
            <br className="eu-desktop-break" /> átlátható kapcsolatokat teremt,
            több figyelem kerül arra, ami számít.
          </p>
          <div className="eu-hero-actions">
            <button
              className="eu-button eu-button-primary"
              onClick={() => setAuthOpen(true)}
            >
              Belépés / regisztráció <ArrowForwardRounded />
            </button>
            <Link className="eu-text-link" to="/activities">
              Események megtekintése <ArrowOutwardRounded fontSize="small" />
            </Link>
          </div>
        </div>
        <div className="eu-hero-art" aria-hidden="true">
          <div className="eu-orbit eu-orbit-outer" />
          <div className="eu-orbit eu-orbit-inner" />
          <span className="eu-orbit-dot eu-dot-one" />
          <span className="eu-orbit-dot eu-dot-two" />
          <div className="eu-center-card">
            <img
              src="/brand/EgeszsegUt_cross_icon_blue.svg"
              alt=""
              width="84"
              height="84"
            />
            <span className="eu-center-label">EGÉSZSÉGÚT</span>
            <h2>
              Te vagy a<br />
              középpontban.
            </h2>
            <p>
              A következő lépéshez
              <br />
              együtt érkezünk.
            </p>
            <div className="eu-center-line">
              <span />
              <span />
              <span />
            </div>
          </div>
          <div className="eu-floating eu-floating-patient">
            <span className="eu-float-icon">
              <PersonOutlineRounded />
            </span>
            <div>
              <small>A TE NÉZŐPONTOD</small>
              <strong>Páciensek</strong>
            </div>
            <span className="eu-float-check">
              <CheckRounded fontSize="small" />
            </span>
          </div>
          <div className="eu-floating eu-floating-doctor">
            <span className="eu-float-icon">
              <MedicalServicesOutlined />
            </span>
            <div>
              <small>KÖZÖS FIGYELEM</small>
              <strong>Kezelőorvosok</strong>
            </div>
          </div>
          <div className="eu-floating eu-floating-event">
            <span className="eu-float-icon">
              <CalendarMonthOutlined />
            </span>
            <div>
              <small>LÉPÉSRŐL LÉPÉSRE</small>
              <strong>Események</strong>
            </div>
            <span className="eu-event-lines">
              <i />
              <i />
              <i />
            </span>
          </div>
          <div className="eu-art-caption">MINDEN KAPCSOLAT SZÁMÍT</div>
        </div>
      </section>
      <section className="eu-benefits" aria-label="Az EgészségÚt lehetőségei">
        <article>
          <span className="eu-benefit-icon">
            <CalendarMonthOutlined />
          </span>
          <div>
            <h2>Átlátható események</h2>
            <p>A következő lépés és az előzmények egy helyen.</p>
          </div>
        </article>
        <article>
          <span className="eu-benefit-icon">
            <HubOutlined />
          </span>
          <div>
            <h2>Összekapcsolt gondoskodás</h2>
            <p>A páciens és a kezelő ugyanazt az eseményt látja.</p>
          </div>
        </article>
        <article>
          <span className="eu-benefit-icon">
            <BadgeOutlined />
          </span>
          <div>
            <h2>Mindenkinek saját nézőpont</h2>
            <p>A feladatokhoz igazodó hozzáférés és lehetőségek.</p>
          </div>
        </article>
      </section>
      <footer className="eu-home-footer">
        <span>EgészségÚt</span>
        <span>Együtt, a következő lépésért.</span>
      </footer>
      <AuthDialog open={authOpen} onClose={() => setAuthOpen(false)} />
    </div>
  );
}
