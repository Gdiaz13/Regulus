import type { LoadStatus } from '../../../API/types';
import type { IPredictionHistoryItem } from '../../../Interfaces/APIResponses/IPrediction';
import { formatCurrency, formatPercentage } from '../../../lib/formatters';
import ResourceStatus from '../../AsyncResource/ResourceStatus';
import styles from './PredictionHistory.module.css';

type Props = {
  values: IPredictionHistoryItem[];
  status: LoadStatus;
  message: string | null;
};

export default function PredictionHistory({ values, status, message }: Props) {
  return (
    <section className={styles.history}>
      <HistoryHeader />
      {status === 'success' ? <HistoryList values={values} /> : <ResourceStatus status={status} message={message} />}
    </section>
  );
}

function HistoryHeader() {
  return (
    <header className={styles.header}>
      <p className={styles.eyebrow}>Saved history</p>
      <h2 className={styles.title}>Recent model calls</h2>
    </header>
  );
}

function HistoryList({ values }: { values: IPredictionHistoryItem[] }) {
  return <div className={styles.list}>{values.map((item) => <HistoryItem key={item.id} item={item} />)}</div>;
}

function HistoryItem({ item }: { item: IPredictionHistoryItem }) {
  return (
    <article className={styles.item}>
      <ItemHeader item={item} />
      <PriceLine item={item} />
      <ScoreLine item={item} />
      <ReasonLine label="Why" values={item.reasons} />
      <ReasonLine label="Risk" values={item.warnings} />
    </article>
  );
}

function ItemHeader({ item }: { item: IPredictionHistoryItem }) {
  return (
    <header className={styles.itemHeader}>
      <h3>{item.assetName} <span>{item.assetId}</span></h3>
      <p>{item.modelName} v{item.modelVersion} - {formatDate(item.createdOn)}</p>
    </header>
  );
}

function PriceLine({ item }: { item: IPredictionHistoryItem }) {
  return (
    <p className={styles.price}>
      {formatCurrency(item.currentPrice)} -&gt; {formatCurrency(item.predictedPrice)}
      <span className={changeClass(item.predictedPercentChange)}>{formatPercentage(item.predictedPercentChange)}</span>
    </p>
  );
}

function ScoreLine({ item }: { item: IPredictionHistoryItem }) {
  return (
    <p className={styles.scores}>
      Confidence {formatPercentage(item.confidenceScore * 100)} - Risk {formatPercentage(item.riskScore * 100)}
    </p>
  );
}

function ReasonLine({ label, values }: { label: string; values: string[] }) {
  if (values.length === 0) {
    return null;
  }
  return <p className={styles.reason}><strong>{label}:</strong> {values[0]}</p>;
}

function changeClass(percent: number) {
  return percent >= 0 ? styles.up : styles.down;
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString();
}
