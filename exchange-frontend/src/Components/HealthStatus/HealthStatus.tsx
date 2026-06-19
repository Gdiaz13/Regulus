import { AlertCircle, CheckCircle2, LoaderCircle } from 'lucide-react';
import { useApiHealth } from '../../hooks/useApiHealth';
import type { IApiHealth } from '../../Interfaces/APIResponses/IApiHealth';
import styles from './HealthStatus.module.css';

type HealthState = ReturnType<typeof useApiHealth>;
type HealthTone = 'checking' | 'ready' | 'warning' | 'offline';

export default function HealthStatus() {
  const health = useApiHealth();
  const tone = healthTone(health);
  return (
    <div className={statusClass(tone)} title={statusTitle(health)} aria-label={statusTitle(health)}>
      {statusIcon(tone)}
      <span>{statusLabel(health)}</span>
    </div>
  );
}

function healthTone(health: HealthState): HealthTone {
  if (health.status === 'loading' || health.status === 'idle') {
    return 'checking';
  }
  if (health.status === 'error' || !health.value) {
    return 'offline';
  }
  return healthReady(health.value) ? 'ready' : 'warning';
}

function healthReady(value: IApiHealth) {
  return value.databaseAvailable && value.marketDataConfigured;
}

function statusClass(tone: HealthTone) {
  return `${styles.status} ${styles[tone]}`;
}

function statusTitle(health: HealthState) {
  if (health.status === 'error') {
    return health.message ?? 'API is offline.';
  }
  return health.value ? healthDetails(health.value) : 'Checking API readiness.';
}

function healthDetails(value: IApiHealth) {
  const database = value.databaseAvailable ? 'database ready' : 'database offline';
  const marketData = value.marketDataConfigured ? 'market data configured' : 'FMP key missing';
  return `API ${value.status}: ${database}, ${marketData}.`;
}

function statusLabel(health: HealthState) {
  if (health.status === 'loading') {
    return 'Checking';
  }
  if (health.status === 'error') {
    return 'API offline';
  }
  return health.value && healthReady(health.value) ? 'API ready' : 'Setup needed';
}

function statusIcon(tone: HealthTone) {
  if (tone === 'checking') {
    return <LoaderCircle className={styles.icon} aria-hidden="true" />;
  }
  return tone === 'ready' ? readyIcon() : warningIcon();
}

function readyIcon() {
  return <CheckCircle2 className={styles.icon} aria-hidden="true" />;
}

function warningIcon() {
  return <AlertCircle className={styles.icon} aria-hidden="true" />;
}
