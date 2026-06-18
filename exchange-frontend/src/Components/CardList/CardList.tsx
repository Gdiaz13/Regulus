import Card from '../Card/Card';
import type { LoadStatus } from '../../API/types';
import type { ICompanySearch } from '../../Interfaces/APIResponses/ICompanySearch';
import Spinner from '../Spinner/Spinner';
import styles from './CardList.module.css';

interface Props {
  searchResults: ICompanySearch[];
  searchStatus: LoadStatus;
  message: string | null;
  onPortfolioAdd: (company: ICompanySearch) => void;
}

const CardList = (props: Props) => {
  if (props.searchStatus === 'idle') {
    return <CardListMessage message="Search for a company to start building your watchlist." />;
  }
  if (props.searchStatus === 'loading') {
    return <CardListLoading />;
  }
  if (props.searchStatus === 'error' || props.searchStatus === 'empty') {
    return <CardListMessage message={props.message ?? 'No companies found.'} />;
  }
  return <SearchCards {...props} />;
};

function CardListLoading() {
  return (
    <div className={styles.cardListContainer}>
      <Spinner />
    </div>
  );
}

function SearchCards({ searchResults, onPortfolioAdd }: Props) {
  return (
    <div className={styles.cardListContainer}>
      {searchResults.map((result) => renderCompanyCard(result, onPortfolioAdd))}
    </div>
  );
}

function renderCompanyCard(
  result: ICompanySearch,
  onPortfolioAdd: (company: ICompanySearch) => void,
) {
  return (
    <Card
      id={result.symbol}
      key={result.symbol}
      searchResult={result}
      onPortfolioAdd={onPortfolioAdd}
    />
  );
}

function CardListMessage({ message }: { message: string }) {
  return <div className={styles.noResults}>{message}</div>;
}

export default CardList;
