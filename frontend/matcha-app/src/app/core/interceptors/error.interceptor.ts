import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { MessageService } from 'primeng/api';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const messageService = inject(MessageService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred';

      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = `Client Error: ${error.error.message}`;
      } else {
        // Server-side error
        switch (error.status) {
          case 400:
            errorMessage = error.error?.message || 'Bad Request - Please check your input';
            break;
          case 401:
            errorMessage = error.error?.message || 'Unauthorized - Please login again';
            break;
          case 403:
            errorMessage = error.error?.message || 'Forbidden - You do not have permission';
            break;
          case 404:
            errorMessage = error.error?.message || 'Not Found - The requested resource was not found';
            break;
          case 409:
            errorMessage = error.error?.message || 'Conflict - The resource already exists';
            break;
          case 422:
            errorMessage = error.error?.message || 'Validation Error - Please check your input';
            break;
          case 429:
            errorMessage = error.error?.message || 'Too Many Requests - Please try again later';
            break;
          case 500:
            errorMessage = error.error?.message || 'Internal Server Error - Please try again later';
            break;
          case 502:
            errorMessage = error.error?.message || 'Bad Gateway - Server is temporarily unavailable';
            break;
          case 503:
            errorMessage = error.error?.message || 'Service Unavailable - Server is temporarily unavailable';
            break;
          case 504:
            errorMessage = error.error?.message || 'Gateway Timeout - Request timed out';
            break;
          default:
            errorMessage = error.error?.message || `Server Error (${error.status}): ${error.statusText}`;
        }
      }

      // Show error message using PrimeNG MessageService
      messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: errorMessage,
        life: 5000, // Message will disappear after 5 seconds
        closable: true
      });

      // Log error for debugging
      console.error('HTTP Error intercepted:', {
        status: error.status,
        statusText: error.statusText,
        url: req.url,
        error: error.error,
        message: errorMessage
      });

      // Re-throw the error so components can still handle it if needed
      return throwError(() => error);
    })
  );
};
