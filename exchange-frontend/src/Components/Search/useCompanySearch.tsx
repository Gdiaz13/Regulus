import { useState, type ChangeEvent, type SyntheticEvent } from 'react';
import type { ICompanySearch } from '../../Interfaces/ICompanySearch';
import { searchCompanies } from '../../API/Api';


export function useCompanySearch() {
  const [search, setSearch] = useState<string>("");
  const [searchResult, setSearchResult] = useState<ICompanySearch[]>([]);
  const [serverError, setServerError] = useState<string>("");

  const handleSearchChange = (e: ChangeEvent<HTMLInputElement>) => {
    console.log(e)
    setSearch(e.target.value);
  };

  const onSearchSubmit = async (e: SyntheticEvent) => {
    try {
    e.preventDefault();
    const result = await searchCompanies(search);
    console.log('API result:', result); // Debug log
    if (typeof result === 'string') {
      setServerError(result);
    } else if (Array.isArray(result.data)) { 
      setSearchResult(result.data);
    }
    }
    catch (error) { 
      console.error('Error fetching data:', error);
      setServerError('An error occurred while fetching data.');
    }
  };
   

  return {
    search,
    searchResult,
    serverError,
    handleSearchChange,
    onSearchSubmit,
  };
}
