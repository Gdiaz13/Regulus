import styles from '../Table.module.css';
import type { DataConfig } from '../types';

export function renderTableHeaders<T>(config: DataConfig<T>[]) {
  return (
    <>
    <tr>
      {config.map((val) => (
        <th className={styles.tableHeader} key={val.label}>
          <div className={styles.animatedBorderInner}>{val.label}</div>
        </th>
      ))}
    </tr>
    </>
  );
}
