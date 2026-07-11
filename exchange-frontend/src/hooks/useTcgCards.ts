import { useRef, useState } from 'react';
import type { Dispatch, MutableRefObject, SetStateAction } from 'react';
import type { ApiResult, LoadStatus } from '../API/types';
import { getMagicCard, getPokemonCard, searchMagicCards, searchPokemonCards } from '../API/tcgClient';
import { getPriceHistory } from '../API/priceHistoryClient';
import type { IPriceHistory } from '../Interfaces/APIResponses/IPriceHistory';
import type { IMagicCardDetail, IMagicCardSearchResponse, IMagicCardSummary } from '../Interfaces/APIResponses/IMagicCard';
import type { IPokemonCardDetail, IPokemonCardSearchResponse, IPokemonCardSummary } from '../Interfaces/APIResponses/IPokemonCard';
import { useActiveFlag } from './useActiveFlag';
import type { ActiveFlag } from './useActiveFlag';

export type TcgGame = 'pokemon' | 'magic';
export type TcgCardSummary = IPokemonCardSummary | IMagicCardSummary;
export type TcgCardDetail = IPokemonCardDetail | IMagicCardDetail;
type SearchResponse = IPokemonCardSearchResponse | IMagicCardSearchResponse;

type State = {
  game: TcgGame;
  query: string;
  results: TcgCardSummary[];
  selected: TcgCardDetail | null;
  history: IPriceHistory | null;
  searchStatus: LoadStatus;
  detailStatus: LoadStatus;
  historyStatus: LoadStatus;
  searchMessage: string | null;
  detailMessage: string | null;
  historyMessage: string | null;
};

type Setter = Dispatch<SetStateAction<State>>;
type RequestGuard = MutableRefObject<number>;
type RequestScope = { game: TcgGame; key: number };

export function useTcgCards() {
  const [state, setState] = useState<State>(initialState);
  const active = useActiveFlag();
  const guard = useRef(0);
  const setQuery = (query: string) => changeQuery(query, state.query, guard, setState);
  const setGame = (game: TcgGame) => changeGame(game, state.game, guard, setState);
  const search = () => runSearch(beginScope(state.game, guard), state.query, active, guard, setState);
  const select = (id: string) => runSelect(beginScope(state.game, guard), id, active, guard, setState);
  return { ...state, setGame, setQuery, search, select };
}

async function runSearch(scope: RequestScope, query: string, active: ActiveFlag, guard: RequestGuard, setState: Setter) {
  const clean = query.trim();
  if (!clean) {
    setIfCurrent(active, guard, scope, setState, emptySearch('Enter a card name.'));
    return;
  }
  setIfCurrent(active, guard, scope, setState, searching(scope.game, clean));
  await applySearch(await searchCards(scope.game, clean), scope, active, guard, setState);
}

async function applySearch(result: ApiResult<SearchResponse>, scope: RequestScope, active: ActiveFlag, guard: RequestGuard, setState: Setter) {
  if (!result.ok) {
    setIfCurrent(active, guard, scope, setState, searchError(result.message));
    return;
  }
  setIfCurrent(active, guard, scope, setState, searchSuccess(result.data.cards, scope.game));
  await selectFirst(result.data.cards, scope, active, guard, setState);
}

async function selectFirst(cards: TcgCardSummary[], scope: RequestScope, active: ActiveFlag, guard: RequestGuard, setState: Setter) {
  const first = cards[0];
  if (first) {
    await runSelect(scope, first.id, active, guard, setState);
  }
}

async function runSelect(scope: RequestScope, id: string, active: ActiveFlag, guard: RequestGuard, setState: Setter) {
  setIfCurrent(active, guard, scope, setState, detailLoading());
  const detail = await getCard(scope.game, id);
  if (!detail.ok) {
    setIfCurrent(active, guard, scope, setState, detailError(detail.message));
    return;
  }
  setIfCurrent(active, guard, scope, setState, detailSuccess(detail.data));
  await loadHistory(id, scope, active, guard, setState);
}

async function loadHistory(id: string, scope: RequestScope, active: ActiveFlag, guard: RequestGuard, setState: Setter) {
  setIfCurrent(active, guard, scope, setState, historyLoading());
  const history = await getPriceHistory(id, 'TcgCard', 30);
  setIfCurrent(active, guard, scope, setState, history.ok ? historySuccess(history.data) : historyError(history.message));
}

function changeQuery(query: string, current: string, guard: RequestGuard, setState: Setter) {
  if (query === current) {
    setState((value) => ({ ...value, query }));
    return;
  }
  invalidate(guard);
  setState((value) => resetForQuery(value, query));
}

function resetForQuery(value: State, query: string): State {
  return { ...initialState, game: value.game, query };
}

function changeGame(game: TcgGame, current: TcgGame, guard: RequestGuard, setState: Setter) {
  if (game !== current) {
    invalidate(guard);
  }
  setState((value) => (game === value.game ? value : { ...initialState, game, query: value.query }));
}

function beginScope(game: TcgGame, guard: RequestGuard): RequestScope {
  return { game, key: ++guard.current };
}

function invalidate(guard: RequestGuard) {
  guard.current += 1;
}

function setIfCurrent(active: ActiveFlag, guard: RequestGuard, scope: RequestScope, setState: Setter, state: SetStateAction<State>) {
  if (active.current && guard.current === scope.key) {
    setState(state);
  }
}

function searchCards(game: TcgGame, query: string) {
  return game === 'pokemon' ? searchPokemonCards(query) : searchMagicCards(query);
}

function getCard(game: TcgGame, id: string) {
  return game === 'pokemon' ? getPokemonCard(id) : getMagicCard(id);
}

function searching(game: TcgGame, query: string): State {
  return { ...initialState, game, query, searchStatus: 'loading' };
}

function searchSuccess(results: TcgCardSummary[], game: TcgGame): SetStateAction<State> {
  return (state) => ({ ...state, results, searchStatus: results.length ? 'success' : 'empty', searchMessage: emptyResults(results, game) });
}

function emptyResults(results: TcgCardSummary[], game: TcgGame) {
  return results.length ? null : `No matching ${gameLabel(game)} cards.`;
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

function detailSuccess(selected: TcgCardDetail): SetStateAction<State> {
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

function gameLabel(game: TcgGame) {
  return game === 'pokemon' ? 'Pokemon' : 'Magic';
}

const initialState = {
  game: 'pokemon',
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
