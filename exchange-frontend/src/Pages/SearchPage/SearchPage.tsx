
import { useState, type ChangeEvent, type SyntheticEvent } from 'react';
import type { ICompanySearch } from '../../Interfaces/ICompanySearch';
import { searchCompanies } from '../../API/Api';
import { StarBackground } from '../../Components/Backgrounds/StarBackground';
import Hero from '../../Components/Hero/Hero';
import Search from '../../Components/Search/Search';
import PortfolioList from '../../Components/Portfolio/PortfolioList/PortfolioList';
import CardList from '../../Components/CardList/CardList';

interface Props {}

const SearchPage = (props: Props) => {
    const [search, setSearch] = useState<string>("");
    const [portfolioValues, setPortfolioValues] = useState<string[]>([]);
    const [searchResult, setSearchResult] = useState<ICompanySearch[]>([]);
    const [serverError, setServerError] = useState<string | null>(null);
  
    const handleSearchChange = (e: ChangeEvent<HTMLInputElement>) => {
      setSearch(e.target.value);
    };
  
    const onPortfolioAdd = (e: any) => {
      e.preventDefault();
  
      const exists = portfolioValues.find((value) => value === e.target[0].value);
      if (exists) return;
      const updatedPortfolio = [...portfolioValues, e.target[0].value];
      setPortfolioValues(updatedPortfolio);
    };
  
    const onPortfolioDelete = (e: any) => {
      e.preventDefault();
      const removed = portfolioValues.filter((value) => {
        return value !== e.target[0].value;
      });
      setPortfolioValues(removed);
    };
  
    const onSearchSubmit = async (e: SyntheticEvent) => {
      e.preventDefault();
      const result = await searchCompanies(search);
      //setServerError(result.data);
      if (typeof result === "string") {
        setServerError(result);
      } else if (Array.isArray(result.data)) {
        setSearchResult(result.data);
      }
    };
  return (
    <>
    <StarBackground />
    <Hero />
    <Search
      onSearchSubmit={onSearchSubmit}
      search={search}
      handleSearchChange={handleSearchChange}
    />
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
  )
}

export default SearchPage

