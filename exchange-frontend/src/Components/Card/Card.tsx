import React, { type SyntheticEvent } from "react";
import type { JSX } from "react";
import styles from './Card.module.css';
import type { ICompanySearch } from '../../Interfaces/APIResponses/ICompanySearch';
import AddToPortfolio from '../Portfolio/AddToPortfolio/AddToPortfolio';
import { Link } from 'react-router-dom';

interface Props {
  id: string;
  searchResult: ICompanySearch;
  onPortfolioAdd: (e: SyntheticEvent) => void;
}

const Card: React.FC<Props> = ({
  id,
  searchResult,
  onPortfolioAdd,
}: Props): JSX.Element => {
  return (
    <div className={styles.card} key={id} id={id}>
      {/* Full: Name (Ticker) */}
      <Link
        to={`/company/${searchResult.symbol}/company-profile`}
        className={`${styles.title} ${styles.cardTitleFull}`}
        data-label="full"
      >
        {searchResult.name} ({searchResult.symbol})
      </Link>
      {/* Medium: Name only */}
      <Link
        to={`/company/${searchResult.symbol}`}
        className={`${styles.title} ${styles.cardTitleMedium}`}
        data-label="medium"
      >
        {searchResult.name}
      </Link>
      {/* Small: Ticker only */}
      <Link
        to={`/company/${searchResult.symbol}`}
        className={`${styles.title} ${styles.cardTitleShort}`}
        data-label="short"
      >
        {searchResult.symbol}
      </Link>
      {/* Only show currency and exchange info on large screens */}
      <p className={`${styles.currency} ${styles.cardInfoFull}`} data-label="full">{searchResult.currency}</p>
      <p className={`${styles.info} ${styles.cardInfoFull}`} data-label="full">{searchResult.exchangeFullName} - {searchResult.exchange}</p>
      <AddToPortfolio
        onPortfolioAdd={onPortfolioAdd}
        symbol={searchResult.symbol}
      />
    </div>
  );
};

export default Card;
