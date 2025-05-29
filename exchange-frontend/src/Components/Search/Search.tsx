import React, { type JSX, type SyntheticEvent, type ChangeEvent } from 'react';
import styles from './Search.module.css';

interface Props {
  onSearchSubmit: (e: SyntheticEvent) => void;
  search: string | undefined;
  handleSearchChange: (e: ChangeEvent<HTMLInputElement>) => void;
}

const Search: React.FC<Props> = ({
  onSearchSubmit,
  search,
  handleSearchChange,
}: Props): JSX.Element => {
  return (
    <section className={styles.section}>
      <div className={styles.container}>
        <form className={styles.form} onSubmit={onSearchSubmit}>
          <input
            className={styles.input}
            id="search-input"
            placeholder="Search companies"
            value={search}
            onChange={handleSearchChange}
          ></input>
        </form>
      </div>
    </section>
  );
};

export default Search;