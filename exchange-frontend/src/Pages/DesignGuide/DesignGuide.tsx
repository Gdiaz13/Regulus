import RatioList from '../../Components/RatioList/RatioList'
import Table from '../../Components/Table/Table'
import { IncomeStatementTest } from '../../TestData/API-Response-Test/IncomeStatementTest'

type Props = {}
const tableConfig = [
  {
  Label: "Market Cap",
  render: (company: any) => company.marketCap,
  subTitle: "market cap in USD",
  },
]


const DesignPage = (props: Props) => {
  return (
    <>
    <h1> Regulus Design page</h1>
    <p>This page is under construction. Please check back later.</p>
    <p>We are working hard to bring you a great design experience.</p>  
    <RatioList data={IncomeStatementTest} config={tableConfig}/>
    <Table data={IncomeStatementTest} config={tableConfig}/>
    </>
  )
}

export default DesignPage