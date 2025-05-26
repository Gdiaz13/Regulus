import React, { type JSX, type SyntheticEvent } from 'react';
import Card  from "../Card/Card";
import type { ICompanySearch } from '../../Interfaces/ICompanySearch';

interface Props {
  searchResults: ICompanySearch[];
  onPortFolioCreate: (event: SyntheticEvent) => void;
}

const CardList: React.FC<Props> = ({ searchResults, onPortFolioCreate }: Props): JSX.Element => {
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
              onPortFolioCreate={onPortFolioCreate}
            />
          ))
        )}
    </div>
)};

export default CardList;