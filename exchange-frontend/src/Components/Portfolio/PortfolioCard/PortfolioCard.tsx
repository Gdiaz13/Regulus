import React from 'react'
import DeletePortfolio from '../DeletePortfolio/DeletePortfolio';
import styles from './PortfolioCard.module.css';

interface Props {
    portfolioValue: string;
    onPortfolioDelete: (e: React.SyntheticEvent) => void;
}

const PortfolioCard = ({portfolioValue , onPortfolioDelete}: Props) => {
  return (
    <div className={styles.portfolioCard}>
      <p className={styles.portfolioTitle}>{portfolioValue}</p>
      <DeletePortfolio onPortfolioDelete={onPortfolioDelete} portfolioValue={portfolioValue}/>
    </div>
  )
}

export default PortfolioCard