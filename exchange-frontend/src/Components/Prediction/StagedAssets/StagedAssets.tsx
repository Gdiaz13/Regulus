import { formatCurrency } from '../../../lib/formatters';
import type { IPredictAsset } from '../../../Interfaces/APIResponses/IPrediction';
import styles from './StagedAssets.module.css';

type Props = {
  assets: IPredictAsset[];
  onRemove: (index: number) => void;
  onRun: () => void;
  running: boolean;
};

// The assets the user has staged but not yet sent for a prediction.
export default function StagedAssets({ assets, onRemove, onRun, running }: Props) {
  if (assets.length === 0) {
    return null;
  }
  return (
    <section className={styles.staged}>
      <ul className={styles.list}>
        {assets.map((asset, index) => <StagedItem key={itemKey(asset, index)} asset={asset} onRemove={() => onRemove(index)} />)}
      </ul>
      <button type="button" onClick={onRun} disabled={running}>{runLabel(running, assets.length)}</button>
    </section>
  );
}

function StagedItem({ asset, onRemove }: { asset: IPredictAsset; onRemove: () => void }) {
  return (
    <li className={styles.item}>
      <span className={styles.symbol}>{asset.symbol}</span>
      <span className={styles.meta}>{asset.assetType}{asset.category ? ` - ${asset.category}` : ''} - {formatCurrency(asset.currentPrice)}</span>
      <button type="button" className={styles.remove} onClick={onRemove} aria-label={`Remove ${asset.symbol}`}>x</button>
    </li>
  );
}

function runLabel(running: boolean, count: number) {
  return running ? 'Predicting...' : `Run prediction (${count})`;
}

function itemKey(asset: IPredictAsset, index: number) {
  return `${asset.symbol}-${index}`;
}
