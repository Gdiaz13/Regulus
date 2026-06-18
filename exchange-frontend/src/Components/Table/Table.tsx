import { renderTableRows } from './helpers/renderTableRows';
import { renderTableHeaders } from './helpers/renderTableHeaders';
import styles from './Table.module.css';
import type { DataConfig } from './types';

type Props<T> = {
  config: DataConfig<T>[];
  data: T[];
}

const Table = <T,>({ config, data }: Props<T>) => {
  return (
    <div className={styles.tableWrapper}>
      <table className={styles.table}>
        <thead >
          {renderTableHeaders(config)}
        </thead>
        <tbody>
          {renderTableRows(data, config)}
        </tbody>
      </table>
    </div>
  );
};

export default Table;
