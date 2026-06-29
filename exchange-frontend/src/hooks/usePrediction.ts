import { useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import type { LoadStatus } from '../API/types';
import { postPrediction } from '../API/predictionClient';
import type { IAiOverview, IPredictAsset } from '../Interfaces/APIResponses/IPrediction';
import { setIfActive, useActiveFlag } from './useActiveFlag';
import type { ActiveFlag } from './useActiveFlag';

type PredictionState = {
  value: IAiOverview | null;
  status: LoadStatus;
  message: string | null;
};

type StateSetter = Dispatch<SetStateAction<PredictionState>>;

// Runs on demand (not on mount): the user stages assets, then asks for a prediction.
export function usePrediction() {
  const [state, setState] = useState<PredictionState>(initialState);
  const active = useActiveFlag();
  const predict = (assets: IPredictAsset[]) => runPrediction(assets, active, setState);
  const reset = () => setState(initialState);
  return { ...state, predict, reset };
}

async function runPrediction(assets: IPredictAsset[], active: ActiveFlag, setState: StateSetter) {
  setIfActive(active, setState, loadingState());
  const result = await postPrediction(assets);
  if (!result.ok) {
    setIfActive(active, setState, errorState(result.message));
    return false;
  }
  setIfActive(active, setState, successState(result.data));
  return true;
}

function loadingState(): PredictionState {
  return { value: null, status: 'loading', message: null };
}

function successState(value: IAiOverview): PredictionState {
  const hasResults = value.categories.length > 0;
  return { value, status: hasResults ? 'success' : 'empty', message: hasResults ? null : 'No predictions came back.' };
}

function errorState(message: string): PredictionState {
  return { value: null, status: 'error', message };
}

const initialState = {
  value: null,
  status: 'idle',
  message: null,
} satisfies PredictionState;
