import React from 'react';
import { useSearchLogic } from './helpers/useSearchLogic';
import type { ICompanySearch } from '../../Interfaces/APIResponses/ICompanySearch';
import styles from './Search.module.css';
import PortfolioList from '../../Components/Portfolio/PortfolioList/PortfolioList';
import CardList from '../../Components/CardList/CardList';

const Search: React.FC = () => {
  const {
    search,
    handleSearchChange,
    onSearchSubmit,
    portfolioValues,
    onPortfolioAdd,
    onPortfolioDelete,
    searchResult,
    serverError,
  } = useSearchLogic();

  return (
    <>
      <section className={styles.section}>
        <div className={styles.container}>
          <form className={styles.form} onSubmit={onSearchSubmit}>
            <input
              className={styles.input}
              id="search-input"
              placeholder="Search companies"
              value={search}
              onChange={handleSearchChange}
            />
          </form>
        </div>
      </section>
      <PortfolioList
        portfolioValues={portfolioValues}
        onPortfolioDelete={onPortfolioDelete}
      />
      <CardList
        searchResults={searchResult}
        onPortfolioAdd={onPortfolioAdd}
      />
      {serverError && <div>Unable to connect to API</div>}
    </>
  );
};

export default Search;