import { Link } from 'react-router-dom';
import styles from './NotFoundPage.module.css';

const NotFoundPage = () => (
  <main className={styles.page}>
    <h1>Page not found</h1>
    <p>The route you opened is not part of Regulus.</p>
    <Link to="/search">Search companies</Link>
  </main>
);

export default NotFoundPage;
