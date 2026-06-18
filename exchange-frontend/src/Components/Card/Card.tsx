import { Link } from 'react-router-dom';
import type { ICompanySearch } from '../../Interfaces/APIResponses/ICompanySearch';
import AddToPortfolio from '../Portfolio/AddToPortfolio/AddToPortfolio';
import styles from './Card.module.css';

interface Props {
  id: string;
  searchResult: ICompanySearch;
  onPortfolioAdd: (company: ICompanySearch) => void;
}

const Card = ({ id, searchResult, onPortfolioAdd }: Props) => (
  <div className={styles.card} key={id} id={id}>
    <CardTitleLinks company={searchResult} />
    <CardInfo company={searchResult} />
    <AddToPortfolio onPortfolioAdd={() => onPortfolioAdd(searchResult)} symbol={id} />
  </div>
);

function CardTitleLinks({ company }: { company: ICompanySearch }) {
  return (
    <>
      <CompanyLink company={company} variant="full" label={fullTitle(company)} />
      <CompanyLink company={company} variant="medium" label={company.name} />
      <CompanyLink company={company} variant="short" label={company.symbol} />
    </>
  );
}

function CompanyLink(props: CompanyLinkProps) {
  return (
    <Link to={companyPath(props.company)} className={titleClass(props.variant)} data-label={props.variant}>
      {props.label}
    </Link>
  );
}

function CardInfo({ company }: { company: ICompanySearch }) {
  return (
    <>
      <p className={`${styles.currency} ${styles.cardInfoFull}`} data-label="full">{company.currency}</p>
      <p className={`${styles.info} ${styles.cardInfoFull}`} data-label="full">{exchangeLabel(company)}</p>
    </>
  );
}

function titleClass(variant: TitleVariant) {
  return `${styles.title} ${titleVariantClass(variant)}`;
}

function titleVariantClass(variant: TitleVariant) {
  return variant === 'full' ? styles.cardTitleFull : compactTitleClass(variant);
}

function compactTitleClass(variant: Exclude<TitleVariant, 'full'> | TitleVariant) {
  return variant === 'medium' ? styles.cardTitleMedium : styles.cardTitleShort;
}

function fullTitle(company: ICompanySearch) {
  return `${company.name} (${company.symbol})`;
}

function exchangeLabel(company: ICompanySearch) {
  return `${company.exchangeFullName} - ${company.exchange}`;
}

function companyPath(company: ICompanySearch) {
  return `/company/${company.symbol}/company-profile`;
}

type TitleVariant = 'full' | 'medium' | 'short';

type CompanyLinkProps = {
  company: ICompanySearch;
  variant: TitleVariant;
  label: string;
};

export default Card;
