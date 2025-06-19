import type { ICompanyKeyMetrics } from "../../../Interfaces/APIResponses/ICompanyKeyMetrics";

export const tableConfig = [
  {
    label: "Symbol",
    render: (company: ICompanyKeyMetrics) => company.symbol,
    subTitle: "The stock ticker symbol for the company.",
  },
  {
    label: "Market Cap",
    render: (company: ICompanyKeyMetrics) => company.marketCap,
    subTitle: "The total market value of all outstanding shares of the company.",
    isCurrency: true,
  },
  {
    label: "Enterprise Value",
    render: (company: ICompanyKeyMetrics) => company.enterpriseValueTTM,
    subTitle: "The total value of the company, including debt and excluding cash, over the trailing twelve months.",
    isCurrency: true,
  },
  {
    label: "EV to Sales",
    render: (company: ICompanyKeyMetrics) => company.evToSalesTTM,
    subTitle: "Enterprise value divided by sales over the trailing twelve months.",
    isCurrency: true,
  },
  {
    label: "EV to Operating Cash Flow",
    render: (company: ICompanyKeyMetrics) => company.evToOperatingCashFlowTTM,
    subTitle: "Enterprise value divided by operating cash flow over the trailing twelve months.",
    isCurrency: true,
  },
  {
    label: "EV to Free Cash Flow",
    render: (company: ICompanyKeyMetrics) => company.evToFreeCashFlowTTM,
    subTitle: "Enterprise value divided by free cash flow over the trailing twelve months.",
    isCurrency: true,
  },
  {
    label: "EV to EBITDA",
    render: (company: ICompanyKeyMetrics) => company.evToEBITDATTM,
    subTitle: "Enterprise value divided by EBITDA over the trailing twelve months.",
  },
  {
    label: "Net Debt to EBITDA",
    render: (company: ICompanyKeyMetrics) => company.netDebtToEBITDATTM,
    subTitle: "Net debt divided by EBITDA over the trailing twelve months.",
  },
  {
    label: "Current Ratio",
    render: (company: ICompanyKeyMetrics) => company.currentRatioTTM,
    subTitle: "Current assets divided by current liabilities over the trailing twelve months.",
  },
  {
    label: "Income Quality",
    render: (company: ICompanyKeyMetrics) => company.incomeQualityTTM,
    subTitle: "The quality of earnings, measured as cash flow from operations divided by net income, over the trailing twelve months.",
  },
  {
    label: "Graham Number",
    render: (company: ICompanyKeyMetrics) => company.grahamNumberTTM,
    subTitle: "A value measure calculated using earnings per share and book value per share over the trailing twelve months.",
  },
  {
    label: "Graham Net-Net",
    render: (company: ICompanyKeyMetrics) => company.grahamNetNetTTM,
    subTitle: "A value investing metric based on current assets minus total liabilities over the trailing twelve months.",
  },
  {
    label: "Tax Burden",
    render: (company: ICompanyKeyMetrics) => company.taxBurdenTTM,
    subTitle: "The ratio of net income to pre-tax income over the trailing twelve months, showing the effect of taxes.",
  },
  {
    label: "Interest Burden",
    render: (company: ICompanyKeyMetrics) => company.interestBurdenTTM,
    subTitle: "The ratio of pre-tax income to EBIT over the trailing twelve months, showing the effect of interest expense.",
  },
  {
    label: "Working Capital",
    render: (company: ICompanyKeyMetrics) => company.workingCapitalTTM,
    subTitle: "Current assets minus current liabilities over the trailing twelve months.",
    isCurrency: true,
  },
  {
    label: "Invested Capital",
    render: (company: ICompanyKeyMetrics) => company.investedCapitalTTM,
    subTitle: "The total amount of capital invested in the company over the trailing twelve months.",
    isCurrency: true,
  },
  {
    label: "Return on Assets",
    render: (company: ICompanyKeyMetrics) => company.returnOnAssetsTTM,
    subTitle: "Net income divided by total assets over the trailing twelve months.",
  },
  {
    label: "Operating Return on Assets",
    render: (company: ICompanyKeyMetrics) => company.operatingReturnOnAssetsTTM,
    subTitle: "Operating income divided by total assets over the trailing twelve months.",
  },
  {
    label: "Return on Tangible Assets",
    render: (company: ICompanyKeyMetrics) => company.returnOnTangibleAssetsTTM,
    subTitle: "Net income divided by tangible assets over the trailing twelve months.",
  },
  {
    label: "Return on Equity",
    render: (company: ICompanyKeyMetrics) => company.returnOnEquityTTM,
    subTitle: "Net income divided by shareholder equity over the trailing twelve months.",
  },
  {
    label: "Return on Invested Capital",
    render: (company: ICompanyKeyMetrics) => company.returnOnInvestedCapitalTTM,
    subTitle: "Net operating profit after tax divided by invested capital over the trailing twelve months.",
  },
  {
    label: "Return on Capital Employed",
    render: (company: ICompanyKeyMetrics) => company.returnOnCapitalEmployedTTM,
    subTitle: "Earnings before interest and tax divided by capital employed over the trailing twelve months.",
  },
  {
    label: "Earnings Yield",
    render: (company: ICompanyKeyMetrics) => company.earningsYieldTTM,
    subTitle: "Earnings per share divided by price per share over the trailing twelve months.",
  },
  {
    label: "Free Cash Flow Yield",
    render: (company: ICompanyKeyMetrics) => company.freeCashFlowYieldTTM,
    subTitle: "Free cash flow per share divided by price per share over the trailing twelve months.",
  },
  {
    label: "CapEx to Operating Cash Flow",
    render: (company: ICompanyKeyMetrics) => company.capexToOperatingCashFlowTTM,
    subTitle: "Capital expenditures divided by operating cash flow over the trailing twelve months.",
  },
  {
    label: "CapEx to Depreciation",
    render: (company: ICompanyKeyMetrics) => company.capexToDepreciationTTM,
    subTitle: "Capital expenditures divided by depreciation over the trailing twelve months.",
  },
  {
    label: "CapEx to Revenue",
    render: (company: ICompanyKeyMetrics) => company.capexToRevenueTTM,
    subTitle: "Capital expenditures divided by revenue over the trailing twelve months.",
  },
  {
    label: "SG&A to Revenue",
    render: (company: ICompanyKeyMetrics) => company.salesGeneralAndAdministrativeToRevenueTTM,
    subTitle: "Selling, general, and administrative expenses divided by revenue over the trailing twelve months.",
  },
  {
    label: "R&D to Revenue",
    render: (company: ICompanyKeyMetrics) => company.researchAndDevelopementToRevenueTTM,
    subTitle: "Research and development expenses divided by revenue over the trailing twelve months.",
  },
  {
    label: "Stock-Based Comp to Revenue",
    render: (company: ICompanyKeyMetrics) => company.stockBasedCompensationToRevenueTTM,
    subTitle: "Stock-based compensation divided by revenue over the trailing twelve months.",
  },
  {
    label: "Intangibles to Total Assets",
    render: (company: ICompanyKeyMetrics) => company.intangiblesToTotalAssetsTTM,
    subTitle: "Intangible assets divided by total assets over the trailing twelve months.",
  },
  {
    label: "Average Receivables",
    render: (company: ICompanyKeyMetrics) => company.averageReceivablesTTM,
    subTitle: "The average value of receivables over the trailing twelve months.",
    isCurrency: true,
  },
  {
    label: "Average Payables",
    render: (company: ICompanyKeyMetrics) => company.averagePayablesTTM,
    subTitle: "The average value of payables over the trailing twelve months.",
    isCurrency: true,
  },
  {
    label: "Average Inventory",
    render: (company: ICompanyKeyMetrics) => company.averageInventoryTTM,
    subTitle: "The average value of inventory over the trailing twelve months.",
    isCurrency: true,
  },
  {
    label: "Days Sales Outstanding",
    render: (company: ICompanyKeyMetrics) => company.daysOfSalesOutstandingTTM,
    subTitle: "The average number of days it takes to collect payment after a sale over the trailing twelve months.",
  },
  {
    label: "Days Payables Outstanding",
    render: (company: ICompanyKeyMetrics) => company.daysOfPayablesOutstandingTTM,
    subTitle: "The average number of days it takes to pay suppliers over the trailing twelve months.",
  },
  {
    label: "Days Inventory Outstanding",
    render: (company: ICompanyKeyMetrics) => company.daysOfInventoryOutstandingTTM,
    subTitle: "The average number of days inventory is held before being sold over the trailing twelve months.",
  },
  {
    label: "Operating Cycle",
    render: (company: ICompanyKeyMetrics) => company.operatingCycleTTM,
    subTitle: "The average time between purchasing inventory and receiving cash from sales over the trailing twelve months.",
  },
  {
    label: "Cash Conversion Cycle",
    render: (company: ICompanyKeyMetrics) => company.cashConversionCycleTTM,
    subTitle: "The time it takes for a company to convert its investments in inventory and other resources into cash flows from sales over the trailing twelve months.",
  },
  {
    label: "Free Cash Flow to Equity",
    render: (company: ICompanyKeyMetrics) => company.freeCashFlowToEquityTTM,
    subTitle: "The amount of cash a company generates that is available to be potentially distributed to shareholders over the trailing twelve months.",
    isCurrency: true,
  },
  {
    label: "Free Cash Flow to Firm",
    render: (company: ICompanyKeyMetrics) => company.freeCashFlowToFirmTTM,
    subTitle: "The amount of cash a company generates that is available to all funding providers, including debt and equity holders, over the trailing twelve months.",
    isCurrency: true,
  },
  {
    label: "Tangible Asset Value",
    render: (company: ICompanyKeyMetrics) => company.tangibleAssetValueTTM,
    subTitle: "The value of the company's tangible assets over the trailing twelve months.",
    isCurrency: true,
  },
  {
    label: "Net Current Asset Value",
    render: (company: ICompanyKeyMetrics) => company.netCurrentAssetValueTTM,
    subTitle: "Current assets minus total liabilities over the trailing twelve months.",
    isCurrency: true,
  },
];
