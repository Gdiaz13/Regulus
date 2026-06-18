import { useOutletContext } from 'react-router-dom';
import { getBalanceSheet } from '../../API/GET/getBalanceSheet';
import ResourceStatus from '../AsyncResource/ResourceStatus';
import RatioList from '../RatioList/RatioList';
import { useTickerFirstResource } from '../../hooks/useTickerResource';
import { balanceSheetConfig } from './Config/balanceSheetConfig';

const emptyMessage = 'No balance sheet found for this ticker.';

const BalanceSheet = () => {
  const ticker = useOutletContext<string>();
  const state = useTickerFirstResource(ticker, getBalanceSheet, emptyMessage);

  if (!state.data) {
    return <ResourceStatus status={state.status} message={state.message} />;
  }

  return <RatioList config={balanceSheetConfig} data={state.data} />;
};

export default BalanceSheet;
