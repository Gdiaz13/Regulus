import styles from './AddToPortfolio.module.css';

interface Props {
    symbol: string;
    onPortfolioAdd: () => void;
}

const AddToPortfolio = ({ symbol, onPortfolioAdd }: Props) => {
  return (
    <form
      className={styles.addToPortfolioForm}
      onSubmit={(event) => {
        event.preventDefault();
        onPortfolioAdd();
      }}
      aria-label={`Add ${symbol} to portfolio`}
    >
      {/* Responsive button labels */}
      <button className={styles.addToPortfolioButton} data-label="full" type="submit">
        Add to Portfolio
      </button>
      <button className={styles.addToPortfolioButton} data-label="medium" type="submit">
        Add
      </button>
      <button className={styles.addToPortfolioButton} data-label="short" type="submit">
        +
      </button>
    </form>
  );
};

export default AddToPortfolio
