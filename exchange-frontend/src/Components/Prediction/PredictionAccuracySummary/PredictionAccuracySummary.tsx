import type { LoadStatus } from '../../../API/types';
import type { IModelAccuracySummary } from '../../../Interfaces/APIResponses/IPrediction';
import { accuracyMetrics } from '../../../lib/accuracyPresentation';
import ResourceStatus from '../../AsyncResource/ResourceStatus';
import styles from './PredictionAccuracySummary.module.css';

type Props = {
  values: IModelAccuracySummary[];
  status: LoadStatus;
  message: string | null;
};

export default function PredictionAccuracySummary({ values, status, message }: Props) {
  return (
    <section className={styles.section}>
      <SummaryHeader />
      {status === 'success' ? <SummaryList values={values} /> : <ResourceStatus status={status} message={message} />}
    </section>
  );
}

function SummaryHeader() {
  return (
    <header className={styles.header}>
      <h2>Model accuracy</h2>
      <p>Scores compare saved predictions with stored prices after their target dates. Current models remain mock.</p>
    </header>
  );
}

function SummaryList({ values }: { values: IModelAccuracySummary[] }) {
  return <div className={styles.list}>{values.map((summary) => <SummaryCard key={summary.modelName} summary={summary} />)}</div>;
}

function SummaryCard({ summary }: { summary: IModelAccuracySummary }) {
  return (
    <article className={styles.card}>
      <CardHeader summary={summary} />
      <dl className={styles.metrics}>{accuracyMetrics(summary).map((metric) => <Metric key={metric.label} {...metric} />)}</dl>
    </article>
  );
}

function CardHeader({ summary }: { summary: IModelAccuracySummary }) {
  return (
    <header className={styles.cardHeader}>
      <h3>{summary.modelName}</h3>
      <p>{summary.scoredCount} scored {summary.scoredCount === 1 ? 'prediction' : 'predictions'}</p>
    </header>
  );
}

function Metric({ label, value }: { label: string; value: string }) {
  return <div><dt>{label}</dt><dd>{value}</dd></div>;
}
