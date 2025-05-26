import React, { type SyntheticEvent } from 'react'

interface Props {
    symbol: string;
}

const AddToPortfolio = ({ symbol }: Props) => {
  const onPortfolioCreate = (e: SyntheticEvent) => {
    e.preventDefault();
    console.log('Following item wad added to portfolio:', symbol);
  };

  return (
    <form onSubmit={onPortfolioCreate}>
      <input readOnly={true} hidden={true} value={symbol} />
      <button type="submit">Add to Portfolio</button>
    </form>
  );
};

export default AddToPortfolio