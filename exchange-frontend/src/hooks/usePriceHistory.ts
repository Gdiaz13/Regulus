import { useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import type { ApiResult, LoadStatus } from '../API/types';
import { capturePriceHistory, getPriceHistory } from '../API/priceHistoryClient';
import type { IPriceHistory } from '../Interfaces/APIResponses/IPriceHistory';
import { setIfActive, useActiveFlag } from './useActiveFlag';
import type { ActiveFlag } from './useActiveFlag';

type State = {
  value: IPriceHistory | null;
  status: LoadStatus;
  message: string | null;
};

type Setter = Dispatch<SetStateAction<State>>;

// Runs on demand: the user picks a symbol, then loads stored history or captures
// fresh history from the provider (which then loads what was just stored).
export function usePriceHistory() {
  const [state, setState] = useState<State>(initialState);
  const active = useActiveFlag();
  const load = (symbol: string, assetType: string, take?: number) => runLoad(symbol, assetType, take, active, setState);
  const capture = (symbol: string, assetType: string, take?: number) => runCapture(symbol, assetType, take, active, setState);
  return { ...state, load, capture };
}

async function runLoad(symbol: string, assetType: string, take: number | undefined, active: ActiveFlag, setState: Setter) {
  setIfActive(active, setState, loadingState());
  applyResult(await getPriceHistory(symbol, assetType, take), active, setState);
}

async function runCapture(symbol: string, assetType: string, take: number | undefined, active: ActiveFlag, setState: Setter) {
  setIfActive(active, setState, loadingState());
  const captured = await capturePriceHistory(symbol, assetType);
  if (!captured.ok) {
    setIfActive(active, setState, errorState(captured.message));
    return;
  }
  applyResult(await getPriceHistory(symbol, assetType, take), active, setState);
}

function applyResult(result: ApiResult<IPriceHistory>, active: ActiveFlag, setState: Setter) {
  if (!result.ok) {
    setIfActive(active, setState, errorState(result.message));
    return;
  }
  setIfActive(active, setState, successState(result.data));
}

function loadingState(): State {
  return { value: null, status: 'loading', message: null };
}

function successState(value: IPriceHistory): State {
  const hasPoints = value.points.length > 0;
  return { value, status: hasPoints ? 'success' : 'empty', message: hasPoints ? null : 'No history stored yet. Capture from the provider to populate it.' };
}

function errorState(message: string): State {
  return { value: null, status: 'error', message };
}

const initialState = {
  value: null,
  status: 'idle',
  message: null,
} satisfies State;
