import React from 'react';
import './App.css';
import Search from './Components/Search/Search';
import PortfolioList from './Components/Portfolio/PortfolioList/PortfolioList';

function App() {
  const [portfolioValues, setPortfolioValues] = React.useState<string[]>([]);
  
  const onPortfolioAdd = (e: any) => {
    e.preventDefault();
    const exist = portfolioValues.find((value) => value === e.target[0].value);
    if (exist) return;
    console.log('Following item wad added to portfolio:', e.target[0].value);
    const updatedPortfolio = [...portfolioValues, e.target[0].value];
    setPortfolioValues(updatedPortfolio);
  };

  return (
    <>
      <div className="App">
        <PortfolioList portfolioValues={portfolioValues}/>
        <Search onPortfolioAdd={onPortfolioAdd} />
      </div>
    </>
  );
}

export default App;
