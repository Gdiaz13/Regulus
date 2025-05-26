import React, { type SyntheticEvent } from 'react'

interface Props {
    symbol: string;
    onPortfolioAdd: (event: SyntheticEvent) => void;
    
}

const AddToPortfolio = ({ symbol, onPortfolioAdd }: Props) => {
 
  return (
    <form onSubmit={onPortfolioAdd}>
      <input readOnly={true} hidden={true} value={symbol} />
      <button type="submit">Add to Portfolio</button>
    </form>
  );
};

export default AddToPortfolio