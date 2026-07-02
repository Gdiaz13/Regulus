import type { ApiResult } from '../API/types';
import type { IAuthResponse, ICurrentUser, LoginRequest, RegisterRequest } from '../Interfaces/APIResponses/IAuth';

export type AuthStatus = 'loading' | 'authenticated' | 'anonymous';

export type AuthState = {
  status: AuthStatus;
  user: ICurrentUser | null;
  message: string | null;
};

export type AuthContextValue = AuthState & {
  login: (request: LoginRequest) => Promise<ApiResult<IAuthResponse>>;
  register: (request: RegisterRequest) => Promise<ApiResult<IAuthResponse>>;
  logout: () => Promise<void>;
};
