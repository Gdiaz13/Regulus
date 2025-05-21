import React, { useState, type ChangeEvent, type JSX, type SyntheticEvent } from 'react'

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
          onChange={(e) => handleChange(e)}
          name="company-search"
          id="company-search"
        />
        <button onClick={(e) => onClick(e)} /> 
    </div>
  )
}

export default Search