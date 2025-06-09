import React from 'react';

export function renderTableRows(data: any[], configs: any[]) {
  return data.map((company) => (
    <tr key={company.cik} className="tableRow">
      {configs.map((val: any) => (
        <td className="tableCell" key={val.Label}>
          {val.render(company)}
        </td>
      ))}
    </tr>
  ));
}
