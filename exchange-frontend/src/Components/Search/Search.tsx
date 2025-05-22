import React, {type ChangeEvent, type JSX, type SyntheticEvent } from 'react'

interface Props {
  onClick: (e: SyntheticEvent) => void;
  search : string | undefined;
  handleChange: (e: ChangeEvent<HTMLInputElement>) => void;
}

const Search: React.FC<Props> = ({onClick, search, handleChange}: Props): JSX.Element => {

  return (
    <div>
      <input
        value={search}
        onChange={handleChange}
        name="company-search"
        id="company-search"
      />
      <button onClick={onClick}>Search</button>
    </div>
  )
}

export default Search