import React, { type JSX } from 'react';
import "./Card.css"
import type { ICompanySearch } from '../../Interfaces/ICompanySearch';

interface Props {
    id: string; 
    searchResult: ICompanySearch;
};

const Card: React.FC<Props> = ({id, searchResult}: Props) : JSX.Element => {
    return (
        <div className ="card">
            <img 
                alt="Company Logo"
            />
            <div className="details">
                <h2>{searchResult.name} ({searchResult.symbol})</h2>
                <p>${searchResult.currency}</p>
            </div>
            <p className = "info">
                {searchResult.exchangeFullName} - {searchResult.exchange}
            </p>
        </div>
    )
}

export default Card;