import type { ICompanyIncomeStatement } from '../../../Interfaces/APIResponses/ICompanyIncomeStatement';

export const incomeStatementConfig = [
  {
    label: 'Date',
    render: (data: ICompanyIncomeStatement) => data.date,
    subTitle: 'The reporting period for this income statement.'
  },
  {
    label: 'Symbol',
    render: (data: ICompanyIncomeStatement) => data.symbol,
    subTitle: 'The stock ticker symbol for the company.'
  },
  {
    label: 'Revenue',
    render: (data: ICompanyIncomeStatement) => data.revenue,
    subTitle: 'Total income from sales of goods and services.',
    isCurrency: true
  },
  {
    label: 'Cost of Revenue',
    render: (data: ICompanyIncomeStatement) => data.costOfRevenue,
    subTitle: 'Direct costs attributable to the production of goods sold.',
    isCurrency: true
  },
  {
    label: 'Gross Profit',
    render: (data: ICompanyIncomeStatement) => data.grossProfit,
    subTitle: 'Revenue minus cost of revenue.',
    isCurrency: true
  },
  {
    label: 'Research and Development Expenses',
    render: (data: ICompanyIncomeStatement) => data.researchAndDevelopmentExpenses,
    subTitle: 'Expenses for research and development activities.',
    isCurrency: true
  },
  {
    label: 'General and Administrative Expenses',
    render: (data: ICompanyIncomeStatement) => data.generalAndAdministrativeExpenses,
    subTitle: 'Expenses for general and administrative activities.',
    isCurrency: true
  },
  {
    label: 'Selling and Marketing Expenses',
    render: (data: ICompanyIncomeStatement) => data.sellingAndMarketingExpenses,
    subTitle: 'Expenses for selling and marketing activities.',
    isCurrency: true
  },
  {
    label: 'Other Expenses',
    render: (data: ICompanyIncomeStatement) => data.otherExpenses,
    subTitle: 'Other miscellaneous expenses.',
    isCurrency: true
  },  {
    label: 'Operating Expenses',
    render: (data: ICompanyIncomeStatement) => data.operatingExpenses,
    subTitle: 'Expenses related to normal business operations.',
    isCurrency: true
  },
  {
    label: 'Cost and Expenses',
    render: (data: ICompanyIncomeStatement) => data.costAndExpenses,
    subTitle: 'Total costs and expenses.',
    isCurrency: true
  },
  {
    label: 'Interest Income',
    render: (data: ICompanyIncomeStatement) => data.interestIncome,
    subTitle: 'Income from interest.',
    isCurrency: true
  },
  {
    label: 'Interest Expense',
    render: (data: ICompanyIncomeStatement) => data.interestExpense,
    subTitle: 'Cost incurred by an entity for borrowed funds.',
    isCurrency: true
  },
  {
    label: 'Depreciation and Amortization',
    render: (data: ICompanyIncomeStatement) => data.depreciationAndAmortization,
    subTitle: 'Non-cash expenses for depreciation and amortization.',
    isCurrency: true
  },
  {
    label: 'EBITDA',
    render: (data: ICompanyIncomeStatement) => data.ebitda,
    subTitle: 'Earnings before interest, taxes, depreciation, and amortization.',
    isCurrency: true
  },
  {
    label: 'Operating Income',
    render: (data: ICompanyIncomeStatement) => data.operatingIncome,
    subTitle: 'Profit from business operations after operating expenses.',
    isCurrency: true
  },
  {
    label: 'Total Other Income Expenses Net',
    render: (data: ICompanyIncomeStatement) => data.totalOtherIncomeExpensesNet,
    subTitle: 'Net total of other income and expenses.',
    isCurrency: true
  },
  {
    label: 'Income Before Tax',
    render: (data: ICompanyIncomeStatement) => data.incomeBeforeTax,
    subTitle: 'Income before income tax expense.',
    isCurrency: true
  },
  {
    label: 'Income Tax Expense',
    render: (data: ICompanyIncomeStatement) => data.incomeTaxExpense,
    subTitle: 'Total tax expense for the period.',
    isCurrency: true
  },
  {
    label: 'Net Income',
    render: (data: ICompanyIncomeStatement) => data.netIncome,
    subTitle: 'Total profit after all expenses, taxes, and costs.',
    isCurrency: true
  },
  {
    label: 'EPS',
    render: (data: ICompanyIncomeStatement) => data.eps,
    subTitle: 'Earnings per share.'
  },
  {
    label: 'EPS Diluted',
    render: (data: ICompanyIncomeStatement) => data.epsDiluted,
    subTitle: 'Diluted earnings per share.'
  },
  {
    label: 'Weighted Average Shares Outstanding',
    render: (data: ICompanyIncomeStatement) => data.weightedAverageShsOut,
    subTitle: 'Average number of shares outstanding during the period.'
  },
  {
    label: 'Weighted Average Shares (Diluted)',
    render: (data: ICompanyIncomeStatement) => data.weightedAverageShsOutDil,
    subTitle: 'Average number of diluted shares outstanding during the period.'
  },
];
