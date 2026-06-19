import { useState } from 'react';
import type { ChangeEvent, Dispatch, FormEvent, SetStateAction } from 'react';
import ResourceStatus from '../../Components/AsyncResource/ResourceStatus';
import PortfolioCard from '../../Components/Portfolio/PortfolioCard/PortfolioCard';
import StockNotes from '../../Components/Portfolio/StockNotes/StockNotes';
import type { CreatePortfolioStock, IPortfolioStock } from '../../Interfaces/APIResponses/IPortfolioStock';
import { usePortfolioStocks } from '../../hooks/usePortfolioStocks';
import styles from './PortfolioPage.module.css';

type Portfolio = ReturnType<typeof usePortfolioStocks>;
type StockFieldName = keyof StockFieldState;

const stockSymbolMaxLength = 32;

type StockFieldState = {
  symbol: string;
  companyName: string;
  purchasePrice: string;
  lastDividend: string;
  industry: string;
  marketCap: string;
};

const stockFields = [
  { name: 'symbol', label: 'Ticker', maxLength: stockSymbolMaxLength },
  { name: 'companyName', label: 'Company name' },
  { name: 'purchasePrice', label: 'Purchase price', type: 'number' },
  { name: 'lastDividend', label: 'Last dividend', type: 'number' },
  { name: 'industry', label: 'Industry' },
  { name: 'marketCap', label: 'Market cap', type: 'number' },
] satisfies StockFieldConfig[];

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
      <input id="portfolio-symbol" maxLength={stockSymbolMaxLength} value={symbol} onChange={(event) => setSymbol(event.target.value)} />
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
  const form = useStockDetailsForm(stock, onUpdate);
  return (
    <article className={styles.stockPanel}>
      <PortfolioCard portfolioValue={stock} onPortfolioDelete={onDelete} />
      <StockDetailsForm {...form} />
      <StockNotes stockId={stock.id} />
    </article>
  );
}

function useStockDetailsForm(stock: IPortfolioStock, onUpdate: UpdateStock) {
  const [fields, setFields] = useState(() => stockFieldState(stock));
  return {
    fields,
    setField: fieldSetter(setFields),
    submit: submitStockDetails(stock.id, fields, onUpdate),
  };
}

function StockDetailsForm(props: StockDetailsFormProps) {
  return (
    <form className={styles.detailsForm} onSubmit={props.submit}>
      <div className={styles.detailsGrid}>{stockFields.map(renderStockField(props))}</div>
      <button type="submit" disabled={!props.fields.symbol.trim()}>Update details</button>
    </form>
  );
}

function renderStockField(props: StockDetailsFormProps) {
  return (field: StockFieldConfig) => <StockField key={field.name} field={field} form={props} />;
}

function StockField({ field, form }: StockFieldProps) {
  return (
    <label>
      <span>{field.label}</span>
      <input {...stockInputProps(field, form)} />
    </label>
  );
}

function stockInputProps(field: StockFieldConfig, form: StockDetailsFormProps) {
  return {
    maxLength: field.maxLength,
    onChange: fieldChange(field, form),
    type: field.type ?? 'text',
    value: form.fields[field.name],
  };
}

function fieldChange(field: StockFieldConfig, form: StockDetailsFormProps) {
  return (event: ChangeEvent<HTMLInputElement>) => form.setField(field.name, event.target.value);
}

function fieldSetter(setFields: Dispatch<SetStateAction<StockFieldState>>) {
  return (name: StockFieldName, value: string) => setFields((fields) => ({ ...fields, [name]: value }));
}

function submitStockDetails(id: number, fields: StockFieldState, onUpdate: UpdateStock) {
  return (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    void onUpdate(id, stockRequest(fields));
  };
}

function stockFieldState(stock: IPortfolioStock): StockFieldState {
  return {
    symbol: stock.symbol,
    companyName: stock.companyName,
    purchasePrice: String(stock.purchasePrice),
    lastDividend: String(stock.lastDividend),
    industry: stock.industry,
    marketCap: String(stock.marketCap),
  };
}

function stockRequest(fields: StockFieldState): CreatePortfolioStock {
  return {
    symbol: fields.symbol,
    companyName: fields.companyName,
    purchasePrice: numberField(fields.purchasePrice),
    lastDividend: numberField(fields.lastDividend),
    industry: fields.industry,
    marketCap: numberField(fields.marketCap),
  };
}

function numberField(value: string) {
  return value.trim() ? Number(value) : undefined;
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

type StockFieldConfig = {
  name: StockFieldName;
  label: string;
  maxLength?: number;
  type?: 'number';
};

type StockDetailsFormProps = {
  fields: StockFieldState;
  setField: (name: StockFieldName, value: string) => void;
  submit: (event: FormEvent<HTMLFormElement>) => void;
};

type StockFieldProps = {
  field: StockFieldConfig;
  form: StockDetailsFormProps;
};

export default PortfolioPage;
