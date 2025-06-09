import React from 'react'
import { IncomeStatementTest } from '../../TestData/API-Response-Test/IncomeStatementTest'
import { renderTableRows } from './renderTableRows';
import { renderTableHeaders } from './renderTableHeaders';
import styles from './Table.module.css';

type Props = {}

const data = IncomeStatementTest;
type Company = (typeof data)[0];

const configs = [
    {
        Label: "Year",
        render: (company: Company) => company.acceptedDate
    },
    {
        Label: "Cost of Revenue",
        render: (company: Company) => company.costOfRevenue
    }
]

const Table = (props: Props) => {
  return (
    <div className={styles.tableWrapper}> 
      <table className={styles.table}>
        <thead className={styles.tableHead}> 
          <tr>{renderTableHeaders(configs)}</tr>
        </thead>
        <tbody>
          {renderTableRows(data, configs)}
        </tbody>
      </table> 
    </div>
  )
};

export default Table