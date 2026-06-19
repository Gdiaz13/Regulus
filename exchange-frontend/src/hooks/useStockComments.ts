import { useEffect, useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import type { LoadStatus } from '../API/types';
import { addStockComment, deleteStockComment, getStockComments } from '../API/commentClient';
import type { CreateStockComment, IStockComment } from '../Interfaces/APIResponses/IStockComment';

type CommentState = {
  values: IStockComment[];
  status: LoadStatus;
  message: string | null;
};

type StateSetter = Dispatch<SetStateAction<CommentState>>;

export function useStockComments(stockId: number) {
  const [state, setState] = useState<CommentState>(initialState);
  useEffect(() => {
    void loadComments(stockId, setState);
  }, [stockId]);
  const add = (comment: CreateStockComment) => addComment(stockId, comment, setState);
  const remove = (id: number) => removeComment(id, setState);
  return { ...state, add, remove };
}

async function loadComments(stockId: number, setState: StateSetter) {
  setState(loadingState());
  const result = await getStockComments(stockId);
  if (!result.ok) {
    setState(errorState(result.message));
    return;
  }
  setState(successState(result.data));
}

async function addComment(
  stockId: number,
  comment: CreateStockComment,
  setState: StateSetter,
) {
  const result = await addStockComment(stockId, comment);
  return applyAddResult(result, setState);
}

async function removeComment(id: number, setState: StateSetter) {
  const result = await deleteStockComment(id);
  if (!result.ok) {
    setState((state) => errorState(result.message, state.values));
    return false;
  }
  setState((state) => successState(removeById(state.values, id)));
  return true;
}

function applyAddResult(result: Awaited<ReturnType<typeof addStockComment>>, setState: StateSetter) {
  if (!result.ok) {
    setState((state) => errorState(result.message, state.values));
    return false;
  }
  setState((state) => successState([result.data, ...state.values]));
  return true;
}

function removeById(values: IStockComment[], id: number) {
  return values.filter((comment) => comment.id !== id);
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
