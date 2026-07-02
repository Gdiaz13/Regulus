export type ICurrentUser = {
  id: string;
  email: string;
  displayName: string;
  createdAt: string;
  lastLoginAt: string | null;
};

export type IAuthResponse = {
  token: string;
  expiresAt: string;
  user: ICurrentUser;
};

export type LoginRequest = {
  email: string;
  password: string;
};

export type RegisterRequest = LoginRequest & {
  displayName: string;
};
