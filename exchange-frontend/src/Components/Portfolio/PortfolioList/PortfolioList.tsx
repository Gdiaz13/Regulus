import React from 'react'
import PortfolioCard from '../PortfolioCard/PortfolioCard';
// import styles from './PortfolioList.module.css';

interface Props {
    portfolioValues: string[];
}

const PortfolioList = ({portfolioValues}: Props) => {

  return (
    <>
        <h3>My Portfolio</h3>
        <ul>
            {portfolioValues.map((portfolioValue) => {
                return <PortfolioCard portfolioValue={portfolioValue}/>;
               
            })}
        </ul>
    </>
  );
};

export default PortfolioList