import styles from '../Table.module.css';
import { formatConfigValue, type DataConfig } from '../types';

export function renderTableRows<T>(data: T[], config: DataConfig<T>[]) {
  return data.map((row, rowIndex) => (
    <tr key={`row-${rowIndex}`} className="tableRow">
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
