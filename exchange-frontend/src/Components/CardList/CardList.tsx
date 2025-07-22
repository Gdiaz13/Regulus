import React, { type SyntheticEvent } from "react";
import Card from "../Card/Card";
import type { ICompanySearch } from '../../Interfaces/APIResponses/ICompanySearch';
import styles from './CardList.module.css';
import Spinner from "../Spinner/Spinner";


interface Props {
  searchResults: ICompanySearch[];
  onPortfolioAdd: (e: SyntheticEvent) => void;
}

const CardList: React.FC<Props> = ({
  searchResults,
  onPortfolioAdd,
}: Props): React.ReactElement => {
  return (
    <div className={styles.cardListContainer}>
    {searchResults.length > 0 ? (
      searchResults.map((result) => {
        return (
          <Card
            id={result.symbol}
            key={crypto.randomUUID()}
            searchResult={result}
            onPortfolioAdd={onPortfolioAdd}
          />
        );
      })
    ) : (
      <Spinner />
    )}
      </div>
  );
};

export default CardList;