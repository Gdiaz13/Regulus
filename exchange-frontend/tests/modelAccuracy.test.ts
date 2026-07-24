import { deepStrictEqual } from 'node:assert';
import test from 'node:test';
import { accuracyMetrics } from '../src/lib/accuracyPresentation.ts';

test('accuracyMetrics formats the core model accuracy signals', () => {
  const metrics = accuracyMetrics({
    winRate: 62.5,
    averageAbsolutePercentError: 7.25,
    confidenceCalibrationError: 14.5,
    averageTimeHorizonDays: 45.4,
  });

  deepStrictEqual(metrics, [
    { label: 'Direction wins', value: '62.50%' },
    { label: 'Average error', value: '7.25%' },
    { label: 'Confidence gap', value: '14.50%' },
    { label: 'Average horizon', value: '45 days' },
  ]);
});

test('accuracyMetrics uses singular day when the rounded horizon is one', () => {
  const metrics = accuracyMetrics({
    winRate: 50,
    averageAbsolutePercentError: 10,
    confidenceCalibrationError: 5,
    averageTimeHorizonDays: 1.4,
  });

  deepStrictEqual(metrics[3], { label: 'Average horizon', value: '1 day' });
});
