import { useCompanySearch } from './Components/Search/useCompanySearch';
import './App.css';
import CardList  from './Components/CardList/CardList';
import Search from './Components/Search/Search';

function App() {
  const {
    search,
    searchResult,
    serverError,
    handleSearchChange,
    onSearchSubmit,
  } = useCompanySearch();

  const onPortFolioCreate = (e: React.SyntheticEvent) => {
    e.preventDefault();
    console.log('Portfolio created for:', e);
  };

  return (
    <>
      <div className="App">
        <Search onSearchSubmit={onSearchSubmit} search={search} handleSearchChange={handleSearchChange}/>
        <CardList searchResults={searchResult} onPortFolioCreate={onPortFolioCreate}/> 
        {serverError && <div style={{color: 'red'}}>{serverError}</div>}
      </div>
    </>
  )
}

export default App
