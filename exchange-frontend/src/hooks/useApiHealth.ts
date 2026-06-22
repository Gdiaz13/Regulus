import { useEffect, useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import { getApiHealth } from '../API/healthClient';
import type { LoadStatus } from '../API/types';
import type { IApiHealth } from '../Interfaces/APIResponses/IApiHealth';
import { setIfActive, useActiveFlag } from './useActiveFlag';
import type { ActiveFlag } from './useActiveFlag';

type ApiHealthState = {
  value: IApiHealth | null;
  status: LoadStatus;
  message: string | null;
};

type StateSetter = Dispatch<SetStateAction<ApiHealthState>>;

export function useApiHealth() {
  const [state, setState] = useState<ApiHealthState>(loadingState());
  const active = useActiveFlag();
  useEffect(() => {
    void loadHealth(active, setState);
  }, [active]);
  return state;
}

async function loadHealth(active: ActiveFlag, setState: StateSetter) {
  const result = await getApiHealth();
  setIfActive(active, setState, result.ok ? successState(result.data) : errorState(result.message));
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
