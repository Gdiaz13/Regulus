import styles from '../RatioList.module.css';
import { formatConfigValue, type DataConfig } from '../../Table/types';

export function renderRatioListCells<T>(configs: DataConfig<T>[], data: T) {
  return configs.map((row, idx) => (
    <li
      className={styles.ratioListItem}
      key={`${row.label || 'item'}-${idx}`}
    >
      <div className={styles.ratioListRow}>
        <div className={styles.ratioListLabelWrap}>
          <p className={styles.ratioListLabel}>{row.label}</p>
          <p className={styles.ratioListSubLabel}>{row.subTitle}</p>
        </div>
        <div className={styles.ratioListValue}>
          {formatConfigValue(row.render(data), row.isCurrency)}
        </div>
      </div>
    </li>
  ));
}
