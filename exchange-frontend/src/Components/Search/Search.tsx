import type { ChangeEventHandler, FormEventHandler } from 'react';
import CardList from '../../Components/CardList/CardList';
import PortfolioList from '../../Components/Portfolio/PortfolioList/PortfolioList';
import { useSearchLogic } from './helpers/useSearchLogic';
import styles from './Search.module.css';

type SearchLogic = ReturnType<typeof useSearchLogic>;

const Search = () => {
  const logic = useSearchLogic();
  return (
    <>
      <SearchForm {...searchFormProps(logic)} />
      <SearchPortfolio {...portfolioProps(logic)} />
      <SearchResults {...resultProps(logic)} />
    </>
  );
};

function SearchForm(props: SearchFormProps) {
  return (
    <section className={styles.section}>
      <div className={styles.container}>
        <form className={styles.form} onSubmit={props.onSubmit} aria-label="Search companies">
          <input className={styles.input} {...searchInputProps(props)} />
        </form>
      </div>
    </section>
  );
}

function SearchPortfolio({ portfolioError, ...listProps }: PortfolioProps) {
  return (
    <>
      <PortfolioList {...listProps} />
      {portfolioError && <div className={styles.message}>{portfolioError}</div>}
    </>
  );
}

function SearchResults(props: ResultProps) {
  return <CardList {...props} />;
}

function searchFormProps(logic: SearchLogic): SearchFormProps {
  return {
    search: logic.search,
    onChange: logic.handleSearchChange,
    onSubmit: logic.onSearchSubmit,
  };
}

function portfolioProps(logic: SearchLogic): PortfolioProps {
  return {
    portfolioValues: logic.portfolioValues,
    onPortfolioDelete: logic.onPortfolioDelete,
    portfolioError: logic.portfolioError,
  };
}

function resultProps(logic: SearchLogic): ResultProps {
  return {
    searchResults: logic.searchResult,
    searchStatus: logic.searchStatus,
    message: logic.searchMessage,
    onPortfolioAdd: logic.onPortfolioAdd,
  };
}

function searchInputProps(props: SearchFormProps) {
  return {
    id: 'search-input',
    'aria-label': 'Company search',
    placeholder: 'Search companies',
    value: props.search,
    onChange: props.onChange,
  };
}

type SearchFormProps = {
  search: string;
  onChange: ChangeEventHandler<HTMLInputElement>;
  onSubmit: FormEventHandler<HTMLFormElement>;
};

type PortfolioProps = Pick<
  SearchLogic,
  'portfolioValues' | 'onPortfolioDelete' | 'portfolioError'
>;

type ResultProps = {
  searchResults: SearchLogic['searchResult'];
  searchStatus: SearchLogic['searchStatus'];
  message: SearchLogic['searchMessage'];
  onPortfolioAdd: SearchLogic['onPortfolioAdd'];
};

export default Search;
