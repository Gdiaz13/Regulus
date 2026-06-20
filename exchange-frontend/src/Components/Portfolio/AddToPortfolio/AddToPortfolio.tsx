import type { FormEvent } from 'react';
import styles from './AddToPortfolio.module.css';

interface Props {
  symbol: string;
  onPortfolioAdd: () => void;
}

const AddToPortfolio = ({ symbol, onPortfolioAdd }: Props) => (
  <form
    className={styles.addToPortfolioForm}
    onSubmit={submitHandler(onPortfolioAdd)}
    aria-label={`Add ${symbol} to portfolio`}
  >
    <button className={styles.addToPortfolioButton} type="submit" aria-label={`Add ${symbol} to portfolio`}>
      <ButtonLabel />
    </button>
  </form>
);

function submitHandler(onSubmit: () => void) {
  return (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    onSubmit();
  };
}

function ButtonLabel() {
  return (
    <>
      <span className={styles.fullLabel}>Add to Portfolio</span>
      <span className={styles.mediumLabel}>Add</span>
      <span className={styles.shortLabel}>+</span>
    </>
  );
}

export default AddToPortfolio;
