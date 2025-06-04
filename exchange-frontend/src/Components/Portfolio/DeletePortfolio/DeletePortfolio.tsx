import React from 'react'
import styles from './DeletePortfolio.module.css';

interface Props {
    onPortfolioDelete: (e: React.SyntheticEvent) => void;
    portfolioValue: string;
}

const DeletePortfolio = ({onPortfolioDelete, portfolioValue}: Props) => {
  return (
  <div>
    <form className={styles.deletePortfolioForm} onSubmit={onPortfolioDelete}>
        <input hidden={true} value={portfolioValue} readOnly/>
        <button className={styles.deleteButton} data-label="full" type="submit">Delete Portfolio</button>
        <button className={styles.deleteButton} data-label="medium" type="submit">Delete</button>
        <button className={styles.deleteButton} data-label="short" type="submit">×</button>
    </form>
  </div>
    );
};

export default DeletePortfolio