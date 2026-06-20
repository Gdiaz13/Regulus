import type { ReactNode } from 'react';
import { Outlet } from 'react-router-dom';
import styles from './Dashboard.module.css';

type Props = {
  children: ReactNode;
  ticker: string;
};

const Dashboard = ({ children, ticker }: Props) => (
  <main className={styles.dashboardMain}>
    <section className={styles.dashboardSummary}>{children}</section>
    <section className={styles.dashboardDetails}>
      <Outlet context={ticker} />
    </section>
  </main>
);

export default Dashboard;
