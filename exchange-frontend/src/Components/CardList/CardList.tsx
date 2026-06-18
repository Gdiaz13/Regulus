import React from "react";
import Card from "../Card/Card";
import type { ICompanySearch } from '../../Interfaces/APIResponses/ICompanySearch';
import styles from './CardList.module.css';
import Spinner from "../Spinner/Spinner";
import type { LoadStatus } from "../../API/types";


interface Props {
  searchResults: ICompanySearch[];
  searchStatus: LoadStatus;
  message: string | null;
  onPortfolioAdd: (company: ICompanySearch) => void;
}

const CardList: React.FC<Props> = ({
  searchResults,
  searchStatus,
  message,
  onPortfolioAdd,
}: Props): React.ReactElement => {
  if (searchStatus === 'idle') {
    return (
      <div className={styles.noResults}>
        Search for a company to start building your watchlist.
      </div>
    );
  }

  if (searchStatus === 'loading') {
    return (
      <div className={styles.cardListContainer}>
        <Spinner />
      </div>
    );
  }

  if (searchStatus === 'error' || searchStatus === 'empty') {
    return <div className={styles.noResults}>{message}</div>;
  }

  return (
    <div className={styles.cardListContainer}>
      {searchResults.map((result) => {
        return (
          <Card
            id={result.symbol}
            key={result.symbol}
            searchResult={result}
            onPortfolioAdd={onPortfolioAdd}
          />
        );
      })}
    </div>
  );
};

export default CardList;
