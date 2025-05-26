import React from 'react'
import PortfolioCard from '../PortfolioCard/PortfolioCard';
// import styles from './PortfolioList.module.css';

interface Props {
    portfolioValues: string[];
    onPortfolioDelete: (e: React.SyntheticEvent) => void;
}

const PortfolioList = ({portfolioValues, onPortfolioDelete}: Props) => {

  return (
    <>
        <h3>My Portfolio</h3>
        <ul>
            {portfolioValues.map((portfolioValue) => {
                return <PortfolioCard portfolioValue={portfolioValue} onPortfolioDelete={onPortfolioDelete}/>;
               
            })}
        </ul>
    </>
  );
};

export default PortfolioList