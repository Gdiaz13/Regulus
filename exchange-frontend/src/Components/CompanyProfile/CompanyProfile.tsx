import { useOutletContext } from 'react-router-dom';
import { getKeyMetrics } from '../../API/GET/getKeyMetrics';
import ResourceStatus from '../AsyncResource/ResourceStatus';
import RatioList from '../RatioList/RatioList';
import { useTickerFirstResource } from '../../hooks/useTickerResource';
import { keyMetricsConfig } from './Config/CompanyProfileConfig';

const emptyMessage = 'No key metrics found for this ticker.';

const CompanyProfile = () => {
  const ticker = useOutletContext<string>();
  const state = useTickerFirstResource(ticker, getKeyMetrics, emptyMessage);

  if (!state.data) {
    return <ResourceStatus status={state.status} message={state.message} />;
  }

  return <RatioList data={state.data} config={keyMetricsConfig} />;
};

export default CompanyProfile;
