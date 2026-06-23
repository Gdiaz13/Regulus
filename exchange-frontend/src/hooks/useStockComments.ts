import { useEffect, useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import type { LoadStatus } from '../API/types';
import { addStockComment, deleteStockComment, getStockComments, updateStockComment } from '../API/commentClient';
import type { CreateStockComment, IStockComment } from '../Interfaces/APIResponses/IStockComment';
import { setIfActive, useActiveFlag } from './useActiveFlag';
import type { ActiveFlag } from './useActiveFlag';

type CommentState = {
  values: IStockComment[];
  status: LoadStatus;
  message: string | null;
};

type StateSetter = Dispatch<SetStateAction<CommentState>>;

export function useStockComments(stockId: number) {
  const [state, setState] = useState<CommentState>(initialState);
  const active = useActiveFlag();
  useEffect(() => {
    void loadComments(stockId, active, setState);
  }, [active, stockId]);
  const add = (comment: CreateStockComment) => addComment(stockId, comment, active, setState);
  const update = (id: number, comment: CreateStockComment) => updateComment(id, comment, active, setState);
  const remove = (id: number) => removeComment(id, active, setState);
  return { ...state, add, update, remove };
}

async function loadComments(stockId: number, active: ActiveFlag, setState: StateSetter) {
  setIfActive(active, setState, loadingState());
  const result = await getStockComments(stockId);
  if (!result.ok) {
    setIfActive(active, setState, errorState(result.message));
    return;
  }
  setIfActive(active, setState, successState(result.data));
}

async function addComment(
  stockId: number,
  comment: CreateStockComment,
  active: ActiveFlag,
  setState: StateSetter,
) {
  const result = await addStockComment(stockId, comment);
  return applyAddResult(result, active, setState);
}

async function removeComment(id: number, active: ActiveFlag, setState: StateSetter) {
  const result = await deleteStockComment(id);
  if (!result.ok) {
    setIfActive(active, setState, (state) => errorState(result.message, state.values));
    return false;
  }
  setIfActive(active, setState, (state) => successState(removeById(state.values, id)));
  return true;
}

async function updateComment(
  id: number,
  comment: CreateStockComment,
  active: ActiveFlag,
  setState: StateSetter,
) {
  const result = await updateStockComment(id, comment);
  return applyUpdateResult(result, active, setState);
}

function applyAddResult(
  result: Awaited<ReturnType<typeof addStockComment>>,
  active: ActiveFlag,
  setState: StateSetter,
) {
  if (!result.ok) {
    setIfActive(active, setState, (state) => errorState(result.message, state.values));
    return false;
  }
  setIfActive(active, setState, (state) => successState([result.data, ...state.values]));
  return true;
}

function applyUpdateResult(
  result: Awaited<ReturnType<typeof updateStockComment>>,
  active: ActiveFlag,
  setState: StateSetter,
) {
  if (!result.ok) {
    setIfActive(active, setState, (state) => errorState(result.message, state.values));
    return false;
  }
  setIfActive(active, setState, (state) => successState(replaceById(state.values, result.data)));
  return true;
}

function removeById(values: IStockComment[], id: number) {
  return values.filter((comment) => comment.id !== id);
}

function replaceById(values: IStockComment[], next: IStockComment) {
  return values.map((comment) => comment.id === next.id ? next : comment);
}

function loadingState(): CommentState {
  return { values: [], status: 'loading', message: null };
}

function successState(values: IStockComment[]): CommentState {
  return { values, status: values.length > 0 ? 'success' : 'empty', message: null };
}

function errorState(message: string, values: IStockComment[] = []): CommentState {
  return { values, status: 'error', message };
}

const initialState = {
  values: [],
  status: 'idle',
  message: null,
} satisfies CommentState;
