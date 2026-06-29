import { useState } from 'react';
import ResourceStatus from '../../Components/AsyncResource/ResourceStatus';
import PriceChart from '../../Components/PriceChart/PriceChart';
import type { IPriceHistory } from '../../Interfaces/APIResponses/IPriceHistory';
import { usePriceHistory } from '../../hooks/usePriceHistory';
import styles from './PriceHistoryPage.module.css';

const assetTypes = ['Stock', 'Etf', 'TcgCard', 'Crypto', 'Collectible'];
const historyTakeOptions = ['30', '90', '365', '1000'];

type History = ReturnType<typeof usePriceHistory>;
type Fields = { symbol: string; setSymbol: SetText; assetType: string; setAssetType: SetText; historyTake: string; setHistoryTake: SetText };
type SetText = (value: string) => void;

// Pick a symbol, then read stored end-of-day prices or capture them from the provider.
const PriceHistoryPage = () => {
  const fields = useHistoryFields();
  const history = usePriceHistory();
  const clean = fields.symbol.trim();
  const take = Number(fields.historyTake);
  return (
    <main className={styles.page}>
      <Header />
      <Controls fields={fields} onLoad={() => history.load(clean, fields.assetType, take)} onCapture={() => history.capture(clean, fields.assetType, take)} enabled={clean.length > 0} busy={history.status === 'loading'} />
      <Results history={history} />
    </main>
  );
};

function useHistoryFields(): Fields {
  const [symbol, setSymbol] = useState('');
  const [assetType, setAssetType] = useState('Stock');
  const [historyTake, setHistoryTake] = useState('365');
  return { symbol, setSymbol, assetType, setAssetType, historyTake, setHistoryTake };
}

function Header() {
  return (
    <header className={styles.header}>
      <p className={styles.eyebrow}>Price history</p>
      <h1 className={styles.title}>Stored end-of-day prices.</h1>
      <p className={styles.note}>Capture pulls EOD history from the market-data provider and stores it per asset. Load reads what is already saved.</p>
    </header>
  );
}

function Controls({ fields, onLoad, onCapture, enabled, busy }: ControlsProps) {
  return (
    <div className={styles.controls}>
      <input className={styles.symbolInput} value={fields.symbol} maxLength={32} placeholder="Symbol e.g. AMD" onChange={(event) => fields.setSymbol(event.target.value)} />
      <TypeSelect value={fields.assetType} onChange={fields.setAssetType} />
      <HistoryTakeSelect value={fields.historyTake} onChange={fields.setHistoryTake} />
      <button onClick={onLoad} disabled={!enabled || busy}>Load stored</button>
      <button onClick={onCapture} disabled={!enabled || busy}>Capture from FMP</button>
    </div>
  );
}

function TypeSelect({ value, onChange }: { value: string; onChange: SetText }) {
  return (
    <select value={value} onChange={(event) => onChange(event.target.value)}>
      {assetTypes.map((type) => <option key={type} value={type}>{type}</option>)}
    </select>
  );
}

function HistoryTakeSelect({ value, onChange }: { value: string; onChange: SetText }) {
  return (
    <select value={value} onChange={(event) => onChange(event.target.value)} aria-label="History range">
      {historyTakeOptions.map((take) => <option key={take} value={take}>Latest {take} days</option>)}
    </select>
  );
}

function Results({ history }: { history: History }) {
  if (history.status === 'success' && history.value) {
    return <Loaded history={history.value} />;
  }
  if (history.status === 'idle') {
    return <p className={styles.hint}>Enter a symbol, then load stored history or capture it from the provider.</p>;
  }
  return <ResourceStatus status={history.status} message={history.message} />;
}

function Loaded({ history }: { history: IPriceHistory }) {
  const latest = history.points[history.points.length - 1];
  return (
    <section className={styles.results}>
      <div className={styles.summary}>
        <span className={styles.symbol}>{history.symbol}</span>
        <span className={styles.meta}>{history.count} days · {history.assetType}</span>
        <span className={styles.latest}>Latest close ${latest.close.toFixed(2)}</span>
      </div>
      <PriceChart points={history.points} />
    </section>
  );
}

type ControlsProps = {
  fields: Fields;
  onLoad: () => void;
  onCapture: () => void;
  enabled: boolean;
  busy: boolean;
};

export default PriceHistoryPage;
