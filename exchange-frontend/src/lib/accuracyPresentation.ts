export type AccuracyMetricSource = {
  winRate: number;
  averageAbsolutePercentError: number;
  confidenceCalibrationError: number;
  averageTimeHorizonDays: number;
};

export type AccuracyMetric = {
  label: string;
  value: string;
};

export function accuracyMetrics(summary: AccuracyMetricSource): AccuracyMetric[] {
  return [
    metric('Direction wins', percentage(summary.winRate)),
    metric('Average error', percentage(summary.averageAbsolutePercentError)),
    metric('Confidence gap', percentage(summary.confidenceCalibrationError)),
    metric('Average horizon', horizon(summary.averageTimeHorizonDays)),
  ];
}

function metric(label: string, value: string): AccuracyMetric {
  return { label, value };
}

function percentage(value: number) {
  return `${value.toFixed(2)}%`;
}

function horizon(value: number) {
  const days = Math.round(value);
  return `${days} ${days === 1 ? 'day' : 'days'}`;
}
