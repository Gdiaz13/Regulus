import { useOutletContext } from 'react-router-dom';
import { getCashFlow } from '../../API/GET/getCashFlow';
import ResourceStatus from '../AsyncResource/ResourceStatus';
import Table from '../Table/Table';
import { useTickerListResource } from '../../hooks/useTickerResource';
import { cashFlowConfig } from './Config/CashFlowConfig';
import type { ICompanyCashFlow } from '../../Interfaces/APIResponses/ICompanyCashFlow';

const emptyMessage = 'No cash flow statements found for this ticker.';

const CashFlowStatement = () => {
  const ticker = useOutletContext<string>();
  const state = useTickerListResource(ticker, getCashFlow, emptyMessage);

  if (!state.data) {
    return <ResourceStatus status={state.status} message={state.message} />;
  }

  return <Table config={cashFlowConfig} data={state.data} rowKey={cashFlowKey} />;
};

function cashFlowKey(row: ICompanyCashFlow) {
  return `${row.symbol}-${row.date}-${row.period}`;
}

export default CashFlowStatement;
