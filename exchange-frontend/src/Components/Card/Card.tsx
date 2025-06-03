import React, { type SyntheticEvent } from "react";
import type { JSX } from "react";
import styles from './Card.module.css';
import type { ICompanySearch } from '../../Interfaces/ICompanySearch';
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
      <Link
        to={`/company/${searchResult.symbol}`}
        className={styles.title}>
        {searchResult.name} ({searchResult.symbol})
      </Link>
      <p className={styles.currency}>{searchResult.currency}</p>
      <p className={styles.info}>
        {searchResult.exchangeFullName} - {searchResult.exchange}
      </p>
      <AddToPortfolio
        onPortfolioAdd={onPortfolioAdd}
        symbol={searchResult.symbol}
      />
    </div>
  );
};

export default Card;
