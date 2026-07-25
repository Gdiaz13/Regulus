/** Mirrors the backend IAdminAware: anything carrying a user's admin rights. */
export type IAdminAware = {
  isAdmin: boolean;
};

export type ICurrentUser = IAdminAware & {
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
