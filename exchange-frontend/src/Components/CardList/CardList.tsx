import React, { type JSX } from 'react';
import Card  from "../Card/Card";
import type { ICompanySearch } from '../../Interfaces/ICompanySearch';

interface Props {
  companies?: ICompanySearch[];
}

const CardList: React.FC<Props> = ({ companies = [] }: Props): JSX.Element => {
    return (
    <div> 
        {companies.length === 0 ? (
          <p>No results found.</p>
        ) : (
          companies.map((company) => (
            <Card 
              key={company.symbol}
              companyName={company.name}
              ticker={company.symbol}
              price={100} // Placeholder, update if you have price info
            />
          ))
        )}
    </div>
)};

export default CardList;