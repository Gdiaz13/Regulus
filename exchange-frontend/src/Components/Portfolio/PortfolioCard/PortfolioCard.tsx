import { Link } from 'react-router-dom';
import type { IPortfolioStock } from '../../../Interfaces/APIResponses/IPortfolioStock';
import DeletePortfolio from '../DeletePortfolio/DeletePortfolio';
import styles from './PortfolioCard.module.css';

interface Props {
  portfolioValue: IPortfolioStock;
  onPortfolioDelete: (id: number) => void;
}

const PortfolioCard = ({ portfolioValue, onPortfolioDelete }: Props) => {
  return (
    <div className={styles.portfolioCard}>
      <Link
        to={`/company/${portfolioValue.symbol}/company-profile`}
        className={styles.portfolioTitle}
      >
        {portfolioValue.symbol}
      </Link>
      <DeletePortfolio
        onPortfolioDelete={() => onPortfolioDelete(portfolioValue.id)}
        portfolioValue={portfolioValue.symbol}
      />
    </div>
  );
};

export default PortfolioCard;
