import React, { type JSX } from 'react';
import type { ICompanySearch } from '../../Interfaces/ICompanySearch';
import AddToPortfolio from '../Portfolio/AddToPortfolio/AddToPortfolio';
import styles from './Card.module.css';

interface Props {
    id: string; 
    searchResult: ICompanySearch;
};

const Card: React.FC<Props> = ({ id, searchResult }: Props): JSX.Element => {
    return (
        <div className={styles.card}>
            <img 
                alt="Company Logo"
            /> 
            <div className={styles.details}>
                <h2>{searchResult.name} ({searchResult.symbol})</h2>
                <p>${searchResult.currency}</p>
            </div>
            <p className={styles.info}>
                {searchResult.exchangeFullName} - {searchResult.exchange}
            </p>
            <AddToPortfolio symbol={searchResult.symbol}/>
        </div>
    );
};

export default Card;