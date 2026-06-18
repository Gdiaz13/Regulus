export type ApiResult<T> =
  | { ok: true; data: T }
  | { ok: false; message: string };

export type LoadStatus = "idle" | "loading" | "success" | "empty" | "error";
