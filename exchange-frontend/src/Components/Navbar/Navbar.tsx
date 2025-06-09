import styles from "./Navbar.module.css";
import { ThemeToggle } from "../Theme/ThemeToggle";
import { Link } from "react-router-dom";

interface Props {}

const Navbar = (props: Props) => {
  return (
    <nav className={styles.container}>
      <div className={styles.navbarFlex}>
        <div className={styles.logoGroup}>
          {/* need to add logo here, might make some sort of animated logo with AI */}
          <Link to="/">
            Regulus     
          <img alt="" />
          </Link>
          <div className={styles.menu}>
            <Link to ="/search" className={styles.link}>
              Search
            </Link>
          </div>
        </div>
        <div className={styles.actions}>
        <Link to ="/login" className={styles.link}>
            Login
            </Link>
          <a href="" className={styles.signup}>
            Signup
          </a>
          <ThemeToggle />
        </div>
      </div>
    </nav>
  );
};

export default Navbar;