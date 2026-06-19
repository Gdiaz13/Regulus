import { useState } from 'react';
import type { FormEvent } from 'react';
import ResourceStatus from '../../Components/AsyncResource/ResourceStatus';
import PortfolioCard from '../../Components/Portfolio/PortfolioCard/PortfolioCard';
import { portfolioSymbolMaxLength } from '../../Components/Portfolio/portfolioRules';
import StockDetails from '../../Components/Portfolio/StockDetails/StockDetails';
import StockNotes from '../../Components/Portfolio/StockNotes/StockNotes';
import type { CreatePortfolioStock, IPortfolioStock } from '../../Interfaces/APIResponses/IPortfolioStock';
import { usePortfolioStocks } from '../../hooks/usePortfolioStocks';
import styles from './PortfolioPage.module.css';

type Portfolio = ReturnType<typeof usePortfolioStocks>;

const PortfolioPage = () => {
  const portfolio = usePortfolioStocks();
  const form = usePortfolioForm(portfolio.add);
  return (
    <main className={styles.page}>
      <PortfolioHeader />
      <PortfolioForm {...form} />
      <PortfolioMessage portfolio={portfolio} />
      <PortfolioContent portfolio={portfolio} />
    </main>
  );
};

function usePortfolioForm(add: Portfolio['add']) {
  const [symbol, setSymbol] = useState('');
  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (symbol.trim() && await add(portfolioStock(symbol))) {
      setSymbol('');
    }
  };
  return { symbol, setSymbol, submit };
}

function portfolioStock(symbol: string) {
  return { symbol, companyName: symbol.trim().toUpperCase() };
}

function PortfolioHeader() {
  return (
    <header className={styles.header}>
      <p className={styles.eyebrow}>Portfolio</p>
      <h1 className={styles.title}>Track the companies you care about.</h1>
    </header>
  );
}

function PortfolioForm({ symbol, setSymbol, submit }: FormProps) {
  return (
    <form className={styles.form} onSubmit={submit}>
      <label htmlFor="portfolio-symbol">Ticker symbol</label>
      <input id="portfolio-symbol" maxLength={portfolioSymbolMaxLength} value={symbol} onChange={(event) => setSymbol(event.target.value)} />
      <button type="submit" disabled={!symbol.trim()}>Add</button>
    </form>
  );
}

function PortfolioMessage({ portfolio }: { portfolio: Portfolio }) {
  if (portfolio.status !== 'error') {
    return null;
  }
  return <p className={styles.message}>{portfolio.message}</p>;
}

function PortfolioContent({ portfolio }: { portfolio: Portfolio }) {
  if (portfolio.status === 'loading') {
    return <ResourceStatus status={portfolio.status} message={portfolio.message} />;
  }
  if (portfolio.values.length === 0) {
    return <PortfolioEmpty />;
  }
  return <PortfolioGrid stocks={portfolio.values} onDelete={portfolio.remove} onUpdate={portfolio.update} />;
}

function PortfolioEmpty() {
  return <p className={styles.empty}>Add a ticker to start a persisted portfolio.</p>;
}

function PortfolioGrid({ stocks, onDelete, onUpdate }: GridProps) {
  return (
    <section className={styles.grid}>
      {stocks.map((stock) => renderStock(stock, onDelete, onUpdate))}
    </section>
  );
}

function renderStock(stock: IPortfolioStock, onDelete: DeleteStock, onUpdate: UpdateStock) {
  return <PortfolioStockPanel key={stock.id} stock={stock} onDelete={onDelete} onUpdate={onUpdate} />;
}

function PortfolioStockPanel({ stock, onDelete, onUpdate }: StockPanelProps) {
  return (
    <article className={styles.stockPanel}>
      <PortfolioCard portfolioValue={stock} onPortfolioDelete={onDelete} />
      <StockDetails stock={stock} onUpdate={onUpdate} />
      <StockNotes stockId={stock.id} />
    </article>
  );
}

type DeleteStock = (id: number) => void;
type UpdateStock = (id: number, stock: CreatePortfolioStock) => Promise<boolean>;

type FormProps = {
  symbol: string;
  setSymbol: (value: string) => void;
  submit: (event: FormEvent<HTMLFormElement>) => void;
};

type GridProps = {
  stocks: IPortfolioStock[];
  onDelete: DeleteStock;
  onUpdate: UpdateStock;
};

type StockPanelProps = {
  stock: IPortfolioStock;
  onDelete: DeleteStock;
  onUpdate: UpdateStock;
};

export default PortfolioPage;
