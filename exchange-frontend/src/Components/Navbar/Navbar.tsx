import { Link } from 'react-router-dom';
import { useAuth } from '../../Auth/useAuth';
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
      <Link to="/trading-agents" className={styles.link}>Research</Link>
      <Link to="/price-history" className={styles.link}>Prices</Link>
      <Link to="/tcg" className={styles.link}>TCG</Link>
    </div>
  );
}

function NavActions() {
  const auth = useAuth();
  return (
    <div className={styles.actions}>
      <HealthStatus />
      <AuthAction auth={auth} />
      <ThemeToggle />
    </div>
  );
}

function AuthAction({ auth }: { auth: ReturnType<typeof useAuth> }) {
  if (auth.user) {
    return (
      <div className={styles.userSession}>
        <span className={styles.userName}>{auth.user.displayName}</span>
        <button className={styles.authButton} type="button" onClick={auth.logout}>Sign out</button>
      </div>
    );
  }
  return <Link className={styles.authLink} to="/login">Sign in</Link>;
}

export default Navbar;
