import { Link } from "react-router-dom";
import { ThemeToggle } from "../Theme/ThemeToggle";
import styles from "./Navbar.module.css";

const Navbar = () => {
  return (
    <nav className={styles.container}>
      <div className={styles.navbarFlex}>
        <div className={styles.logoGroup}>
          <Link to="/">Regulus</Link>
          <div className={styles.menu}>
            <Link to="/search" className={styles.link}>
              Search
            </Link>
          </div>
        </div>
        <div className={styles.actions}>
          <ThemeToggle />
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
