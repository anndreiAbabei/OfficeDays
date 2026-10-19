export class ApiError extends Error {
  constructor(message: string, public status: number) { 
    super(message); 
  }
}
let csrf: Promise<string> | undefined;

let unauthorized = () => {};

export function onUnauthorized(handler: () => void): void {
   unauthorized = handler; 
}

export function resetCsrf(): void { 
  csrf = undefined; 
}

async function csrfToken(): Promise<string> {
  csrf ??= request<{ token: string }>('/api/auth/csrf')
              .then(data => data.token)
              .catch(error => { csrf = undefined; throw error;});

  return csrf;
}

async function requestResponse(path: string, options: RequestInit = {}): Promise<Response> {
  const headers = new Headers(options.headers);
  const method = (options.method || 'GET').toUpperCase();

  if (options.body) 
    headers.set('Content-Type', 'application/json');

  if (!['GET', 'HEAD', 'OPTIONS'].includes(method)) 
    headers.set('X-CSRF-TOKEN', await csrfToken());

  const response = await fetch(path, { ...options, headers, credentials: 'same-origin', cache: 'no-store' });
  
  if (!response.ok) {
    const problem = await response.json()
                                  .catch(() => ({}));
    const validation = problem.errors && 
                       Object.values(problem.errors).flat()[0];
    
    if (response.status === 401 && path !== '/api/auth/login') {
      resetCsrf();
      unauthorized();
    }

    const error = response.status === 401
        ? 'Please sign in again.'
        : `Request failed (${response.status})`;

    throw new ApiError(String(validation || 
                              problem.detail || 
                              problem.title ||
                              error), 
                       response.status);
  }

  return response;
}

export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const response = await requestResponse(path, options);
  return response.status === 204 
            ? undefined as T 
            : response.json();
}
export function mutate<T>(path: string, method: string, body?: unknown): Promise<T> {
  return request<T>(path, { method, body: body === undefined ? undefined : JSON.stringify(body) });
}
export const errorMessage = (error: unknown) => error instanceof Error 
    ? error.message 
    : 'Something went wrong. Please try again.';

export async function downloadPersonalData(): Promise<void> {
  const response = await requestResponse('/api/users/me/export');
  const blob = await response.blob();
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = 'officedays-data.json';
  document.body.appendChild(link);
  try {
    link.click();
  } finally {
    link.remove();
    // Give the browser time to start consuming the download before releasing it.
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }
}
