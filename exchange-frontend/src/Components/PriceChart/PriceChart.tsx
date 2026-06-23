import type { IPricePoint } from '../../Interfaces/APIResponses/IPriceHistory';
import styles from './PriceChart.module.css';

const width = 820;
const height = 320;
const padding = 44;

type Scale = { min: number; max: number; count: number };

// A dependency-free SVG line chart of closing prices over time.
export default function PriceChart({ points }: { points: IPricePoint[] }) {
  if (points.length < 2) {
    return <p className={styles.empty}>Need at least two days of data to draw a chart.</p>;
  }
  return <Chart points={points} scale={buildScale(points)} />;
}

function Chart({ points, scale }: { points: IPricePoint[]; scale: Scale }) {
  return (
    <svg className={styles.svg} viewBox={`0 0 ${width} ${height}`} role="img" aria-label="Closing price history">
      <GridLines scale={scale} />
      <polygon className={styles.area} points={areaPoints(points, scale)} />
      <polyline className={styles.line} points={linePoints(points, scale)} />
      <LastDot points={points} scale={scale} />
      <PriceLabels scale={scale} />
      <DateLabels points={points} />
    </svg>
  );
}

function LastDot({ points, scale }: { points: IPricePoint[]; scale: Scale }) {
  const last = points[points.length - 1];
  return <circle className={styles.dot} cx={xFor(scale.count - 1, scale.count)} cy={yFor(last.close, scale)} r={4} />;
}

function GridLines({ scale }: { scale: Scale }) {
  const lines = [scale.max, (scale.min + scale.max) / 2, scale.min];
  return (
    <g>
      {lines.map((value) => (
        <line key={value} className={styles.grid} x1={padding} x2={width - padding} y1={yFor(value, scale)} y2={yFor(value, scale)} />
      ))}
    </g>
  );
}

function PriceLabels({ scale }: { scale: Scale }) {
  return (
    <g>
      <text className={styles.axisText} x={8} y={yFor(scale.max, scale) + 4}>{money(scale.max)}</text>
      <text className={styles.axisText} x={8} y={yFor(scale.min, scale) + 4}>{money(scale.min)}</text>
    </g>
  );
}

function DateLabels({ points }: { points: IPricePoint[] }) {
  return (
    <g>
      <text className={styles.axisText} x={padding} y={height - 14}>{points[0].date}</text>
      <text className={styles.axisText} x={width - padding} y={height - 14} textAnchor="end">{points[points.length - 1].date}</text>
    </g>
  );
}

function buildScale(points: IPricePoint[]): Scale {
  const closes = points.map((point) => point.close);
  return { min: Math.min(...closes), max: Math.max(...closes), count: points.length };
}

function linePoints(points: IPricePoint[], scale: Scale) {
  return points.map((point, index) => `${xFor(index, scale.count)},${yFor(point.close, scale)}`).join(' ');
}

function areaPoints(points: IPricePoint[], scale: Scale) {
  const start = `${xFor(0, scale.count)},${height - padding}`;
  const end = `${xFor(scale.count - 1, scale.count)},${height - padding}`;
  return `${start} ${linePoints(points, scale)} ${end}`;
}

function xFor(index: number, count: number) {
  const span = width - padding * 2;
  return padding + (count <= 1 ? 0 : (index / (count - 1)) * span);
}

function yFor(value: number, scale: Scale) {
  const span = height - padding * 2;
  const range = scale.max - scale.min || 1;
  return padding + span - ((value - scale.min) / range) * span;
}

function money(value: number) {
  return `$${value.toFixed(2)}`;
}
