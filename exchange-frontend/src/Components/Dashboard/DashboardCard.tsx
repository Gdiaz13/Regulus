import React from 'react';
import styles from './DashboardCard.module.css';

interface Props {
  title: string;
  value: string | number;
  children?: React.ReactNode;
}

const DashboardCard: React.FC<Props> = ({ title, value, children }) => {
  return (
    <div className={styles.dashboardCard}>
      <div className={styles.dashboardCardInner}>
        <h5 className={styles.dashboardCardTitle}>{title}</h5>
        <span className={styles.dashboardCardValue}>{value}</span>
        {children}
      </div>
    </div>
  );
};

export default DashboardCard;
