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
        <input hidden={true} value={portfolioValue} />
        <button className={styles.deleteButton} type="submit">Delete Portfolio</button>
    </form>
  </div>
    );
};

export default DeletePortfolio