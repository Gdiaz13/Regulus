import { renderTableRows } from './helpers/renderTableRows';
import { renderTableHeaders } from './helpers/renderTableHeaders';
import styles from './Table.module.css';

type Props = {
  config: any;
  data: any;
}

const Table = ({ config, data }: Props) => {
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