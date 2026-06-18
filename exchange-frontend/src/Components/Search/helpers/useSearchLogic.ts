import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react';
import type { ICompanySearch } from '../../../Interfaces/APIResponses/ICompanySearch';
import getCompanies from '../../../API/GET/getCompanies';
import type { IPortfolioStock } from '../../../Interfaces/APIResponses/IPortfolioStock';
import {
  addPortfolioStock,
  deletePortfolioStock,
  getPortfolioStocks,
} from '../../../API/portfolioClient';
import type { LoadStatus } from '../../../API/types';

export function useSearchLogic() {
  const [search, setSearch] = useState<string>('');
  const [portfolioValues, setPortfolioValues] = useState<IPortfolioStock[]>([]);
  const [searchResult, setSearchResult] = useState<ICompanySearch[]>([]);
  const [searchStatus, setSearchStatus] = useState<LoadStatus>('idle');
  const [searchMessage, setSearchMessage] = useState<string | null>(null);
  const [portfolioError, setPortfolioError] = useState<string | null>(null);

  useEffect(() => {
    const loadPortfolio = async () => {
      const result = await getPortfolioStocks();

      if (result.ok) {
        setPortfolioValues(result.data);
        setPortfolioError(null);
      } else {
        setPortfolioError(result.message);
      }
    };

    loadPortfolio();
  }, []);

  const handleSearchChange = (e: ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value);
  };

  const onPortfolioAdd = async (company: ICompanySearch) => {
    const normalizedSymbol = company.symbol.toUpperCase();
    const exists = portfolioValues.some(
      (value) => value.symbol.toUpperCase() === normalizedSymbol,
    );

    if (exists) {
      setPortfolioError(`${normalizedSymbol} is already in your portfolio.`);
      return;
    }

    const result = await addPortfolioStock({
      symbol: normalizedSymbol,
      companyName: company.name,
    });

    if (result.ok) {
      setPortfolioValues((current) => [...current, result.data]);
      setPortfolioError(null);
    } else {
      setPortfolioError(result.message);
    }
  };

  const onPortfolioDelete = async (id: number) => {
    const result = await deletePortfolioStock(id);

    if (result.ok) {
      setPortfolioValues((current) => current.filter((value) => value.id !== id));
      setPortfolioError(null);
    } else {
      setPortfolioError(result.message);
    }
  };

  const onSearchSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const query = search.trim();

    if (!query) {
      setSearchResult([]);
      setSearchStatus('idle');
      setSearchMessage(null);
      return;
    }

    setSearchStatus('loading');
    setSearchMessage(null);

    const result = await getCompanies(query);

    if (result.ok && result.data.length > 0) {
      setSearchResult(result.data);
      setSearchStatus('success');
    } else if (result.ok) {
      setSearchResult([]);
      setSearchStatus('empty');
      setSearchMessage(`No companies found for "${query}".`);
    } else {
      setSearchResult([]);
      setSearchStatus('error');
      setSearchMessage(result.message);
    }
  };

  return {
    search,
    handleSearchChange,
    onSearchSubmit,
    portfolioValues,
    onPortfolioAdd,
    onPortfolioDelete,
    searchResult,
    searchStatus,
    searchMessage,
    portfolioError,
  };
}
