import type { CSSProperties } from 'react';
import type { LoadStatus } from '../../API/types';
import Spinner from '../Spinner/Spinner';

const defaultMessageStyle = {
  color: '#FFD700',
  marginTop: '2rem',
  textAlign: 'center',
} satisfies CSSProperties;

type Props = {
  status: LoadStatus;
  message: string | null;
  style?: CSSProperties;
};

export default function ResourceStatus({ status, message, style }: Props) {
  if (status === 'loading') {
    return <Spinner />;
  }

  return <div style={{ ...defaultMessageStyle, ...style }}>{messageText(message)}</div>;
}

function messageText(message: string | null) {
  return message ?? 'No data available.';
}
