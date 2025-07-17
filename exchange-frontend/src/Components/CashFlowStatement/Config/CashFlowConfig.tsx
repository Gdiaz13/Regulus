import type { ICompanyCashFlow } from "../../../Interfaces/APIResponses/ICompanyCashFlow";

export const config = [
  {
    label: "Date",
    render: (company: ICompanyCashFlow) => company.date,
  },
  {
    label: "Operating Cashflow",
    render: (company: ICompanyCashFlow) => company.growthOperatingCashFlow,
    isCurrency: true,
  },
  {
    label: "Investing Cashflow",
    render: (company: ICompanyCashFlow) => company.growthOtherInvestingActivites,
    isCurrency: true,
  },
  {
    label: "Financing Cashflow",
    render: (company: ICompanyCashFlow) => company.growthNetCashUsedProvidedByFinancingActivities,
    isCurrency: true,
  },
  {
    label: "Cash At End of Period",
    render: (company: ICompanyCashFlow) => company.growthCashAtEndOfPeriod,
    isCurrency: true,
  },
  {
    label: "CapEX",
    render: (company: ICompanyCashFlow) => company.growthCapitalExpenditure,
    isCurrency: true,
  },
  {
    label: "Issuance Of Stock",
    render: (company: ICompanyCashFlow) => company.growthCommonStockIssued,
    isCurrency: true,
  },
  {
    label: "Free Cash Flow",
    render: (company: ICompanyCashFlow) => company.growthFreeCashFlow,
    isCurrency: true,
  },
];
