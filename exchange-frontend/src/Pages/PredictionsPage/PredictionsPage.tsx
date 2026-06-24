import { useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import ResourceStatus from '../../Components/AsyncResource/ResourceStatus';
import PredictionForm from '../../Components/Prediction/PredictionForm/PredictionForm';
import PredictionHistory from '../../Components/Prediction/PredictionHistory/PredictionHistory';
import PredictionOverview from '../../Components/Prediction/PredictionOverview/PredictionOverview';
import StagedAssets from '../../Components/Prediction/StagedAssets/StagedAssets';
import type { IPredictAsset } from '../../Interfaces/APIResponses/IPrediction';
import { usePrediction } from '../../hooks/usePrediction';
import { usePredictionHistory } from '../../hooks/usePredictionHistory';
import styles from './PredictionsPage.module.css';

type Prediction = ReturnType<typeof usePrediction>;
type History = ReturnType<typeof usePredictionHistory>;

// Stage one or more assets, ask RegulasCoreAI through the gateway, and show the result.
const PredictionsPage = () => {
  const [staged, setStaged] = useState<IPredictAsset[]>([]);
  const prediction = usePrediction();
  const history = usePredictionHistory();
  const stage = useStaging(setStaged);
  return (
    <main className={styles.page}>
      <PredictionsHeader />
      <PredictionForm onAdd={stage.add} />
      <StagedAssets assets={staged} onRemove={stage.remove} onRun={() => runPrediction(staged, prediction, history)} running={prediction.status === 'loading'} />
      <PredictionResults prediction={prediction} />
      <PredictionHistory values={history.values} status={history.status} message={history.message} />
    </main>
  );
};

function useStaging(setStaged: SetStaged) {
  const add = (asset: IPredictAsset) => setStaged((list) => [...list, asset]);
  const remove = (index: number) => setStaged((list) => list.filter((_, position) => position !== index));
  return { add, remove };
}

function PredictionsHeader() {
  return (
    <header className={styles.header}>
      <p className={styles.eyebrow}>Predictions</p>
      <h1 className={styles.title}>Ask the models where things might go.</h1>
      <p className={styles.note}>Every prediction is mock for now and clearly flagged. The model estimates and signals are research, not advice.</p>
    </header>
  );
}

async function runPrediction(staged: IPredictAsset[], prediction: Prediction, history: History) {
  if (await prediction.predict(staged)) {
    await history.reload();
  }
}

function PredictionResults({ prediction }: { prediction: Prediction }) {
  if (prediction.status === 'success' && prediction.value) {
    return <PredictionOverview overview={prediction.value} />;
  }
  if (prediction.status === 'idle') {
    return <p className={styles.hint}>Add an asset above, then run a prediction.</p>;
  }
  return <ResourceStatus status={prediction.status} message={prediction.message} />;
}

type SetStaged = Dispatch<SetStateAction<IPredictAsset[]>>;

export default PredictionsPage;
