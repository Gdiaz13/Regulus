import { useCompanySearch } from './Components/Search/useCompanySearch';
import './App.css';
import CardList  from './Components/CardList/CardList';
import Search from './Components/Search/Search';

function App() {
  const {
    search,
    searchResult,
    serverError,
    handleChange,
    onClick,
  } = useCompanySearch();

  return (
    <>
      <div className="App">
        <Search onClick={onClick} search={search} handleChange={handleChange}/>
        <CardList searchResults={searchResult} /> 
        {serverError && <div style={{color: 'red'}}>{serverError}</div>}
      </div>
    </>
  )
}

export default App
