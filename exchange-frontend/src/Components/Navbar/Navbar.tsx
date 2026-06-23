import { Link } from 'react-router-dom';
import HealthStatus from '../HealthStatus/HealthStatus';
import { ThemeToggle } from '../Theme/ThemeToggle';
import styles from './Navbar.module.css';

const Navbar = () => (
  <nav className={styles.container}>
    <div className={styles.navbarFlex}>
      <LogoGroup />
      <NavActions />
    </div>
  </nav>
);

function LogoGroup() {
  return (
    <div className={styles.logoGroup}>
      <Link to="/">Regulus</Link>
      <SearchMenu />
    </div>
  );
}

function SearchMenu() {
  return (
    <div className={styles.menu}>
      <Link to="/search" className={styles.link}>Search</Link>
      <Link to="/portfolio" className={styles.link}>Portfolio</Link>
      <Link to="/predictions" className={styles.link}>Predictions</Link>
      <Link to="/price-history" className={styles.link}>Prices</Link>
    </div>
  );
}

function NavActions() {
  return (
    <div className={styles.actions}>
      <HealthStatus />
      <ThemeToggle />
    </div>
  );
}

export default Navbar;
