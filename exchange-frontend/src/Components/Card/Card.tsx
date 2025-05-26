import React, { type JSX, type SyntheticEvent } from 'react';
import "./Card.css"
import type { ICompanySearch } from '../../Interfaces/ICompanySearch';
import AddToPortfolio from '../Portfolio/AddToPortfolio/AddToPortfolio';

interface Props {
    id: string; 
    searchResult: ICompanySearch;
    onPortFolioCreate: (event: SyntheticEvent) => void;
};

const Card: React.FC<Props> = ({id, searchResult, onPortFolioCreate}: Props) : JSX.Element => {
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
            <AddToPortfolio onPortFolioCreate={onPortFolioCreate} symbol={searchResult.symbol}/>
        </div>
    )
}

export default Card;