import { useEffect, useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import type { LoadStatus } from '../API/types';
import { getPredictionAccuracySummary } from '../API/predictionClient';
import type { IModelAccuracySummary } from '../Interfaces/APIResponses/IPrediction';
import { setIfActive, useActiveFlag } from './useActiveFlag';
import type { ActiveFlag } from './useActiveFlag';

type State = {
  values: IModelAccuracySummary[];
  status: LoadStatus;
  message: string | null;
};

type Setter = Dispatch<SetStateAction<State>>;

export function usePredictionAccuracySummary() {
  const [state, setState] = useState<State>(initialState);
  const active = useActiveFlag();
  useEffect(() => {
    void loadSummary(active, setState);
  }, [active]);
  return state;
}

async function loadSummary(active: ActiveFlag, setState: Setter) {
  setIfActive(active, setState, loadingState());
  const result = await getPredictionAccuracySummary();
  if (!result.ok) {
    setIfActive(active, setState, errorState(result.message));
    return;
  }
  setIfActive(active, setState, successState(result.data));
}

function loadingState(): State {
  return { values: [], status: 'loading', message: null };
}

function successState(values: IModelAccuracySummary[]): State {
  const message = values.length > 0 ? null : 'No scored predictions yet. Scores appear after target dates have matching market prices.';
  return { values, status: values.length > 0 ? 'success' : 'empty', message };
}

function errorState(message: string): State {
  return { values: [], status: 'error', message };
}

const initialState = {
  values: [],
  status: 'idle',
  message: null,
} satisfies State;
