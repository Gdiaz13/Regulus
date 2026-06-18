import { useState } from 'react';
import type { ChangeEvent, Dispatch, FormEvent, SetStateAction } from 'react';
import getCompanies from '../../../API/GET/getCompanies';
import type { LoadStatus } from '../../../API/types';
import type { ICompanySearch } from '../../../Interfaces/APIResponses/ICompanySearch';
import { usePortfolioStocks } from '../../../hooks/usePortfolioStocks';

type SearchSetters = {
  setResults: Dispatch<SetStateAction<ICompanySearch[]>>;
  setStatus: Dispatch<SetStateAction<LoadStatus>>;
  setMessage: Dispatch<SetStateAction<string | null>>;
};

export function useSearchLogic() {
  const search = useCompanySearch();
  const portfolio = usePortfolioStocks();
  return searchLogic(search, portfolio);
}

function searchLogic(search: CompanySearchState, portfolio: PortfolioState) {
  return {
    search: search.query,
    handleSearchChange: search.handleChange,
    onSearchSubmit: search.submit,
    portfolioValues: portfolio.values,
    onPortfolioAdd: (company: ICompanySearch) => portfolio.add(companyStock(company)),
    onPortfolioDelete: portfolio.remove,
    searchResult: search.results,
    searchStatus: search.status,
    searchMessage: search.message,
    portfolioError: portfolioError(portfolio),
  };
}

type CompanySearchState = ReturnType<typeof useCompanySearch>;
type PortfolioState = ReturnType<typeof usePortfolioStocks>;

function useCompanySearch() {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<ICompanySearch[]>([]);
  const [status, setStatus] = useState<LoadStatus>('idle');
  const [message, setMessage] = useState<string | null>(null);
  const setters = { setResults, setStatus, setMessage };
  const handleChange = (event: ChangeEvent<HTMLInputElement>) => setQuery(event.target.value);
  const submit = (event: FormEvent<HTMLFormElement>) => submitSearch(event, query, setters);
  return { query, results, status, message, handleChange, submit };
}

async function submitSearch(
  event: FormEvent<HTMLFormElement>,
  query: string,
  setters: SearchSetters,
) {
  event.preventDefault();
  const trimmedQuery = query.trim();
  if (!trimmedQuery) {
    setIdleSearch(setters);
    return;
  }
  startSearch(setters);
  applySearchResult(trimmedQuery, await getCompanies(trimmedQuery), setters);
}

function applySearchResult(
  query: string,
  result: Awaited<ReturnType<typeof getCompanies>>,
  setters: SearchSetters,
) {
  if (!result.ok) {
    setSearchError(result.message, setters);
    return;
  }
  if (result.data.length > 0) {
    setSearchSuccess(result.data, setters);
    return;
  }
  setSearchEmpty(query, setters);
}

function setIdleSearch({ setResults, setStatus, setMessage }: SearchSetters) {
  setResults([]);
  setStatus('idle');
  setMessage(null);
}

function startSearch({ setStatus, setMessage }: SearchSetters) {
  setStatus('loading');
  setMessage(null);
}

function setSearchSuccess(results: ICompanySearch[], setters: SearchSetters) {
  setters.setResults(results);
  setters.setStatus('success');
}

function setSearchEmpty(query: string, { setResults, setStatus, setMessage }: SearchSetters) {
  setResults([]);
  setStatus('empty');
  setMessage(`No companies found for "${query}".`);
}

function setSearchError(message: string, { setResults, setStatus, setMessage }: SearchSetters) {
  setResults([]);
  setStatus('error');
  setMessage(message);
}

function companyStock(company: ICompanySearch) {
  return { symbol: company.symbol, companyName: company.name };
}

function portfolioError(portfolio: PortfolioState) {
  return portfolio.status === 'error' ? portfolio.message : null;
}
