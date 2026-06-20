import { Menu, X } from 'lucide-react';
import { useState } from 'react';
import { FcHome, FcMoneyTransfer } from 'react-icons/fc';
import { FaScaleBalanced } from 'react-icons/fa6';
import type { IconType } from 'react-icons';
import { NavLink } from 'react-router-dom';
import styles from './Sidebar.module.css';

const navItems: SidebarItem[] = [
  { to: 'company-profile', label: 'Company Profile', Icon: FcHome },
  { to: 'income-statement', label: 'Income Statement', Icon: FcMoneyTransfer },
  { to: 'cashflow-statement', label: 'Cash Flow Statement', Icon: FcMoneyTransfer },
  { to: 'balance-sheet', label: 'Balance Sheet', Icon: FaScaleBalanced },
];

const Sidebar = () => {
  const [open, setOpen] = useState(false);
  const close = () => setOpen(false);
  return (
    <>
      <SidebarToggle open={open} onToggle={() => setOpen((value) => !value)} />
      <SidebarPanel open={open} onNavigate={close} />
    </>
  );
};

function SidebarToggle({ open, onToggle }: ToggleProps) {
  return (
    <button type="button" className={styles.sidebarButton} onClick={onToggle} aria-label={toggleLabel(open)}>
      {open ? <X aria-hidden="true" /> : <Menu aria-hidden="true" />}
    </button>
  );
}

function SidebarPanel({ open, onNavigate }: PanelProps) {
  return (
    <nav className={sidebarClass(open)} aria-label="Company sections">
      <div className={styles.sidebarContent}>
        <div className={styles.sidebarNav}>{navItems.map(renderNavItem(onNavigate))}</div>
      </div>
    </nav>
  );
}

function renderNavItem(onNavigate: () => void) {
  return (item: SidebarItem) => <SidebarLink item={item} onNavigate={onNavigate} key={item.to} />;
}

function SidebarLink({ item, onNavigate }: LinkProps) {
  return (
    <NavLink to={item.to} className={linkClass} onClick={onNavigate}>
      <item.Icon aria-hidden="true" />
      <span className={styles.linkLabel}>{item.label}</span>
    </NavLink>
  );
}

function sidebarClass(open: boolean) {
  return open ? `${styles.sidebar} ${styles.open}` : styles.sidebar;
}

function linkClass({ isActive }: { isActive: boolean }) {
  return isActive ? `${styles.link} ${styles.active}` : styles.link;
}

function toggleLabel(open: boolean) {
  return open ? 'Close company navigation' : 'Open company navigation';
}

type ToggleProps = {
  open: boolean;
  onToggle: () => void;
};

type PanelProps = {
  open: boolean;
  onNavigate: () => void;
};

type LinkProps = {
  item: SidebarItem;
  onNavigate: () => void;
};

type SidebarItem = {
  to: string;
  label: string;
  Icon: IconType;
};

export default Sidebar;
