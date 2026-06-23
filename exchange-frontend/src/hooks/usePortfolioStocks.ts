import { useEffect, useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import type { CreatePortfolioStock, IPortfolioStock } from '../Interfaces/APIResponses/IPortfolioStock';
import type { LoadStatus } from '../API/types';
import { addPortfolioStock, deletePortfolioStock, getPortfolioStocks, updatePortfolioStock } from '../API/portfolioClient';
import { setIfActive, useActiveFlag } from './useActiveFlag';
import type { ActiveFlag } from './useActiveFlag';

type PortfolioState = {
  values: IPortfolioStock[];
  status: LoadStatus;
  message: string | null;
};

type StateSetter = Dispatch<SetStateAction<PortfolioState>>;

export function usePortfolioStocks() {
  const [state, setState] = useState<PortfolioState>(initialState);
  const active = useActiveFlag();
  useEffect(() => {
    void loadPortfolio(active, setState);
  }, [active]);
  const add = (stock: CreatePortfolioStock) => addPortfolio(stock, state.values, active, setState);
  const update = (id: number, stock: CreatePortfolioStock) => updatePortfolio(id, stock, state.values, active, setState);
  const remove = (id: number) => removePortfolio(id, active, setState);
  return { ...state, add, update, remove };
}

async function loadPortfolio(active: ActiveFlag, setState: StateSetter) {
  setIfActive(active, setState, loadingState());
  const result = await getPortfolioStocks();
  if (!result.ok) {
    setIfActive(active, setState, errorState(result.message));
    return;
  }
  setIfActive(active, setState, result.data.length > 0 ? successState(result.data) : emptyState());
}

async function addPortfolio(
  stock: CreatePortfolioStock,
  values: IPortfolioStock[],
  active: ActiveFlag,
  setState: StateSetter,
) {
  const request = normalizeStock(stock);
  if (portfolioHasSymbol(values, request.symbol)) {
    setIfActive(active, setState, errorState(`${request.symbol} is already in your portfolio.`, values));
    return false;
  }
  return applyAddResult(await addPortfolioStock(request), active, setState);
}

async function removePortfolio(id: number, active: ActiveFlag, setState: StateSetter) {
  const result = await deletePortfolioStock(id);
  if (!result.ok) {
    setIfActive(active, setState, (state) => errorState(result.message, state.values));
    return false;
  }
  setIfActive(active, setState, (state) => successState(removeStock(state.values, id)));
  return true;
}

async function updatePortfolio(
  id: number,
  stock: CreatePortfolioStock,
  values: IPortfolioStock[],
  active: ActiveFlag,
  setState: StateSetter,
) {
  const request = normalizeStock(stock);
  if (rejectDuplicateUpdate(values, id, request.symbol, active, setState)) {
    return false;
  }
  return applyUpdateResult(await updatePortfolioStock(id, request), active, setState);
}

function rejectDuplicateUpdate(
  values: IPortfolioStock[],
  id: number,
  symbol: string,
  active: ActiveFlag,
  setState: StateSetter,
) {
  if (!portfolioHasOtherSymbol(values, id, symbol)) {
    return false;
  }
  setIfActive(active, setState, errorState(`${symbol} is already in your portfolio.`, values));
  return true;
}

function applyUpdateResult(
  result: Awaited<ReturnType<typeof updatePortfolioStock>>,
  active: ActiveFlag,
  setState: StateSetter,
) {
  if (!result.ok) {
    setIfActive(active, setState, (state) => errorState(result.message, state.values));
    return false;
  }
  setIfActive(active, setState, (state) => successState(replaceStock(state.values, result.data)));
  return true;
}

function applyAddResult(
  result: Awaited<ReturnType<typeof addPortfolioStock>>,
  active: ActiveFlag,
  setState: StateSetter,
) {
  if (!result.ok) {
    setIfActive(active, setState, (state) => errorState(result.message, state.values));
    return false;
  }
  setIfActive(active, setState, (state) => successState([...state.values, result.data]));
  return true;
}

function normalizeStock(stock: CreatePortfolioStock): CreatePortfolioStock {
  return { ...stock, symbol: stock.symbol.trim().toUpperCase() };
}

function removeStock(values: IPortfolioStock[], id: number) {
  return values.filter((stock) => stock.id !== id);
}

function replaceStock(values: IPortfolioStock[], next: IPortfolioStock) {
  return values.map((stock) => stock.id === next.id ? next : stock);
}

function portfolioHasSymbol(values: IPortfolioStock[], symbol: string) {
  return values.some((stock) => stock.symbol.toUpperCase() === symbol);
}

function portfolioHasOtherSymbol(values: IPortfolioStock[], id: number, symbol: string) {
  return values.some((stock) => stock.id !== id && stock.symbol.toUpperCase() === symbol);
}

function loadingState(): PortfolioState {
  return { values: [], status: 'loading', message: null };
}

function successState(values: IPortfolioStock[]): PortfolioState {
  return { values, status: values.length > 0 ? 'success' : 'empty', message: null };
}

function emptyState(): PortfolioState {
  return { values: [], status: 'empty', message: 'No stocks in your portfolio yet.' };
}

function errorState(message: string, values: IPortfolioStock[] = []): PortfolioState {
  return { values, status: 'error', message };
}

const initialState = {
  values: [],
  status: 'idle',
  message: null,
} satisfies PortfolioState;
