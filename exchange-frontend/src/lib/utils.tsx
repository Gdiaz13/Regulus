import { clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

type CnFunction = {
  (...inputs: (string | number | null | undefined | false)[]): string;
};

export const cn: CnFunction = (...inputs) => {
  return twMerge(clsx(inputs));
};
