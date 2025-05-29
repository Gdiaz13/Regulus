
import styles from "./Navbar.module.css";

interface Props {}

const Navbar = (props: Props) => {
  return (
    <nav className={styles.container}>
      <div className={styles.navbarFlex}>
        <div className={styles.logoGroup}>
          <img  alt="" />
          <div className={styles.menu}>
            <a href="" className={styles.link}>
              Dashboard
            </a>
          </div>
        </div>
        <div className={styles.actions}>
          <div className={styles.login}>Login</div>
          <a href="" className={styles.signup}>
            Signup
          </a>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;