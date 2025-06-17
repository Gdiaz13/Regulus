import type { ICompanyBalanceSheet } from "../../../Interfaces/APIResponses/ICompanyBalanceSheet";

export const balanceSheetConfig = [
  {
    label: "Date",
    render: (company: ICompanyBalanceSheet) => company.date,
  },
  {
    label: "Symbol",
    render: (company: ICompanyBalanceSheet) => company.symbol,
  },
  {
    label: "Currency",
    render: (company: ICompanyBalanceSheet) => company.reportedCurrency,
  },
  {
    label: "Fiscal Year",
    render: (company: ICompanyBalanceSheet) => company.fiscalYear,
  },
  {
    label: "Period",
    render: (company: ICompanyBalanceSheet) => company.period,
  },
  {
    label: "Cash & Equivalents",
    render: (company: ICompanyBalanceSheet) => company.cashAndCashEquivalents,
  },
  {
    label: "Short-Term Investments",
    render: (company: ICompanyBalanceSheet) => company.shortTermInvestments,
  },
  {
    label: "Net Receivables",
    render: (company: ICompanyBalanceSheet) => company.netReceivables,
  },
  {
    label: "Inventory",
    render: (company: ICompanyBalanceSheet) => company.inventory,
  },
  {
    label: "Total Current Assets",
    render: (company: ICompanyBalanceSheet) => company.totalCurrentAssets,
  },
  {
    label: "Property, Plant & Equipment (Net)",
    render: (company: ICompanyBalanceSheet) => company.propertyPlantEquipmentNet,
  },
  {
    label: "Goodwill",
    render: (company: ICompanyBalanceSheet) => company.goodwill,
  },
  {
    label: "Intangible Assets",
    render: (company: ICompanyBalanceSheet) => company.intangibleAssets,
  },
  {
    label: "Long-Term Investments",
    render: (company: ICompanyBalanceSheet) => company.longTermInvestments,
  },
  {
    label: "Total Assets",
    render: (company: ICompanyBalanceSheet) => company.totalAssets,
  },
  {
    label: "Total Liabilities",
    render: (company: ICompanyBalanceSheet) => company.totalLiabilities,
  },
  {
    label: "Common Stock",
    render: (company: ICompanyBalanceSheet) => company.commonStock,
  },
  {
    label: "Retained Earnings",
    render: (company: ICompanyBalanceSheet) => company.retainedEarnings,
  },
  {
    label: "Total Stockholders' Equity",
    render: (company: ICompanyBalanceSheet) => company.totalStockholdersEquity,
  },
  {
    label: "Total Equity",
    render: (company: ICompanyBalanceSheet) => company.totalEquity,
  },
  {
    label: "Total Liabilities & Equity",
    render: (company: ICompanyBalanceSheet) => company.totalLiabilitiesAndTotalEquity,
  },
];
