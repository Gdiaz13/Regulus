import styles from "./Hero.module.css";
import { Link } from "react-router-dom";
import Spline from "@splinetool/react-spline";

interface Props {}

const Hero = (props: Props) => {
  return (
    <section className={styles.heroSection}>
      <div className={styles.heroContainer}>
        <div className={styles.heroContent}>
          <h1 className={styles.heroTitle}>
            Illuminate your financial universe.
          </h1>
          <p className={styles.heroDesc}>
            Discover data and insights with clarity—no noise, no hype, just the
            brilliance of Regulus.
          </p>
          <div className={styles.heroButtonWrap}>
            <Link
              to = "/search"
              className={styles.heroButton}
            >
              Get Started
            </Link>
          </div>
        </div>
        <div className={styles.heroImageWrap}>
          <Spline scene="https://prod.spline.design/NOJJe08001V5hvjW/scene.splinecode" />
        </div>
      </div>
    </section>
  );
};

export default Hero;