import { useEffect, useState } from 'react'
import type { ICompanyKeyMetrics } from '../../Interfaces/APIResponses/ICompanyKeyMetrics'
import { tableConfig } from './Config/CompanyProfileConfig'
import { useOutletContext } from 'react-router-dom';
import { getKeyMetrics } from '../../API/GET/getKeyMetrics';
import RatioList from '../RatioList/RatioList';

interface Props {}


const CompanyProfile = (props: Props) => {
  const ticker = useOutletContext<string>();
  const [companyData, setCompanyData] = useState<ICompanyKeyMetrics>();

  useEffect(()=> {
    const getCompanyKeyMetrics = async () => {
      const value = await getKeyMetrics(ticker);
      setCompanyData(value?.data[0]);
    };
    getCompanyKeyMetrics();
  }, []); // the [] dependency array ensures this effect runs only once when the component mounts
  
  return (
    <>
    { companyData ? (
      <>
      <RatioList data={companyData} config={tableConfig} />
      </>
    ) : (
      <div>Loading...</div> 
    )}
    </>
  )
  
}

export default CompanyProfile