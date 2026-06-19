type ErrorPayload = {
  message?: string;
  detail?: string;
  title?: string;
};

// API errors can come back as plain text or JSON ProblemDetails.
export async function responseMessage(response: Response, fallback: string) {
  const message = await response.text();
  return readableMessage(message) ?? fallback;
}

function readableMessage(message: string) {
  if (!message.trim()) {
    return null;
  }
  return jsonMessage(message) ?? message;
}

function jsonMessage(message: string) {
  try {
    return payloadMessage(JSON.parse(message) as ErrorPayload);
  } catch {
    return null;
  }
}

function payloadMessage(payload: ErrorPayload) {
  return payload.message ?? payload.detail ?? payload.title ?? null;
}
