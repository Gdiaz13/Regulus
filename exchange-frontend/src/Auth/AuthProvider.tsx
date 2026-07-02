import { useEffect, useState } from 'react';
import type { PropsWithChildren } from 'react';
import { clearAuthToken, getAuthToken, setAuthToken } from '../API/authTokenStore';
import { getCurrentUser, loginUser, logoutUser, registerUser } from '../API/authClient';
import type { IAuthResponse } from '../Interfaces/APIResponses/IAuth';
import type { ApiResult } from '../API/types';
import { AuthContext } from './AuthContext';
import type { AuthContextValue, AuthState } from './types';

type Setter = (state: AuthState) => void;

export function AuthProvider({ children }: PropsWithChildren) {
  const [state, setState] = useState<AuthState>(initialAuthState);
  useEffect(() => {
    void hydrateSession(setState);
  }, []);
  return <AuthContext.Provider value={contextValue(state, setState)}>{children}</AuthContext.Provider>;
}

function contextValue(state: AuthState, setState: Setter): AuthContextValue {
  return {
    ...state,
    login: (request) => authenticate(() => loginUser(request), setState),
    register: (request) => authenticate(() => registerUser(request), setState),
    logout: () => logoutSession(setState),
  };
}

async function hydrateSession(setState: Setter) {
  if (!getAuthToken()) {
    setState(anonymousState(null));
    return;
  }
  applyCurrentUser(await getCurrentUser(), setState);
}

async function authenticate(
  request: () => Promise<ApiResult<IAuthResponse>>,
  setState: Setter,
) {
  setState(loadingState());
  const result = await request();
  return result.ok ? authSucceeded(result, setState) : authFailed(result, setState);
}

async function logoutSession(setState: Setter) {
  await logoutUser();
  clearAuthToken();
  setState(anonymousState(null));
}

function applyCurrentUser(result: Awaited<ReturnType<typeof getCurrentUser>>, setState: Setter) {
  if (result.ok) {
    setState(authenticatedState(result.data));
    return;
  }
  clearAuthToken();
  setState(anonymousState(result.message));
}

function authSucceeded(result: ApiResult<IAuthResponse>, setState: Setter) {
  if (result.ok) {
    setAuthToken(result.data.token);
    setState(authenticatedState(result.data.user));
  }
  return result;
}

function authFailed(result: ApiResult<IAuthResponse>, setState: Setter) {
  if (!result.ok) {
    clearAuthToken();
    setState(anonymousState(result.message));
  }
  return result;
}

function loadingState(): AuthState {
  return { status: 'loading', user: null, message: null };
}

function anonymousState(message: string | null): AuthState {
  return { status: 'anonymous', user: null, message };
}

function authenticatedState(user: AuthState['user']): AuthState {
  return { status: 'authenticated', user, message: null };
}

const initialAuthState = getAuthToken() ? loadingState() : anonymousState(null);
