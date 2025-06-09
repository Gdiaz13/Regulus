import React from 'react'
import { CompanyDataTest } from '../../TestData/API-Response-Test/CompanyDataTest'
import styles from './RatioList.module.css'
import { renderRatioListCells } from './helpers/renderRatioListCells'

type Props = {}

const data = CompanyDataTest[0];

type Company = typeof data;

const configs = [
  {
    Label: "Company Name",
    render: (company: Company) => company.companyName,
    subTitle: "company name"
  },
  {
    Label: "Company Symbol",
    render: (company: Company) => company.symbol,
    subTitle: "company symbol"
  },
];

const RatioList = (props: Props) => {
  return (
    <div className={styles.ratioListWrapper}>
      <ul className={styles.ratioList}>
        {renderRatioListCells(configs, data)}
      </ul>
    </div>
  )
};

export default RatioList