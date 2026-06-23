import { useEffect, useRef } from 'react';
import type { Dispatch, SetStateAction } from 'react';

export type ActiveFlag = {
  current: boolean;
};

type StateSetter<T> = Dispatch<SetStateAction<T>>;

// Async hooks use this flag so old requests cannot update a closed screen.
export function useActiveFlag() {
  const active = useRef(true);
  useEffect(() => stopOnUnmount(active), [active]);
  return active;
}

export function setIfActive<T>(active: ActiveFlag, setState: StateSetter<T>, state: SetStateAction<T>) {
  if (active.current) {
    setState(state);
  }
}

function stopOnUnmount(active: ActiveFlag) {
  active.current = true;
  return () => {
    active.current = false;
  };
}
