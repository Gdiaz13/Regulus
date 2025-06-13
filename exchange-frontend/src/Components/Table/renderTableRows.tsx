import React from 'react';
import styles from './Table.module.css';

export function renderTableRows(data: any, config: any) {
  return data.map((company: any) => (
    <tr key={company.cik} className="tableRow">
      {config.map((val: any) => (
        <td className={styles.tableCell} key={val.Label}>
          <div className={styles.animatedBorderInner}>{val.render(company)}</div>
        </td>
      ))}
    </tr>
  ));
}
