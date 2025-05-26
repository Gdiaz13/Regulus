import React, { type JSX, type SyntheticEvent } from 'react';
import { useCompanySearch } from './useCompanySearch';
import CardList from '../CardList/CardList';

interface SearchProps {
  onPortfolioAdd: (event: SyntheticEvent) => void;
}

const Search: React.FC<SearchProps> = ({ onPortfolioAdd }): JSX.Element => {
  const {
    search,
    searchResult,
    serverError,
    handleSearchChange,
    onSearchSubmit,
  } = useCompanySearch();

  return (
    <>
      <form onSubmit={onSearchSubmit}>
        <input value={search} onChange={handleSearchChange} />
      </form>
      <CardList searchResults={searchResult} onPortfolioAdd={onPortfolioAdd} />
      {serverError && <div style={{ color: 'red' }}>{serverError}</div>}
    </>
  );
};

export default Search;