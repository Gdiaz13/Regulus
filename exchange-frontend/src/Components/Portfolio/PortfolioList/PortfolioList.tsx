import { useState } from 'react';
import type { IPortfolioStock } from '../../../Interfaces/APIResponses/IPortfolioStock';
import PortfolioCard from '../PortfolioCard/PortfolioCard';
import styles from './PortfolioList.module.css';

interface Props {
  portfolioValues: IPortfolioStock[];
  onPortfolioDelete: (id: number) => void;
}

const PortfolioList = ({ portfolioValues, onPortfolioDelete }: Props) => {
  const [open, setOpen] = useState(false);
  if (portfolioValues.length === 0) {
    return <section id="portfolio" />;
  }
  return (
    <section id="portfolio">
      <PortfolioToggle open={open} onToggle={() => setOpen((value) => !value)} />
      <PortfolioPanel open={open} values={portfolioValues} onDelete={onPortfolioDelete} />
    </section>
  );
};

function PortfolioToggle({ open, onToggle }: ToggleProps) {
  return (
    <button {...toggleProps(open, onToggle)}>
      {open ? 'Close' : 'My Portfolio'}
    </button>
  );
}

function toggleProps(open: boolean, onToggle: () => void) {
  return {
    'aria-expanded': open,
    'aria-label': toggleLabel(open),
    className: styles.portfolioTab,
    onClick: onToggle,
    type: 'button' as const,
  };
}

function PortfolioPanel({ open, values, onDelete }: PanelProps) {
  return (
    <div className={portfolioClass(open)}>
      <PortfolioTitle />
      <ul>{values.map((value) => renderPortfolioCard(value, onDelete))}</ul>
    </div>
  );
}

function PortfolioTitle() {
  return (
    <h3 className={styles.portfolioTitle}>
      <span className={styles.gradientText}>
        <span className={styles.portfolioIcon}></span>
        <span className={styles.portfolioText}>My Portfolio</span>
      </span>
    </h3>
  );
}

function renderPortfolioCard(value: IPortfolioStock, onDelete: (id: number) => void) {
  return <PortfolioCard portfolioValue={value} onPortfolioDelete={onDelete} key={value.id} />;
}

function portfolioClass(open: boolean) {
  return open ? `${styles.portfolioList} ${styles.open}` : styles.portfolioList;
}

function toggleLabel(open: boolean) {
  return open ? 'Hide Portfolio' : 'Show Portfolio';
}

type ToggleProps = {
  open: boolean;
  onToggle: () => void;
};

type PanelProps = {
  open: boolean;
  values: IPortfolioStock[];
  onDelete: (id: number) => void;
};

export default PortfolioList;
