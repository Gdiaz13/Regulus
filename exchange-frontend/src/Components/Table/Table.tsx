import { renderTableRows } from './helpers/renderTableRows';
import { renderTableHeaders } from './helpers/renderTableHeaders';
import styles from './Table.module.css';
import type { DataConfig, RowKey } from './types';

type Props<T> = {
  config: DataConfig<T>[];
  data: T[];
  rowKey: RowKey<T>;
}

const Table = <T,>({ config, data, rowKey }: Props<T>) => {
  return (
    <div className={styles.tableWrapper}>
      <table className={styles.table}>
        <thead >
          {renderTableHeaders(config)}
        </thead>
        <tbody>
          {renderTableRows(data, config, rowKey)}
        </tbody>
      </table>
    </div>
  );
};

export default Table;
