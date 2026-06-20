import { Link } from 'react-router-dom';
import type { IPortfolioStock } from '../../../Interfaces/APIResponses/IPortfolioStock';
import { companyProfilePath } from '../../../Routes/companyPaths';
import DeletePortfolio from '../DeletePortfolio/DeletePortfolio';
import styles from './PortfolioCard.module.css';

interface Props {
  portfolioValue: IPortfolioStock;
  onPortfolioDelete: (id: number) => void;
}

const PortfolioCard = ({ portfolioValue, onPortfolioDelete }: Props) => {
  return (
    <div className={styles.portfolioCard}>
      <PortfolioLink symbol={portfolioValue.symbol} />
      <DeletePortfolio {...deleteProps(portfolioValue, onPortfolioDelete)} />
    </div>
  );
};

function PortfolioLink({ symbol }: { symbol: string }) {
  return (
    <Link to={companyProfilePath(symbol)} className={styles.portfolioTitle}>
      {symbol}
    </Link>
  );
}

function deleteProps(stock: IPortfolioStock, onDelete: (id: number) => void) {
  return {
    onPortfolioDelete: () => onDelete(stock.id),
    portfolioValue: stock.symbol,
  };
}

export default PortfolioCard;
