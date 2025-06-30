import styles from '../Table.module.css';

export function renderTableHeaders(config: any[]) {
  return (
    <>
    <tr>
      {config.map((val: any) => (
        <th className={styles.tableHeader} key={val.label}>
          <div className={styles.animatedBorderInner}>{val.label}</div>
        </th>
      ))}
    </tr>
    </>
  );
}
