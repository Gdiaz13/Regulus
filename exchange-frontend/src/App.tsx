import { useState, type ChangeEvent, type SyntheticEvent } from 'react';
import './App.css';
// import { Card } from './Components/Card/Card'
import CardList  from './Components/CardList/CardList';
import Search from './Components/Search/Search';

function App() {
  const [search, setSearch] = useState<string>("");

  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value);
    console.log(e)
  }
  const onClick = (e: SyntheticEvent) => {
    // e.preventDefault();
    console.log(search);
  };

  return (
    <>
      <div>
        <Search onClick={onClick} search={search} handleChange={handleChange}/>
        <CardList />
      </div>
    </>
  )
}

export default App
