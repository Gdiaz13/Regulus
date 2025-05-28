import React, { type JSX } from 'react';
import Card  from "../Card/Card";
import type { ICompanySearch } from '../../Interfaces/ICompanySearch';
import styles from './CardList.module.css';

interface Props {
  searchResults: ICompanySearch[];
  onPortfolioAdd: (e: React.SyntheticEvent) => void;
}
const CardList: React.FC<Props> = ({ searchResults, onPortfolioAdd }: Props): JSX.Element => {
    return (
    <div className={styles.cardListContainer}> 
        {searchResults.length !== 0 ? (
           searchResults.map((searchResult) => (
            <Card 
              id={searchResult.symbol} 
              key={crypto.randomUUID()}
              searchResult={searchResult}
              onPortfolioAdd={onPortfolioAdd}
            />
          ))
        ) : (
          <p></p>
        )}
    </div>
)};


export default CardList;