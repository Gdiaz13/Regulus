import { useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import type { ApiResult, LoadStatus } from '../API/types';
import { getPokemonCard, searchPokemonCards } from '../API/tcgClient';
import { getPriceHistory } from '../API/priceHistoryClient';
import type { IPriceHistory } from '../Interfaces/APIResponses/IPriceHistory';
import type { IPokemonCardDetail, IPokemonCardSearchResponse, IPokemonCardSummary } from '../Interfaces/APIResponses/IPokemonCard';
import { setIfActive, useActiveFlag } from './useActiveFlag';
import type { ActiveFlag } from './useActiveFlag';

type State = {
  query: string;
  results: IPokemonCardSummary[];
  selected: IPokemonCardDetail | null;
  history: IPriceHistory | null;
  searchStatus: LoadStatus;
  detailStatus: LoadStatus;
  historyStatus: LoadStatus;
  searchMessage: string | null;
  detailMessage: string | null;
  historyMessage: string | null;
};

type Setter = Dispatch<SetStateAction<State>>;

export function usePokemonCards() {
  const [state, setState] = useState<State>(initialState);
  const active = useActiveFlag();
  const setQuery = (query: string) => setState((value) => ({ ...value, query }));
  const search = () => runSearch(state.query, active, setState);
  const select = (id: string) => runSelect(id, active, setState);
  return { ...state, setQuery, search, select };
}

async function runSearch(query: string, active: ActiveFlag, setState: Setter) {
  const clean = query.trim();
  if (!clean) {
    setIfActive(active, setState, emptySearch('Enter a card name.'));
    return;
  }
  setIfActive(active, setState, searching(clean));
  await applySearch(await searchPokemonCards(clean), active, setState);
}

async function applySearch(result: ApiResult<IPokemonCardSearchResponse>, active: ActiveFlag, setState: Setter) {
  if (!result.ok) {
    setIfActive(active, setState, searchError(result.message));
    return;
  }
  setIfActive(active, setState, searchSuccess(result.data.cards));
  const first = result.data.cards[0];
  if (first) {
    await runSelect(first.id, active, setState);
  }
}

async function runSelect(id: string, active: ActiveFlag, setState: Setter) {
  setIfActive(active, setState, detailLoading());
  const detail = await getPokemonCard(id);
  if (!detail.ok) {
    setIfActive(active, setState, detailError(detail.message));
    return;
  }
  setIfActive(active, setState, detailSuccess(detail.data));
  await loadHistory(id, active, setState);
}

async function loadHistory(id: string, active: ActiveFlag, setState: Setter) {
  setIfActive(active, setState, historyLoading());
  const history = await getPriceHistory(id, 'TcgCard', 30);
  setIfActive(active, setState, history.ok ? historySuccess(history.data) : historyError(history.message));
}

function searching(query: string): State {
  return { ...initialState, query, searchStatus: 'loading' };
}

function searchSuccess(results: IPokemonCardSummary[]): SetStateAction<State> {
  return (state) => ({ ...state, results, searchStatus: results.length ? 'success' : 'empty', searchMessage: emptyResults(results) });
}

function emptyResults(results: IPokemonCardSummary[]) {
  return results.length ? null : 'No matching Pokemon cards.';
}

function emptySearch(message: string): SetStateAction<State> {
  return (state) => ({ ...state, searchStatus: 'empty', searchMessage: message });
}

function searchError(message: string): SetStateAction<State> {
  return (state) => ({ ...state, results: [], searchStatus: 'error', searchMessage: message });
}

function detailLoading(): SetStateAction<State> {
  return (state) => ({ ...state, selected: null, history: null, detailStatus: 'loading', historyStatus: 'idle' });
}

function detailSuccess(selected: IPokemonCardDetail): SetStateAction<State> {
  return (state) => ({ ...state, selected, detailStatus: 'success', detailMessage: null });
}

function detailError(message: string): SetStateAction<State> {
  return (state) => ({ ...state, selected: null, detailStatus: 'error', detailMessage: message });
}

function historyLoading(): SetStateAction<State> {
  return (state) => ({ ...state, historyStatus: 'loading', historyMessage: null });
}

function historySuccess(history: IPriceHistory): SetStateAction<State> {
  return (state) => ({ ...state, history, historyStatus: history.points.length ? 'success' : 'empty', historyMessage: historyMessage(history) });
}

function historyError(message: string): SetStateAction<State> {
  return (state) => ({ ...state, history: null, historyStatus: 'error', historyMessage: message });
}

function historyMessage(history: IPriceHistory) {
  return history.points.length ? null : 'No stored prices for this card yet.';
}

const initialState = {
  query: '',
  results: [],
  selected: null,
  history: null,
  searchStatus: 'idle',
  detailStatus: 'idle',
  historyStatus: 'idle',
  searchMessage: null,
  detailMessage: null,
  historyMessage: null,
} satisfies State;
