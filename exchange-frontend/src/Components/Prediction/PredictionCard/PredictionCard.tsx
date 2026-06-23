import { formatCurrency, formatPercentage } from '../../../lib/formatters';
import type { IAiPrediction } from '../../../Interfaces/APIResponses/IPrediction';
import styles from './PredictionCard.module.css';

// One specialist prediction for a single asset.
export default function PredictionCard({ prediction }: { prediction: IAiPrediction }) {
  return (
    <article className={styles.card}>
      <PredictionHeader prediction={prediction} />
      <PriceRow prediction={prediction} />
      <ScoreGrid prediction={prediction} />
      <ReasonList title="Reasons" items={prediction.reasons} tone="reason" />
      <ReasonList title="Warnings" items={prediction.warnings} tone="warning" />
      <p className={styles.model}>{prediction.modelName} v{prediction.modelVersion}</p>
    </article>
  );
}

function PredictionHeader({ prediction }: { prediction: IAiPrediction }) {
  return (
    <header className={styles.header}>
      <h3 className={styles.title}>{prediction.assetName} <span className={styles.symbol}>{prediction.assetId}</span></h3>
      {isMock(prediction) ? <span className={styles.mock}>MOCK</span> : null}
    </header>
  );
}

function PriceRow({ prediction }: { prediction: IAiPrediction }) {
  return (
    <div className={styles.priceRow}>
      <span>{formatCurrency(prediction.currentPrice)}</span>
      <span className={styles.arrow}>→</span>
      <span className={styles.predicted}>{formatCurrency(prediction.predictedPrice)}</span>
      <span className={changeClass(prediction.predictedPercentChange)}>{formatPercentage(prediction.predictedPercentChange)}</span>
    </div>
  );
}

function ScoreGrid({ prediction }: { prediction: IAiPrediction }) {
  return (
    <dl className={styles.scores}>
      <Score label="Confidence" value={prediction.confidenceScore} />
      <Score label="Risk" value={prediction.riskScore} />
      <Score label="Bullish" value={prediction.bullishScore} />
      <Score label="Bearish" value={prediction.bearishScore} />
    </dl>
  );
}

function Score({ label, value }: { label: string; value: number }) {
  return (
    <div className={styles.score}>
      <dt>{label}</dt>
      <dd>{formatPercentage(value * 100)}</dd>
    </div>
  );
}

function ReasonList({ title, items, tone }: { title: string; items: string[]; tone: 'reason' | 'warning' }) {
  if (items.length === 0) {
    return null;
  }
  return (
    <div className={styles.reasons}>
      <p className={styles.reasonTitle}>{title}</p>
      <ul className={tone === 'warning' ? styles.warningList : styles.reasonList}>
        {items.map((item) => <li key={item}>{item}</li>)}
      </ul>
    </div>
  );
}

function changeClass(percent: number) {
  return percent >= 0 ? styles.up : styles.down;
}

function isMock(prediction: IAiPrediction) {
  return prediction.warnings.some((warning) => warning.toUpperCase().includes('MOCK'));
}
