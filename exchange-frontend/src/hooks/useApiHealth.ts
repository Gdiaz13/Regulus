import { useEffect, useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import { getApiHealth } from '../API/healthClient';
import type { LoadStatus } from '../API/types';
import type { IApiHealth } from '../Interfaces/APIResponses/IApiHealth';

type ApiHealthState = {
  value: IApiHealth | null;
  status: LoadStatus;
  message: string | null;
};

type ActiveFlag = { current: boolean };
type StateSetter = Dispatch<SetStateAction<ApiHealthState>>;

export function useApiHealth() {
  const [state, setState] = useState<ApiHealthState>(loadingState());
  useEffect(() => watchHealth(setState), []);
  return state;
}

function watchHealth(setState: StateSetter) {
  const active = { current: true };
  void loadHealth(active, setState);
  return () => stopWatching(active);
}

async function loadHealth(active: ActiveFlag, setState: StateSetter) {
  const result = await getApiHealth();
  updateIfActive(active, setState, result.ok ? successState(result.data) : errorState(result.message));
}

function updateIfActive(active: ActiveFlag, setState: StateSetter, state: ApiHealthState) {
  if (active.current) {
    setState(state);
  }
}

function stopWatching(active: ActiveFlag) {
  active.current = false;
}

function loadingState(): ApiHealthState {
  return { value: null, status: 'loading', message: null };
}

function successState(value: IApiHealth): ApiHealthState {
  return { value, status: 'success', message: null };
}

function errorState(message: string): ApiHealthState {
  return { value: null, status: 'error', message };
}
