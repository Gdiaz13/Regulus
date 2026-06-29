import { useEffect, useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import type { LoadStatus } from '../API/types';
import { getTradingAgentsHealth, getTradingAgentsModelInfo } from '../API/tradingAgentsClient';
import type { ITradingAgentsHealth, ITradingAgentsModelInfo } from '../Interfaces/APIResponses/ITradingAgents';
import { setIfActive, useActiveFlag } from './useActiveFlag';
import type { ActiveFlag } from './useActiveFlag';

type State = {
  health: ITradingAgentsHealth | null;
  model: ITradingAgentsModelInfo | null;
  status: LoadStatus;
  message: string | null;
};

type Setter = Dispatch<SetStateAction<State>>;

export function useTradingAgentsStatus() {
  const [state, setState] = useState<State>(loadingState());
  const active = useActiveFlag();
  useEffect(() => {
    void loadStatus(active, setState);
  }, [active]);
  return state;
}

async function loadStatus(active: ActiveFlag, setState: Setter) {
  const [health, model] = await Promise.all([getTradingAgentsHealth(), getTradingAgentsModelInfo()]);
  if (!health.ok || !model.ok) {
    setIfActive(active, setState, errorState(failureMessage(health, model)));
    return;
  }
  setIfActive(active, setState, successState(health.data, model.data));
}

function failureMessage(...results: Array<{ ok: boolean; message?: string }>) {
  return results.find((result) => !result.ok)?.message ?? 'TradingAgents status unavailable.';
}

function loadingState(): State {
  return { health: null, model: null, status: 'loading', message: null };
}

function successState(health: ITradingAgentsHealth, model: ITradingAgentsModelInfo): State {
  return { health, model, status: 'success', message: null };
}

function errorState(message: string): State {
  return { health: null, model: null, status: 'error', message };
}
