import styles from '../RatioList.module.css';

export function renderRatioListCells(configs: any[], data: any) {
  return configs.map((row: any, idx: number) => (
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
          {row.isCurrency ? `$${row.render(data)?.toLocaleString() || 'N/A'}` : row.render(data)}
        </div>
      </div>
    </li>
  ));
}
