import { useEffect, useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import type { CreatePortfolioStock, IPortfolioStock } from '../Interfaces/APIResponses/IPortfolioStock';
import type { LoadStatus } from '../API/types';
import { addPortfolioStock, deletePortfolioStock, getPortfolioStocks } from '../API/portfolioClient';

type PortfolioState = {
  values: IPortfolioStock[];
  status: LoadStatus;
  message: string | null;
};

type StateSetter = Dispatch<SetStateAction<PortfolioState>>;

export function usePortfolioStocks() {
  const [state, setState] = useState<PortfolioState>(initialState);
  useEffect(() => {
    void loadPortfolio(setState);
  }, []);
  const add = (stock: CreatePortfolioStock) => addPortfolio(stock, state.values, setState);
  const remove = (id: number) => removePortfolio(id, setState);
  return { ...state, add, remove };
}

async function loadPortfolio(setState: StateSetter) {
  setState(loadingState());
  const result = await getPortfolioStocks();
  if (!result.ok) {
    setState(errorState(result.message));
    return;
  }
  setState(result.data.length > 0 ? successState(result.data) : emptyState());
}

async function addPortfolio(
  stock: CreatePortfolioStock,
  values: IPortfolioStock[],
  setState: StateSetter,
) {
  const request = normalizeStock(stock);
  if (portfolioHasSymbol(values, request.symbol)) {
    setState(errorState(`${request.symbol} is already in your portfolio.`, values));
    return false;
  }
  return applyAddResult(await addPortfolioStock(request), setState);
}

async function removePortfolio(id: number, setState: StateSetter) {
  const result = await deletePortfolioStock(id);
  if (!result.ok) {
    setState((state) => errorState(result.message, state.values));
    return false;
  }
  setState((state) => successState(removeStock(state.values, id)));
  return true;
}

function applyAddResult(result: Awaited<ReturnType<typeof addPortfolioStock>>, setState: StateSetter) {
  if (!result.ok) {
    setState((state) => errorState(result.message, state.values));
    return false;
  }
  setState((state) => successState([...state.values, result.data]));
  return true;
}

function normalizeStock(stock: CreatePortfolioStock): CreatePortfolioStock {
  return { ...stock, symbol: stock.symbol.trim().toUpperCase() };
}

function removeStock(values: IPortfolioStock[], id: number) {
  return values.filter((stock) => stock.id !== id);
}

function portfolioHasSymbol(values: IPortfolioStock[], symbol: string) {
  return values.some((stock) => stock.symbol.toUpperCase() === symbol);
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
