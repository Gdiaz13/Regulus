import styles from './Spinner.module.css';

type Props = {
  isLoading?: boolean;
};

const Spinner = ({ isLoading = true }: Props) => {
  if (!isLoading) {
    return null;
  }
  return <SpinnerMarkup />;
};

function SpinnerMarkup() {
  return (
    <div className={styles.spinner} role="status" aria-label="Loading content">
      <span className={styles.loader} />
    </div>
  );
}

export default Spinner;
