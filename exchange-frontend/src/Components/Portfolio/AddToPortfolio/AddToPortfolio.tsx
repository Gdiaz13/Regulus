import type { FormEvent } from 'react';
import styles from './AddToPortfolio.module.css';

const buttonLabels = [
  { label: 'Add to Portfolio', size: 'full' },
  { label: 'Add', size: 'medium' },
  { label: '+', size: 'short' },
];

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
    {buttonLabels.map(renderButton)}
  </form>
);

function submitHandler(onSubmit: () => void) {
  return (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    onSubmit();
  };
}

function renderButton(button: { label: string; size: string }) {
  return (
    <button className={styles.addToPortfolioButton} data-label={button.size} type="submit" key={button.size}>
      {button.label}
    </button>
  );
}

export default AddToPortfolio;
