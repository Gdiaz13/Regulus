import styles from './RatioList.module.css';

export function renderRatioListCells(configs: any[], data: any) {
  return configs.map((row: any) => (
    <li className={styles.ratioListItem} key={row.Label}>
      <div className={styles.ratioListRow}>
        <div className={styles.ratioListLabelWrap}>
          <p className={styles.ratioListLabel}>{row.Label}</p>
          <p className={styles.ratioListSubLabel}>{row.subTitle && row.subTitle}</p>
        </div>
        <div className={styles.ratioListValue}>{row.render(data)}</div>
      </div>
    </li>
  ));
}
