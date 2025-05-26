import React, { type SyntheticEvent } from 'react'

interface Props {
    onPortFolioCreate: (event: SyntheticEvent) => void;
    symbol: string;
}

const AddToPortfolio = ({onPortFolioCreate, symbol}: Props) => {
  return <form onSubmit={onPortFolioCreate}>
    <input readOnly={true} hidden={true} value={symbol} />
    <button type="submit">Add to Portfolio</button>
  </form>;
}

export default AddToPortfolio