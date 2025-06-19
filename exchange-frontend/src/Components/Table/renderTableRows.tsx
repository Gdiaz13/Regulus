import React from 'react';
import styles from './Table.module.css';

export function renderTableRows(data: any, config: any) {
  return data.map((company: any, companyIdx: number) => (
    <tr key={`${'company'}-${companyIdx}`} className="tableRow">
      {config.map((val: any) => (
        <td className={styles.tableCell} key={val.label}>
          <div className={styles.animatedBorderInner}>
            {val.isCurrency ? `$${val.render(company)?.toLocaleString() || 'N/A'}` : val.render(company)}
          </div>
        </td>
      ))}
    </tr>
  ));
}
