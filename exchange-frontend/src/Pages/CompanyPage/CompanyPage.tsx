import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom';
import type { ICompanyProfile } from '../../Interfaces/ICompanyProfile';
import { getCompanyProfile } from '../../API/GET/getCompanyProfile';
import styles from './CompanyPage.module.css';

const CompanyPage = () => {
  let { ticker } = useParams();
  const [company, setCompany] = useState<ICompanyProfile>();

  useEffect(() => {
    const getProfileInfo = async () => {
      try {
        const result = await getCompanyProfile(ticker!);
        console.log('Company profile API result:', result);
        setCompany(result?.data[0]);
      } catch (error) {
        console.log('Error fetching company profile:', error);
      }
    };
    getProfileInfo();
    }, []);

  return (
    <div className={styles.companyContainer}>
      {company ? (
        <div>
          <img className={styles.companyLogo} src={company.image} alt={`${company.companyName} logo`} />
          <h1 className={styles.companyHeader}>{company.companyName} ({company.symbol})</h1>
          <p className={styles.companyPrice}>Price: ${company.price}</p>
          <p className={styles.companyInfo}>Exchange: {company.exchange}</p>
          <p className={styles.companyInfo}>Industry: {company.industry}</p>
          <p className={styles.companyDesc}>Description: {company.description}</p>
        </div>
      ) : (
        <p>Loading company information...</p>
      )}
    </div>
  )
}

export default CompanyPage;