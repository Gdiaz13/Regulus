import Spline from '@splinetool/react-spline';
import { Link } from 'react-router-dom';
import styles from './Hero.module.css';

const Hero = () => (
  <section className={styles.heroSection}>
    <div className={styles.heroContainer}>
      <HeroContent />
      <HeroScene />
    </div>
  </section>
);

function HeroContent() {
  return (
    <div className={styles.heroContent}>
      <h1 className={styles.heroTitle}>Illuminate your financial universe.</h1>
      <HeroDescription />
      <HeroButton />
    </div>
  );
}

function HeroDescription() {
  return (
    <p className={styles.heroDesc}>
      Discover data and insights with clarity - no noise, no hype, just the brilliance of Regulus.
    </p>
  );
}

function HeroButton() {
  return (
    <div className={styles.heroButtonWrap}>
      <Link to="/search" className={styles.heroButton}>Get Started</Link>
    </div>
  );
}

function HeroScene() {
  return (
    <div className={styles.heroImageWrap}>
      <Spline scene="https://prod.spline.design/NOJJe08001V5hvjW/scene.splinecode" />
    </div>
  );
}

export default Hero;
