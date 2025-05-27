import { type SyntheticEvent } from 'react'
import styles from './AddToPortfolio.module.css';

interface Props {
    symbol: string;
    onPortfolioAdd: (event: SyntheticEvent) => void;
    
}

const AddToPortfolio = ({ symbol, onPortfolioAdd }: Props) => {
  return (
    <form className={styles.addToPortfolioForm} onSubmit={onPortfolioAdd}>
      <input
        className={styles.addToPortfolioInput}
        readOnly={true}
        hidden={true}
        value={symbol}
      />
      <button className={styles.addToPortfolioButton} type="submit">
        Add to Portfolio
      </button>
    </form>
  );
};

export default AddToPortfolio