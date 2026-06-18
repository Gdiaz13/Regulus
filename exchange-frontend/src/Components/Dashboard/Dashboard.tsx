import type { ReactNode } from 'react'
import styles from './Dashboard.module.css';
import { Outlet } from 'react-router-dom';

interface Props {
    children: ReactNode;
    ticker: string;
}

const Dashboard = ({children, ticker}: Props) => {
  return (
    <div className={styles.dashboardMain}>
    <div className={styles.dashboardHeader}>
      <div className={styles.dashboardHeaderInner}>
   
      </div>
    </div>
    <div className={styles.dashboardCards}> {children} </div>
    <div className={styles.dashboardCards}>{< Outlet context={ticker} />}</div>
  </div>
  )
}

export default Dashboard
