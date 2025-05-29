import styles from "./Hero.module.css";

interface Props {}

const Hero = (props: Props) => {
  return (
    <section className={styles.heroSection}>
      <div className={styles.heroContainer}>
        <div className={styles.heroContent}>
          <h1 className={styles.heroTitle}>
            Financial data with no news.
          </h1>
          <p className={styles.heroDesc}>
            Search relevant financial documents without fear mongering and fake
            news.
          </p>
          <div className={styles.heroButtonWrap}>
            <a
              href=""
              className={styles.heroButton}
            >
              Get Started
            </a>
          </div>
        </div>
        <div className={styles.heroImageWrap}>
          <img  alt="" />
        </div>
      </div>
    </section>
  );
};

export default Hero;