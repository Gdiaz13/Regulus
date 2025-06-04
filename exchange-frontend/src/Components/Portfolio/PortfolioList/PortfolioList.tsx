import React, { useState } from 'react'
import PortfolioCard from '../PortfolioCard/PortfolioCard';
import styles from './PortfolioList.module.css';

interface Props {
    portfolioValues: string[];
    onPortfolioDelete: (e: React.SyntheticEvent) => void;
}

const PortfolioList = ({portfolioValues, onPortfolioDelete}: Props) => {
  const [open, setOpen] = useState(false);
  const isEmpty = portfolioValues.length === 0;

  return (
    <section id="portfolio">
      {!isEmpty && (
        <button
          className={styles.portfolioTab}
          onClick={() => setOpen((prev) => !prev)}
          aria-label={open ? 'Hide Portfolio' : 'Show Portfolio'}
        >
          {open ? '→' : 'My Portfolio'}
        </button>
      )}
      <div className={
        isEmpty
          ? styles.portfolioList
          : open
            ? `${styles.portfolioList} ${styles.open}`
            : styles.portfolioList
      } style={isEmpty ? { display: 'none' } : undefined}>
        <h3 className={styles.portfolioTitle}>
          <span className={styles.gradientText}>
            <span className={styles.portfolioIcon}></span>
            <span className={styles.portfolioText}>My Portfolio</span>
          </span>
        </h3>
        <ul>
          {portfolioValues.map((portfolioValue) => (
            <PortfolioCard portfolioValue={portfolioValue} onPortfolioDelete={onPortfolioDelete} key={portfolioValue}/>
          ))}
        </ul>
      </div>
    </section>
  );
};

export default PortfolioList