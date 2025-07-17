import React from 'react'
import DeletePortfolio from '../DeletePortfolio/DeletePortfolio';
import styles from './PortfolioCard.module.css';
import { Link } from 'react-router-dom';

interface Props {
    portfolioValue: string;
    onPortfolioDelete: (e: React.SyntheticEvent) => void;
}

const PortfolioCard = ({portfolioValue , onPortfolioDelete}: Props) => {
  return (
    <div className={styles.portfolioCard}>
      <Link 
        to={`/company/${portfolioValue}/company-profile`} 
        className={styles.portfolioTitle}
      >
        {portfolioValue}
      </Link>
      <DeletePortfolio onPortfolioDelete={onPortfolioDelete} portfolioValue={portfolioValue}/>
    </div>
  )
}

export default PortfolioCard