import styles from '../Table.module.css';
import { formatConfigValue, type DataConfig, type RowKey } from '../types';

export function renderTableRows<T>(data: T[], config: DataConfig<T>[], rowKey: RowKey<T>) {
  return data.map((row) => (
    <tr key={rowKey(row)} className="tableRow">
      {config.map((val) => (
        <td className={styles.tableCell} key={val.label}>
          <div className={styles.animatedBorderInner}>
            {formatConfigValue(val.render(row), val.isCurrency)}
          </div>
        </td>
      ))}
    </tr>
  ));
}
