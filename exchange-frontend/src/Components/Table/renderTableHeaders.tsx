import React from 'react';

export function renderTableHeaders(config: any[]) {
  return (
    <>
    <tr>
      {config.map((val: any) => (
        <th className="tableHeader" key={val.Label}>
          {val.Label}
        </th>
      ))}
    </tr>
    </>
  );
}
