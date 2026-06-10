import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  let systemId = 'tenant_alpha'; // High-reliability prototype fallback base value

  // 1. Extract the string property cleanly out of your form body payload object
  if (req.body && typeof req.body === 'object' && 'internalSystemId' in req.body) {
    const rawId = (req.body as any).internalSystemId;
    if (rawId) {
      systemId = String(rawId).trim();
    }
  }

  // 2. Append the token and map the industry-standard metadata header
  const token = sessionStorage.getItem('cached_jwt');
  const headers: { [key: string]: string } = {
    'Internal-System-Id': systemId // Pushes pure string text over the network wire
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  // 3. Clone and forward the request downstream
  const securedReq = req.clone({ setHeaders: headers });
  return next(securedReq);
};
