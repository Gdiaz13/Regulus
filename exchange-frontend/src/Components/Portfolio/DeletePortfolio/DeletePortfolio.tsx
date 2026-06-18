import type { FormEvent } from 'react';
import styles from './DeletePortfolio.module.css';

const buttonLabels = [
  { label: 'Delete Portfolio', size: 'full' },
  { label: 'Delete', size: 'medium' },
  { label: 'X', size: 'short' },
];

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
    <button className={styles.deleteButton} data-label={button.size} type="submit" key={button.size}>
      {button.label}
    </button>
  );
}

export default DeletePortfolio;
