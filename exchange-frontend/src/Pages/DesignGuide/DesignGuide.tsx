import RatioList from '../../Components/RatioList/RatioList';
import Table from '../../Components/Table/Table';
import type { DataConfig } from '../../Components/Table/types';
import type { ICompanyIncomeStatement } from '../../Interfaces/APIResponses/ICompanyIncomeStatement';
import { IncomeStatementTest } from '../../TestData/API-Response-Test/IncomeStatementTest';

const tableConfig: DataConfig<ICompanyIncomeStatement>[] = [
  {
    label: 'Market Cap',
    render: (company) => company.revenue,
    subTitle: 'Sample revenue in USD',
    isCurrency: true,
  },
];

const DesignPage = () => {
  const sampleStatement = IncomeStatementTest[0];

  return (
    <>
      <h1>Regulus Design page</h1>
      <p>This page is under construction. Please check back later.</p>
      <p>We are working hard to bring you a great design experience.</p>
      <RatioList data={sampleStatement} config={tableConfig} />
      <Table data={IncomeStatementTest} config={tableConfig} />
    </>
  );
};

export default DesignPage;
