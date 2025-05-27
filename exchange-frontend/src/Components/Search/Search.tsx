import React, { type JSX, type SyntheticEvent } from 'react';
import { useCompanySearch } from './useCompanySearch';
import CardList from '../CardList/CardList';
import styles from './Search.module.css';

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
    <div className={styles.searchContainer}>
      <form onSubmit={onSearchSubmit}>
        <input
          className={styles.searchInput}
          value={search}
          onChange={handleSearchChange}
        />
      </form>
      <CardList searchResults={searchResult} onPortfolioAdd={onPortfolioAdd} />
      {serverError && <div style={{ color: 'red' }}>{serverError}</div>}
    </div>
  );
};

export default Search;