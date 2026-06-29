import { useEffect, useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import type { LoadStatus } from '../API/types';
import { getPredictionHistory } from '../API/predictionClient';
import type { IPredictionHistoryItem } from '../Interfaces/APIResponses/IPrediction';
import { setIfActive, useActiveFlag } from './useActiveFlag';
import type { ActiveFlag } from './useActiveFlag';

type State = {
  values: IPredictionHistoryItem[];
  status: LoadStatus;
  message: string | null;
};

type Setter = Dispatch<SetStateAction<State>>;

export function usePredictionHistory() {
  const [state, setState] = useState<State>(initialState);
  const active = useActiveFlag();
  useEffect(() => {
    void loadHistory(active, setState);
  }, [active]);
  const reload = () => loadHistory(active, setState);
  return { ...state, reload };
}

async function loadHistory(active: ActiveFlag, setState: Setter) {
  setIfActive(active, setState, loadingState());
  const result = await getPredictionHistory();
  if (!result.ok) {
    setIfActive(active, setState, errorState(result.message));
    return;
  }
  setIfActive(active, setState, successState(result.data));
}

function loadingState(): State {
  return { values: [], status: 'loading', message: null };
}

function successState(values: IPredictionHistoryItem[]): State {
  const message = values.length > 0 ? null : 'No saved predictions yet.';
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
