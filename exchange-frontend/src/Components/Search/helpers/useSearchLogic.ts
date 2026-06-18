import { useEffect, useState } from 'react';
import type { ChangeEvent, Dispatch, FormEvent, SetStateAction } from 'react';
import getCompanies from '../../../API/GET/getCompanies';
import type { LoadStatus } from '../../../API/types';
import {
  addPortfolioStock,
  deletePortfolioStock,
  getPortfolioStocks,
} from '../../../API/portfolioClient';
import type { ICompanySearch } from '../../../Interfaces/APIResponses/ICompanySearch';
import type { IPortfolioStock } from '../../../Interfaces/APIResponses/IPortfolioStock';

type StockSetter = Dispatch<SetStateAction<IPortfolioStock[]>>;
type MessageSetter = Dispatch<SetStateAction<string | null>>;

type SearchSetters = {
  setResults: Dispatch<SetStateAction<ICompanySearch[]>>;
  setStatus: Dispatch<SetStateAction<LoadStatus>>;
  setMessage: MessageSetter;
};

export function useSearchLogic() {
  const search = useCompanySearch();
  const portfolio = usePortfolio();
  return searchLogic(search, portfolio);
}

function searchLogic(search: CompanySearchState, portfolio: PortfolioState) {
  return {
    search: search.query,
    handleSearchChange: search.handleChange,
    onSearchSubmit: search.submit,
    portfolioValues: portfolio.values,
    onPortfolioAdd: portfolio.add,
    onPortfolioDelete: portfolio.remove,
    searchResult: search.results,
    searchStatus: search.status,
    searchMessage: search.message,
    portfolioError: portfolio.error,
  };
}

type CompanySearchState = ReturnType<typeof useCompanySearch>;
type PortfolioState = ReturnType<typeof usePortfolio>;

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

function usePortfolio() {
  const [values, setValues] = useState<IPortfolioStock[]>([]);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => {
    void loadPortfolio(setValues, setError);
  }, []);
  const add = (company: ICompanySearch) => addToPortfolio(company, values, setValues, setError);
  const remove = (id: number) => removeFromPortfolio(id, setValues, setError);
  return { values, error, add, remove };
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

async function loadPortfolio(setValues: StockSetter, setError: MessageSetter) {
  const result = await getPortfolioStocks();
  if (result.ok) {
    setValues(result.data);
    setError(null);
    return;
  }
  setError(result.message);
}

async function addToPortfolio(
  company: ICompanySearch,
  values: IPortfolioStock[],
  setValues: StockSetter,
  setError: MessageSetter,
) {
  const symbol = company.symbol.toUpperCase();
  if (portfolioHasSymbol(values, symbol)) {
    setError(`${symbol} is already in your portfolio.`);
    return;
  }
  const result = await addPortfolioStock({ symbol, companyName: company.name });
  applyAddResult(result, setValues, setError);
}

async function removeFromPortfolio(
  id: number,
  setValues: StockSetter,
  setError: MessageSetter,
) {
  const result = await deletePortfolioStock(id);
  if (result.ok) {
    setValues((current) => current.filter((value) => value.id !== id));
    setError(null);
    return;
  }
  setError(result.message);
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

function applyAddResult(
  result: Awaited<ReturnType<typeof addPortfolioStock>>,
  setValues: StockSetter,
  setError: MessageSetter,
) {
  if (result.ok) {
    setValues((current) => [...current, result.data]);
    setError(null);
    return;
  }
  setError(result.message);
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

function portfolioHasSymbol(values: IPortfolioStock[], symbol: string) {
  return values.some((value) => value.symbol.toUpperCase() === symbol);
}
