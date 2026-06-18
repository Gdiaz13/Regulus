import styles from './DeletePortfolio.module.css';

interface Props {
  onPortfolioDelete: () => void;
  portfolioValue: string;
}

const DeletePortfolio = ({ onPortfolioDelete, portfolioValue }: Props) => {
  return (
    <div>
      <form
        className={styles.deletePortfolioForm}
        onSubmit={(event) => {
          event.preventDefault();
          onPortfolioDelete();
        }}
        aria-label={`Remove ${portfolioValue} from portfolio`}
      >
        <button className={styles.deleteButton} data-label="full" type="submit">
          Delete Portfolio
        </button>
        <button className={styles.deleteButton} data-label="medium" type="submit">
          Delete
        </button>
        <button className={styles.deleteButton} data-label="short" type="submit">
          X
        </button>
      </form>
    </div>
  );
};

export default DeletePortfolio;
