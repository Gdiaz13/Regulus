import { useRef, useState } from 'react';
import type { ChangeEvent, Dispatch, FormEvent, SetStateAction } from 'react';
import getCompanies from '../../../API/GET/getCompanies';
import type { LoadStatus } from '../../../API/types';
import type { ICompanySearch } from '../../../Interfaces/APIResponses/ICompanySearch';
import { usePortfolioStocks } from '../../../hooks/usePortfolioStocks';
import { setIfActive, useActiveFlag } from '../../../hooks/useActiveFlag';
import type { ActiveFlag } from '../../../hooks/useActiveFlag';

type SearchSetters = {
  active: ActiveFlag;
  setResults: Dispatch<SetStateAction<ICompanySearch[]>>;
  setStatus: Dispatch<SetStateAction<LoadStatus>>;
  setMessage: Dispatch<SetStateAction<string | null>>;
};

type SearchRequest = {
  current: number;
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
  const active = useActiveFlag();
  const request = useRef(0);
  const setters = { active, setResults, setStatus, setMessage };
  const handleChange = (event: ChangeEvent<HTMLInputElement>) => setQuery(event.target.value);
  const submit = (event: FormEvent<HTMLFormElement>) => submitSearch(event, query, request, setters);
  return { query, results, status, message, handleChange, submit };
}

async function submitSearch(
  event: FormEvent<HTMLFormElement>,
  query: string,
  request: SearchRequest,
  setters: SearchSetters,
) {
  event.preventDefault();
  const trimmedQuery = query.trim();
  if (searchIsBlank(trimmedQuery, request, setters)) {
    return;
  }
  const requestId = startSearchRequest(request, setters);
  const result = await getCompanies(trimmedQuery);
  applyLatestSearchResult(request, requestId, trimmedQuery, result, setters);
}

function searchIsBlank(query: string, request: SearchRequest, setters: SearchSetters) {
  if (query) {
    return false;
  }
  cancelSearch(request, setters);
  return true;
}

// Search requests can finish out of order, so only the latest submit wins.
function applyLatestSearchResult(
  request: SearchRequest,
  requestId: number,
  query: string,
  result: Awaited<ReturnType<typeof getCompanies>>,
  setters: SearchSetters,
) {
  if (request.current === requestId) {
    applySearchResult(query, result, setters);
  }
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

function cancelSearch(request: SearchRequest, setters: SearchSetters) {
  nextRequestId(request);
  setIdleSearch(setters);
}

function startSearchRequest(request: SearchRequest, setters: SearchSetters) {
  const requestId = nextRequestId(request);
  startSearch(setters);
  return requestId;
}

function nextRequestId(request: SearchRequest) {
  request.current += 1;
  return request.current;
}

function setIdleSearch(setters: SearchSetters) {
  setResults(setters, []);
  setStatus(setters, 'idle');
  setMessage(setters, null);
}

function startSearch(setters: SearchSetters) {
  setStatus(setters, 'loading');
  setMessage(setters, null);
}

function setSearchSuccess(results: ICompanySearch[], setters: SearchSetters) {
  setResults(setters, results);
  setStatus(setters, 'success');
}

function setSearchEmpty(query: string, setters: SearchSetters) {
  setResults(setters, []);
  setStatus(setters, 'empty');
  setMessage(setters, `No companies found for "${query}".`);
}

function setSearchError(message: string, setters: SearchSetters) {
  setResults(setters, []);
  setStatus(setters, 'error');
  setMessage(setters, message);
}

function setResults(setters: SearchSetters, results: ICompanySearch[]) {
  setIfActive(setters.active, setters.setResults, results);
}

function setStatus(setters: SearchSetters, status: LoadStatus) {
  setIfActive(setters.active, setters.setStatus, status);
}

function setMessage(setters: SearchSetters, message: string | null) {
  setIfActive(setters.active, setters.setMessage, message);
}

function companyStock(company: ICompanySearch) {
  return { symbol: company.symbol, companyName: company.name };
}

function portfolioError(portfolio: PortfolioState) {
  return portfolio.status === 'error' ? portfolio.message : null;
}
