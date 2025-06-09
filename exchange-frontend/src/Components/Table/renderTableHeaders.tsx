import React from 'react';

export function renderTableHeaders(configs: any[]) {
  return configs.map((config: any) => (
    <th className="tableHeader" key={config.Label}>
      {config.Label}
    </th>
  ));
}
