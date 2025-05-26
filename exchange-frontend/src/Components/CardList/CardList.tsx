import React, { type JSX } from 'react';
import Card  from "../Card/Card";
import type { ICompanySearch } from '../../Interfaces/ICompanySearch';

interface Props {
  searchResults: ICompanySearch[];
}

const CardList: React.FC<Props> = ({ searchResults = [] }: Props): JSX.Element => {
    return (
    <div> 
        {searchResults.length === 0 ? (
          <p>No results found.</p>
        ) : (
          searchResults.map((searchResult) => (
            <Card 
              id={searchResult.symbol} 
              key={crypto.randomUUID()}
              searchResult={searchResult}
            />
          ))
        )}
    </div>
)};

export default CardList;