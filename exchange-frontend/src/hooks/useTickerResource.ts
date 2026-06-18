import { useEffect, useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import type { ApiResult, LoadStatus } from '../API/types';

type TickerLoader<T> = (ticker: string) => Promise<ApiResult<T[]>>;
type Selector<TItem, TData> = (items: TItem[]) => TData | null;

export type ResourceState<T> = {
  data: T | null;
  status: LoadStatus;
  message: string | null;
};

type ResourceOptions<TItem, TData> = {
  ticker: string;
  load: TickerLoader<TItem>;
  select: Selector<TItem, TData>;
  emptyMessage: string;
};

export function useTickerFirstResource<T>(
  ticker: string,
  load: TickerLoader<T>,
  emptyMessage: string,
) {
  return useTickerResource({ ticker, load, select: firstItem, emptyMessage });
}

export function useTickerListResource<T>(
  ticker: string,
  load: TickerLoader<T>,
  emptyMessage: string,
) {
  return useTickerResource({ ticker, load, select: allItems, emptyMessage });
}

function useTickerResource<TItem, TData>(options: ResourceOptions<TItem, TData>) {
  const { emptyMessage, load, select, ticker } = options;
  const [state, setState] = useState(() => loadingResource<TData>());
  useEffect(() => startTickerLoad({ emptyMessage, load, select, ticker }, setState), [
    emptyMessage,
    load,
    select,
    ticker,
  ]);
  return state;
}

function startTickerLoad<TItem, TData>(
  options: ResourceOptions<TItem, TData>,
  setState: Dispatch<SetStateAction<ResourceState<TData>>>,
) {
  let active = true;
  setState(loadingResource<TData>());
  resolveTickerResource(options).then((state) => updateIfActive(active, setState, state));
  return () => {
    active = false;
  };
}

async function resolveTickerResource<TItem, TData>(options: ResourceOptions<TItem, TData>) {
  if (!options.ticker) {
    return errorResource<TData>('Missing company ticker.');
  }

  const result = await options.load(options.ticker);
  return mapApiResult(result, options.select, options.emptyMessage);
}

function mapApiResult<TItem, TData>(
  result: ApiResult<TItem[]>,
  select: Selector<TItem, TData>,
  emptyMessage: string,
) {
  if (!result.ok) {
    return errorResource<TData>(result.message);
  }

  const data = select(result.data);
  return data === null ? emptyResource<TData>(emptyMessage) : successResource(data);
}

function updateIfActive<T>(
  active: boolean,
  setState: Dispatch<SetStateAction<ResourceState<T>>>,
  state: ResourceState<T>,
) {
  if (active) {
    setState(state);
  }
}

function firstItem<T>(items: T[]): T | null {
  return items[0] ?? null;
}

function allItems<T>(items: T[]): T[] | null {
  return items.length > 0 ? items : null;
}

function loadingResource<T>(): ResourceState<T> {
  return { data: null, status: 'loading', message: null };
}

function successResource<T>(data: T): ResourceState<T> {
  return { data, status: 'success', message: null };
}

function emptyResource<T>(message: string): ResourceState<T> {
  return { data: null, status: 'empty', message };
}

function errorResource<T>(message: string): ResourceState<T> {
  return { data: null, status: 'error', message };
}
