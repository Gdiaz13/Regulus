import { Link } from 'react-router-dom';
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
    </div>
  );
}

function NavActions() {
  return (
    <div className={styles.actions}>
      <ThemeToggle />
    </div>
  );
}

export default Navbar;
