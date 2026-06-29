import { useState } from 'react';
import type { FormEvent } from 'react';
import ResourceStatus from '../../Components/AsyncResource/ResourceStatus';
import type { IStockTradingAgentsRequest, IStockTradingAgentsResponse } from '../../Interfaces/APIResponses/ITradingAgents';
import { useStockTradingAgents } from '../../hooks/useStockTradingAgents';
import { useTradingAgentsStatus } from '../../hooks/useTradingAgentsStatus';
import { formatCurrency, formatPercentage } from '../../lib/formatters';
import styles from './TradingAgentsPage.module.css';

type FormState = IStockTradingAgentsRequest;

export default function TradingAgentsPage() {
  const [form, setForm] = useState<FormState>(initialForm);
  const analysis = useStockTradingAgents();
  const service = useTradingAgentsStatus();
  return (
    <main className={styles.page}>
      <Header />
      <ServiceStatus service={service} />
      <AnalysisForm form={form} setForm={setForm} onAnalyze={() => analysis.analyze(form)} running={analysis.status === 'loading'} />
      <AnalysisResult value={analysis.value} status={analysis.status} message={analysis.message} />
    </main>
  );
}

function Header() {
  return (
    <header className={styles.header}>
      <p className={styles.eyebrow}>TradingAgents</p>
      <h1 className={styles.title}>Stock research branch</h1>
    </header>
  );
}

function ServiceStatus({ service }: { service: ReturnType<typeof useTradingAgentsStatus> }) {
  if (service.status === 'loading') {
    return <p className={styles.status}>Checking StockTradingAgentsAI...</p>;
  }
  if (service.status === 'error') {
    return <p className={styles.status}>{service.message}</p>;
  }
  return <ModelStatus service={service} />;
}

function ModelStatus({ service }: { service: ReturnType<typeof useTradingAgentsStatus> }) {
  return (
    <p className={styles.status}>
      {service.health?.aiAvailable ? 'Online' : 'Offline'} - {service.model?.modelName} v{service.model?.modelVersion}
      {service.model?.isMock ? ' - MOCK' : ''}
    </p>
  );
}

function AnalysisForm({ form, setForm, onAnalyze, running }: FormProps) {
  return (
    <form className={styles.form} onSubmit={(event) => submit(event, onAnalyze)}>
      <Input label="Symbol" value={form.symbol} onChange={(symbol) => setForm({ ...form, symbol })} required />
      <Input label="Company" value={form.companyName ?? ''} onChange={(companyName) => setForm({ ...form, companyName })} />
      <Input label="Price" value={String(form.currentPrice)} onChange={(value) => setForm({ ...form, currentPrice: toNumber(value) })} type="number" required />
      <Input label="Date" value={form.analysisDate ?? ''} onChange={(analysisDate) => setForm({ ...form, analysisDate })} type="date" />
      <button type="submit" disabled={running || !form.symbol.trim()}>{running ? 'Analyzing...' : 'Analyze'}</button>
    </form>
  );
}

function Input({ label, value, onChange, type = 'text', required = false }: InputProps) {
  return (
    <label className={styles.field}>
      <span>{label}</span>
      <input type={type} value={value} required={required} onChange={(event) => onChange(event.currentTarget.value)} />
    </label>
  );
}

function AnalysisResult({ value, status, message }: ResultProps) {
  if (status === 'success' && value) {
    return <ResultCard value={value} />;
  }
  if (status === 'idle') {
    return <p className={styles.hint}>Run a stock analysis to see the research branch output.</p>;
  }
  return <ResourceStatus status={status} message={message} />;
}

function ResultCard({ value }: { value: IStockTradingAgentsResponse }) {
  return (
    <section className={styles.result}>
      <ResultHeader value={value} />
      <ScoreRow value={value} />
      <ArgumentList title="Bullish" items={value.bullishArguments} />
      <ArgumentList title="Bearish" items={value.bearishArguments} />
      <ArgumentList title="Warnings" items={value.warnings} />
      {value.rawDecision ? <RawDecision value={value.rawDecision} /> : null}
    </section>
  );
}

function ResultHeader({ value }: { value: IStockTradingAgentsResponse }) {
  return (
    <header className={styles.resultHeader}>
      <p>{value.modelName} v{value.modelVersion}</p>
      <h2>{value.symbol} - {value.recommendation}</h2>
      <p>{value.summary}</p>
    </header>
  );
}

function ScoreRow({ value }: { value: IStockTradingAgentsResponse }) {
  return (
    <dl className={styles.scores}>
      <Score label="Price" value={formatCurrency(value.currentPrice)} />
      <Score label="Confidence" value={formatPercentage(value.confidenceScore * 100)} />
      <Score label="Risk" value={formatPercentage(value.riskScore * 100)} />
    </dl>
  );
}

function Score({ label, value }: { label: string; value: string }) {
  return <div><dt>{label}</dt><dd>{value}</dd></div>;
}

function ArgumentList({ title, items }: { title: string; items: string[] }) {
  return <div className={styles.arguments}><h3>{title}</h3><ul>{items.map((item) => <li key={item}>{item}</li>)}</ul></div>;
}

function RawDecision({ value }: { value: unknown }) {
  return <pre className={styles.raw}>{JSON.stringify(value, null, 2)}</pre>;
}

function submit(event: FormEvent<HTMLFormElement>, onAnalyze: () => void) {
  event.preventDefault();
  onAnalyze();
}

function toNumber(value: string) {
  return Number(value) || 0;
}

type FormProps = {
  form: FormState;
  setForm: (form: FormState) => void;
  onAnalyze: () => void;
  running: boolean;
};

type InputProps = {
  label: string;
  value: string;
  onChange: (value: string) => void;
  type?: string;
  required?: boolean;
};

type ResultProps = {
  value: IStockTradingAgentsResponse | null;
  status: ReturnType<typeof useStockTradingAgents>['status'];
  message: string | null;
};

const initialForm = {
  symbol: 'AMD',
  companyName: 'Advanced Micro Devices',
  currentPrice: 100,
} satisfies FormState;
