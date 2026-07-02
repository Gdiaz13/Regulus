const tokenKey = 'regulas.auth.token';

let memoryToken = storedToken();

export function getAuthToken() {
  return memoryToken;
}

export function setAuthToken(token: string) {
  memoryToken = token;
  sessionStorage.setItem(tokenKey, token);
}

export function clearAuthToken() {
  memoryToken = null;
  sessionStorage.removeItem(tokenKey);
}

function storedToken() {
  return typeof sessionStorage === 'undefined' ? null : sessionStorage.getItem(tokenKey);
}
