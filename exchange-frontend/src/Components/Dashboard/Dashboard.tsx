import React from 'react'
import styles from './Dashboard.module.css';
import DashboardCard from './DashboardCard';
import { Outlet } from 'react-router-dom';

interface Props {
    children: React.ReactNode;
}

const Dashboard = ({children}: Props) => {
  return (
    <div className={styles.dashboardMain}>
    <div className={styles.dashboardHeader}>
      <div className={styles.dashboardHeaderInner}>
   
      </div>
    </div>
    <div className={styles.dashboardCards}> {children} </div>
    <div className={styles.dashboardCards}>{< Outlet />}</div>
  </div>
  )
}

export default Dashboard