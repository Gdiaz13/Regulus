import type { ICompanyIncomeStatement } from '../../../Interfaces/APIResponses/ICompanyIncomeStatement';

export const incomeStatementConfig = [
  {
    Label: 'Date',
    render: (data: ICompanyIncomeStatement) => data.date,
    subTitle: 'The reporting period for this income statement.'
  },
  {
    Label: 'Symbol',
    render: (data: ICompanyIncomeStatement) => data.symbol,
    subTitle: 'The stock ticker symbol for the company.'
  },
  {
    Label: 'Revenue',
    render: (data: ICompanyIncomeStatement) => data.revenue,
    subTitle: 'Total income from sales of goods and services.'
  },
  {
    Label: 'Cost of Revenue',
    render: (data: ICompanyIncomeStatement) => data.costOfRevenue,
    subTitle: 'Direct costs attributable to the production of goods sold.'
  },
  {
    Label: 'Gross Profit',
    render: (data: ICompanyIncomeStatement) => data.grossProfit,
    subTitle: 'Revenue minus cost of revenue.'
  },
  {
    Label: 'Research and Development Expenses',
    render: (data: ICompanyIncomeStatement) => data.researchAndDevelopmentExpenses,
    subTitle: 'Expenses for research and development activities.'
  },
  {
    Label: 'General and Administrative Expenses',
    render: (data: ICompanyIncomeStatement) => data.generalAndAdministrativeExpenses,
    subTitle: 'Expenses for general and administrative activities.'
  },
  {
    Label: 'Selling and Marketing Expenses',
    render: (data: ICompanyIncomeStatement) => data.sellingAndMarketingExpenses,
    subTitle: 'Expenses for selling and marketing activities.'
  },
  {
    Label: 'Other Expenses',
    render: (data: ICompanyIncomeStatement) => data.otherExpenses,
    subTitle: 'Other miscellaneous expenses.'
  },
  {
    Label: 'Operating Expenses',
    render: (data: ICompanyIncomeStatement) => data.operatingExpenses,
    subTitle: 'Expenses related to normal business operations.'
  },
  {
    Label: 'Cost and Expenses',
    render: (data: ICompanyIncomeStatement) => data.costAndExpenses,
    subTitle: 'Total costs and expenses.'
  },
  {
    Label: 'Interest Income',
    render: (data: ICompanyIncomeStatement) => data.interestIncome,
    subTitle: 'Income from interest.'
  },
  {
    Label: 'Interest Expense',
    render: (data: ICompanyIncomeStatement) => data.interestExpense,
    subTitle: 'Cost incurred by an entity for borrowed funds.'
  },
  {
    Label: 'Depreciation and Amortization',
    render: (data: ICompanyIncomeStatement) => data.depreciationAndAmortization,
    subTitle: 'Non-cash expenses for depreciation and amortization.'
  },
  {
    Label: 'EBITDA',
    render: (data: ICompanyIncomeStatement) => data.ebitda,
    subTitle: 'Earnings before interest, taxes, depreciation, and amortization.'
  },
  {
    Label: 'Operating Income',
    render: (data: ICompanyIncomeStatement) => data.operatingIncome,
    subTitle: 'Profit from business operations after operating expenses.'
  },
  {
    Label: 'Total Other Income Expenses Net',
    render: (data: ICompanyIncomeStatement) => data.totalOtherIncomeExpensesNet,
    subTitle: 'Net total of other income and expenses.'
  },
  {
    Label: 'Income Before Tax',
    render: (data: ICompanyIncomeStatement) => data.incomeBeforeTax,
    subTitle: 'Income before income tax expense.'
  },
  {
    Label: 'Income Tax Expense',
    render: (data: ICompanyIncomeStatement) => data.incomeTaxExpense,
    subTitle: 'Total tax expense for the period.'
  },
  {
    Label: 'Net Income',
    render: (data: ICompanyIncomeStatement) => data.netIncome,
    subTitle: 'Total profit after all expenses, taxes, and costs.'
  },
  {
    Label: 'EPS',
    render: (data: ICompanyIncomeStatement) => data.eps,
    subTitle: 'Earnings per share.'
  },
  {
    Label: 'EPS Diluted',
    render: (data: ICompanyIncomeStatement) => data.epsDiluted,
    subTitle: 'Diluted earnings per share.'
  },
  {
    Label: 'Weighted Average Shares Outstanding',
    render: (data: ICompanyIncomeStatement) => data.weightedAverageShsOut,
    subTitle: 'Average number of shares outstanding during the period.'
  },
  {
    Label: 'Weighted Average Shares (Diluted)',
    render: (data: ICompanyIncomeStatement) => data.weightedAverageShsOutDil,
    subTitle: 'Average number of diluted shares outstanding during the period.'
  },
];
