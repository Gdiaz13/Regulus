import styles from '../RatioList.module.css';
import { formatConfigValue, type DataConfig } from '../../Table/types';

export function renderRatioListCells<T>(configs: DataConfig<T>[], data: T) {
  return configs.map((row, idx) => renderRatioListCell(row, data, idx));
}

function renderRatioListCell<T>(row: DataConfig<T>, data: T, index: number) {
  return (
    <li className={styles.ratioListItem} key={ratioKey(row, index)}>
      <div className={styles.ratioListRow}>
        {renderRatioLabel(row)}
        {renderRatioValue(row, data)}
      </div>
    </li>
  );
}

function renderRatioLabel<T>(row: DataConfig<T>) {
  return (
    <div className={styles.ratioListLabelWrap}>
      <p className={styles.ratioListLabel}>{row.label}</p>
      <p className={styles.ratioListSubLabel}>{row.subTitle}</p>
    </div>
  );
}

function renderRatioValue<T>(row: DataConfig<T>, data: T) {
  return (
    <div className={styles.ratioListValue}>
      {formatConfigValue(row.render(data), row.isCurrency)}
    </div>
  );
}

function ratioKey<T>(row: DataConfig<T>, index: number) {
  return `${row.label || 'item'}-${index}`;
}
