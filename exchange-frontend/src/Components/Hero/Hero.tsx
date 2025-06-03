import styles from "./Hero.module.css";
import stars from  "./stars.jpg"
import { Link } from "react-router-dom";

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
          {/* probably going to make this into a live chart later */}
          <img src={stars}  alt="" />
        </div>
      </div>
    </section>
  );
};

export default Hero;