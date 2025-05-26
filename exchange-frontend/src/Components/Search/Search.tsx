import React, { type JSX } from 'react';
import { useCompanySearch } from './useCompanySearch';
import CardList from '../CardList/CardList';

const Search: React.FC = (): JSX.Element => {
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
      <CardList searchResults={searchResult} />
      {serverError && <div style={{ color: 'red' }}>{serverError}</div>}
    </>
  );
};

export default Search;