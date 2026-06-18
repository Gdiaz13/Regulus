import { useOutletContext } from 'react-router-dom';
import { getIncomeStatement } from '../../API/GET/getIncomeStatement';
import ResourceStatus from '../AsyncResource/ResourceStatus';
import Table from '../Table/Table';
import { useTickerListResource } from '../../hooks/useTickerResource';
import { incomeStatementConfig } from './Config/IncomeStatementConfig';

const emptyMessage = 'No income statements found for this ticker.';

const IncomeStatement = () => {
  const ticker = useOutletContext<string>();
  const state = useTickerListResource(ticker, getIncomeStatement, emptyMessage);

  if (!state.data) {
    return <ResourceStatus status={state.status} message={state.message} />;
  }

  return <Table config={incomeStatementConfig} data={state.data} />;
};

export default IncomeStatement;
