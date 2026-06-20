import type { FormEvent } from 'react';
import styles from './DeletePortfolio.module.css';

interface Props {
  onPortfolioDelete: () => void;
  portfolioValue: string;
}

const DeletePortfolio = ({ onPortfolioDelete, portfolioValue }: Props) => (
  <form
    className={styles.deletePortfolioForm}
    onSubmit={submitHandler(onPortfolioDelete)}
    aria-label={`Remove ${portfolioValue} from portfolio`}
  >
    <button className={styles.deleteButton} type="submit" aria-label={`Remove ${portfolioValue} from portfolio`}>
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
      <span className={styles.fullLabel}>Remove from Portfolio</span>
      <span className={styles.mediumLabel}>Remove</span>
      <span className={styles.shortLabel}>X</span>
    </>
  );
}

export default DeletePortfolio;
