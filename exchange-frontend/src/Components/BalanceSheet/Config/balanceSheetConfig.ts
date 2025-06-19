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
    isCurrency: true,
  },
  {
    label: "Short-Term Investments",
    render: (company: ICompanyBalanceSheet) => company.shortTermInvestments,
    isCurrency: true,
  },
  {
    label: "Net Receivables",
    render: (company: ICompanyBalanceSheet) => company.netReceivables,
    isCurrency: true,
  },
  {
    label: "Inventory",
    render: (company: ICompanyBalanceSheet) => company.inventory,
    isCurrency: true,
  },
  {
    label: "Total Current Assets",
    render: (company: ICompanyBalanceSheet) => company.totalCurrentAssets,
    isCurrency: true,
  },
  {
    label: "Property, Plant & Equipment (Net)",
    render: (company: ICompanyBalanceSheet) => company.propertyPlantEquipmentNet,
    isCurrency: true,
  },
  {
    label: "Goodwill",
    render: (company: ICompanyBalanceSheet) => company.goodwill,
    isCurrency: true,
  },
  {
    label: "Intangible Assets",
    render: (company: ICompanyBalanceSheet) => company.intangibleAssets,
    isCurrency: true,
  },
  {
    label: "Long-Term Investments",
    render: (company: ICompanyBalanceSheet) => company.longTermInvestments,
    isCurrency: true,
  },
  {
    label: "Total Assets",
    render: (company: ICompanyBalanceSheet) => company.totalAssets,
    isCurrency: true,
  },
  {
    label: "Total Liabilities",
    render: (company: ICompanyBalanceSheet) => company.totalLiabilities,
    isCurrency: true,
  },
  {
    label: "Common Stock",
    render: (company: ICompanyBalanceSheet) => company.commonStock,
    isCurrency: true,
  },
  {
    label: "Retained Earnings",
    render: (company: ICompanyBalanceSheet) => company.retainedEarnings,
    isCurrency: true,
  },
  {
    label: "Total Stockholders' Equity",
    render: (company: ICompanyBalanceSheet) => company.totalStockholdersEquity,
    isCurrency: true,
  },
  {
    label: "Total Equity",
    render: (company: ICompanyBalanceSheet) => company.totalEquity,
    isCurrency: true,
  },
  {
    label: "Total Liabilities & Equity",
    render: (company: ICompanyBalanceSheet) => company.totalLiabilitiesAndTotalEquity,
    isCurrency: true,
  },
];
