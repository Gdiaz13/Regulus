import { FcHome, FcMoneyTransfer } from 'react-icons/fc';
import { FaScaleBalanced } from 'react-icons/fa6';
import type { IconType } from 'react-icons';
import { Link } from 'react-router-dom';
import styles from './Sidebar.module.css';

const navItems: SidebarItem[] = [
  { to: 'company-profile', label: 'Company Profile', Icon: FcHome },
  { to: 'income-statement', label: 'Income Statement', Icon: FcMoneyTransfer },
  { to: 'cashflow-statement', label: 'Cash Flow Statement', Icon: FcMoneyTransfer },
  { to: 'balance-sheet', label: 'Balance Sheet', Icon: FaScaleBalanced },
];

const Sidebar = () => (
  <nav className={styles.sidebar}>
    <div className={styles.sidebarContent}>
      <div className={styles.sidebarNav}>{navItems.map(renderNavItem)}</div>
    </div>
  </nav>
);

function renderNavItem(item: SidebarItem) {
  return (
    <Link to={item.to} className={styles.link} key={item.to}>
      <item.Icon />
      <h6 className={styles.linkLabel}>{item.label}</h6>
    </Link>
  );
}

type SidebarItem = {
  to: string;
  label: string;
  Icon: IconType;
};

export default Sidebar;
