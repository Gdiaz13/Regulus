import { useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import type { LoadStatus } from '../API/types';
import { analyzeStock } from '../API/tradingAgentsClient';
import type { IStockTradingAgentsRequest, IStockTradingAgentsResponse } from '../Interfaces/APIResponses/ITradingAgents';
import { setIfActive, useActiveFlag } from './useActiveFlag';
import type { ActiveFlag } from './useActiveFlag';

type State = {
  value: IStockTradingAgentsResponse | null;
  status: LoadStatus;
  message: string | null;
};

type Setter = Dispatch<SetStateAction<State>>;

export function useStockTradingAgents() {
  const [state, setState] = useState<State>(initialState);
  const active = useActiveFlag();
  const analyze = (request: IStockTradingAgentsRequest) => runAnalysis(request, active, setState);
  return { ...state, analyze };
}

async function runAnalysis(request: IStockTradingAgentsRequest, active: ActiveFlag, setState: Setter) {
  setIfActive(active, setState, loadingState());
  const result = await analyzeStock(cleanRequest(request));
  if (!result.ok) {
    setIfActive(active, setState, errorState(result.message));
    return;
  }
  setIfActive(active, setState, successState(result.data));
}

function cleanRequest(request: IStockTradingAgentsRequest) {
  return { ...request, symbol: request.symbol.trim().toUpperCase() };
}

function loadingState(): State {
  return { value: null, status: 'loading', message: null };
}

function successState(value: IStockTradingAgentsResponse): State {
  return { value, status: 'success', message: null };
}

function errorState(message: string): State {
  return { value: null, status: 'error', message };
}

const initialState = {
  value: null,
  status: 'idle',
  message: null,
} satisfies State;
