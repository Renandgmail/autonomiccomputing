// Enhanced Error Handling Service for RepoLens
// Implementing PT-005: Comprehensive Error Handling Implementation

import { AxiosError } from 'axios';

// Error types and interfaces
export interface AppError {
  id: string;
  type: ErrorType;
  code: string;
  message: string;
  userMessage: string;
  context?: ErrorContext;
  timestamp: Date;
  severity: ErrorSeverity;
  retryable: boolean;
  actionable: boolean;
  actions?: ErrorAction[];
}

export interface ErrorContext {
  component?: string;
  operation?: string;
  endpoint?: string;
  repositoryId?: number;
  userId?: string;
  sessionId?: string;
  userAgent?: string;
  url?: string;
  timestamp?: string;
  additionalData?: Record<string, any>;
}

export interface ErrorAction {
  type: 'retry' | 'reload' | 'navigate' | 'contact' | 'dismiss' | 'report';
  label: string;
  handler?: () => void;
  url?: string;
}

export enum ErrorType {
  Network = 'network',
  Authentication = 'authentication',
  Authorization = 'authorization',
  Validation = 'validation',
  NotFound = 'not_found',
  ServerError = 'server_error',
  ClientError = 'client_error',
  Timeout = 'timeout',
  RateLimit = 'rate_limit',
  Maintenance = 'maintenance',
  Unknown = 'unknown'
}

export enum ErrorSeverity {
  Low = 'low',
  Medium = 'medium',
  High = 'high',
  Critical = 'critical'
}

// Error state management
interface ErrorState {
  activeErrors: Map<string, AppError>;
  errorHistory: AppError[];
  globalErrorHandler?: (error: AppError) => void;
  componentErrorHandlers: Map<string, (error: AppError) => void>;
  retryAttempts: Map<string, number>;
  maxRetries: number;
}

class ErrorService {
  private state: ErrorState;
  private readonly maxHistorySize = 100;
  private readonly sessionId: string;

  constructor() {
    this.sessionId = this.generateSessionId();
    this.state = {
      activeErrors: new Map(),
      errorHistory: [],
      componentErrorHandlers: new Map(),
      retryAttempts: new Map(),
      maxRetries: 3
    };

    // Set up global error handlers
    this.setupGlobalErrorHandlers();
  }

  // Generate unique session ID for tracking
  private generateSessionId(): string {
    return `session_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  // Set up global error handlers for unhandled errors
  private setupGlobalErrorHandlers(): void {
    // Global JavaScript errors
    window.addEventListener('error', (event) => {
      const error = this.createError({
        type: ErrorType.ClientError,
        code: 'JS_RUNTIME_ERROR',
        message: event.error?.message || event.message || 'JavaScript runtime error',
        context: {
          operation: 'global_error_handler',
          url: event.filename,
          additionalData: {
            line: event.lineno,
            column: event.colno,
            stack: event.error?.stack
          }
        }
      });
      this.handleError(error);
    });

    // Unhandled promise rejections
    window.addEventListener('unhandledrejection', (event) => {
      const error = this.createError({
        type: ErrorType.ClientError,
        code: 'UNHANDLED_PROMISE_REJECTION',
        message: event.reason?.message || 'Unhandled promise rejection',
        context: {
          operation: 'promise_rejection',
          additionalData: {
            reason: event.reason,
            stack: event.reason?.stack
          }
        }
      });
      this.handleError(error);
    });
  }

  // Create standardized error object
  createError({
    type,
    code,
    message,
    userMessage,
    context = {},
    severity,
    retryable,
    actionable
  }: {
    type: ErrorType;
    code: string;
    message: string;
    userMessage?: string;
    context?: Partial<ErrorContext>;
    severity?: ErrorSeverity;
    retryable?: boolean;
    actionable?: boolean;
  }): AppError {
    const errorId = this.generateErrorId();
    
    // Enhance context with session information
    const enhancedContext: ErrorContext = {
      ...context,
      sessionId: this.sessionId,
      userAgent: navigator.userAgent,
      url: window.location.href,
      timestamp: new Date().toISOString()
    };

    // Determine severity if not provided
    const determinedSeverity = severity || this.determineSeverity(type, code);
    
    // Determine if error is retryable
    const isRetryable = retryable !== undefined ? retryable : this.isRetryableError(type, code);
    
    // Determine if error requires user action
    const isActionable = actionable !== undefined ? actionable : this.isActionableError(type, code);
    
    // Generate user-friendly message if not provided
    const friendlyMessage = userMessage || this.generateUserFriendlyMessage(type, code, message);
    
    // Generate appropriate actions
    const actions = this.generateErrorActions(type, code, isRetryable, isActionable);

    return {
      id: errorId,
      type,
      code,
      message,
      userMessage: friendlyMessage,
      context: enhancedContext,
      timestamp: new Date(),
      severity: determinedSeverity,
      retryable: isRetryable,
      actionable: isActionable,
      actions
    };
  }

  // Generate unique error ID
  private generateErrorId(): string {
    return `err_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  // Determine error severity based on type and code
  private determineSeverity(type: ErrorType, code: string): ErrorSeverity {
    switch (type) {
      case ErrorType.Authentication:
      case ErrorType.Authorization:
        return ErrorSeverity.High;
      
      case ErrorType.ServerError:
        if (code.includes('500') || code.includes('INTERNAL')) {
          return ErrorSeverity.Critical;
        }
        return ErrorSeverity.High;
      
      case ErrorType.Network:
        if (code.includes('TIMEOUT') || code.includes('OFFLINE')) {
          return ErrorSeverity.Medium;
        }
        return ErrorSeverity.High;
      
      case ErrorType.NotFound:
        return ErrorSeverity.Medium;
      
      case ErrorType.Validation:
        return ErrorSeverity.Low;
      
      case ErrorType.RateLimit:
        return ErrorSeverity.Medium;
      
      case ErrorType.Maintenance:
        return ErrorSeverity.High;
      
      default:
        return ErrorSeverity.Medium;
    }
  }

  // Check if error is retryable
  private isRetryableError(type: ErrorType, code: string): boolean {
    switch (type) {
      case ErrorType.Network:
      case ErrorType.Timeout:
      case ErrorType.ServerError:
        return !code.includes('400') && !code.includes('401') && !code.includes('403');
      
      case ErrorType.RateLimit:
        return true;
      
      case ErrorType.Authentication:
      case ErrorType.Authorization:
      case ErrorType.Validation:
      case ErrorType.NotFound:
        return false;
      
      default:
        return false;
    }
  }

  // Check if error requires user action
  private isActionableError(type: ErrorType, code: string): boolean {
    switch (type) {
      case ErrorType.Authentication:
      case ErrorType.Authorization:
      case ErrorType.Validation:
      case ErrorType.NotFound:
        return true;
      
      case ErrorType.Network:
        return code.includes('OFFLINE');
      
      case ErrorType.Maintenance:
        return true;
      
      default:
        return false;
    }
  }

  // Generate user-friendly error messages
  private generateUserFriendlyMessage(type: ErrorType, code: string, originalMessage: string): string {
    switch (type) {
      case ErrorType.Authentication:
        return 'Please sign in to continue accessing RepoLens.';
      
      case ErrorType.Authorization:
        return 'You don\'t have permission to access this resource. Please contact your administrator.';
      
      case ErrorType.Network:
        if (code.includes('OFFLINE')) {
          return 'You appear to be offline. Please check your internet connection.';
        }
        if (code.includes('TIMEOUT')) {
          return 'The request is taking longer than expected. Please try again.';
        }
        return 'Unable to connect to RepoLens servers. Please check your connection and try again.';
      
      case ErrorType.ServerError:
        return 'We\'re experiencing technical difficulties. Our team has been notified and is working on a fix.';
      
      case ErrorType.NotFound:
        if (code.includes('REPOSITORY')) {
          return 'The requested repository could not be found. It may have been deleted or moved.';
        }
        if (code.includes('FILE')) {
          return 'The requested file could not be found. It may have been deleted or moved.';
        }
        return 'The requested resource could not be found.';
      
      case ErrorType.Validation:
        return 'Please check your input and try again.';
      
      case ErrorType.RateLimit:
        return 'You\'re making requests too quickly. Please wait a moment and try again.';
      
      case ErrorType.Maintenance:
        return 'RepoLens is temporarily unavailable for maintenance. Please try again later.';
      
      default:
        return 'An unexpected error occurred. Please try again or contact support if the problem persists.';
    }
  }

  // Generate appropriate error actions
  private generateErrorActions(type: ErrorType, code: string, retryable: boolean, actionable: boolean): ErrorAction[] {
    const actions: ErrorAction[] = [];

    // Always allow dismissing the error
    actions.push({
      type: 'dismiss',
      label: 'Dismiss',
      handler: () => this.dismissError
    });

    // Add retry action for retryable errors
    if (retryable) {
      actions.push({
        type: 'retry',
        label: 'Try Again',
        handler: () => this.retryLastOperation
      });
    }

    // Add specific actions based on error type
    switch (type) {
      case ErrorType.Authentication:
        actions.push({
          type: 'navigate',
          label: 'Sign In',
          url: '/login'
        });
        break;
      
      case ErrorType.Network:
        if (code.includes('OFFLINE')) {
          actions.push({
            type: 'reload',
            label: 'Reload Page'
          });
        }
        break;
      
      case ErrorType.NotFound:
        actions.push({
          type: 'navigate',
          label: 'Go to Dashboard',
          url: '/dashboard'
        });
        break;
      
      case ErrorType.ServerError:
        actions.push({
          type: 'report',
          label: 'Report Issue'
        });
        break;
      
      case ErrorType.Maintenance:
        actions.push({
          type: 'contact',
          label: 'Check Status',
          url: '/status'
        });
        break;
    }

    return actions;
  }

  // Handle API errors (from axios)
  handleApiError(axiosError: AxiosError, context: Partial<ErrorContext> = {}): AppError {
    const response = axiosError.response;
    const request = axiosError.request;
    
    let type: ErrorType;
    let code: string;
    let message: string;

    if (response) {
      // Server responded with error status
      const status = response.status;
      
      if (status === 401) {
        type = ErrorType.Authentication;
        code = 'AUTH_INVALID_TOKEN';
        message = 'Authentication token is invalid or expired';
      } else if (status === 403) {
        type = ErrorType.Authorization;
        code = 'AUTH_INSUFFICIENT_PERMISSIONS';
        message = 'Insufficient permissions for this operation';
      } else if (status === 404) {
        type = ErrorType.NotFound;
        code = 'RESOURCE_NOT_FOUND';
        message = 'Requested resource not found';
      } else if (status === 422) {
        type = ErrorType.Validation;
        code = 'VALIDATION_FAILED';
        message = (response.data as any)?.message || 'Validation failed';
      } else if (status === 429) {
        type = ErrorType.RateLimit;
        code = 'RATE_LIMIT_EXCEEDED';
        message = 'Too many requests, please try again later';
      } else if (status >= 500) {
        type = ErrorType.ServerError;
        code = `SERVER_ERROR_${status}`;
        message = (response.data as any)?.message || 'Internal server error';
      } else {
        type = ErrorType.ClientError;
        code = `CLIENT_ERROR_${status}`;
        message = (response.data as any)?.message || 'Client error';
      }
      
      // Enhance context with response details
      context.endpoint = axiosError.config?.url;
      context.additionalData = {
        status,
        statusText: response.statusText,
        method: axiosError.config?.method?.toUpperCase(),
        responseData: response.data
      };
    } else if (request) {
      // Request was made but no response received
      if (axiosError.code === 'ECONNABORTED') {
        type = ErrorType.Timeout;
        code = 'REQUEST_TIMEOUT';
        message = 'Request timeout';
      } else if (axiosError.code === 'ERR_NETWORK') {
        type = ErrorType.Network;
        code = 'NETWORK_ERROR';
        message = 'Network connection error';
      } else {
        type = ErrorType.Network;
        code = 'REQUEST_FAILED';
        message = 'Request failed to complete';
      }
      
      context.additionalData = {
        code: axiosError.code,
        method: axiosError.config?.method?.toUpperCase()
      };
    } else {
      // Something else happened
      type = ErrorType.Unknown;
      code = 'UNKNOWN_ERROR';
      message = axiosError.message || 'Unknown error occurred';
    }

    const error = this.createError({
      type,
      code,
      message,
      context
    });

    return this.handleError(error);
  }

  // Main error handling method
  handleError(error: AppError): AppError {
    // Add to active errors
    this.state.activeErrors.set(error.id, error);
    
    // Add to history
    this.addToHistory(error);
    
    // Log error (in production, this would send to logging service)
    this.logError(error);
    
    // Handle specific error types
    this.handleSpecificErrorType(error);
    
    // Notify registered handlers
    this.notifyErrorHandlers(error);
    
    return error;
  }

  // Add error to history with size limit
  private addToHistory(error: AppError): void {
    this.state.errorHistory.unshift(error);
    
    // Limit history size
    if (this.state.errorHistory.length > this.maxHistorySize) {
      this.state.errorHistory = this.state.errorHistory.slice(0, this.maxHistorySize);
    }
  }

  // Log error for monitoring and debugging
  private logError(error: AppError): void {
    const logLevel = this.getLogLevel(error.severity);
    const logData = {
      errorId: error.id,
      type: error.type,
      code: error.code,
      message: error.message,
      context: error.context,
      timestamp: error.timestamp,
      severity: error.severity,
      sessionId: this.sessionId,
      userAgent: navigator.userAgent,
      url: window.location.href
    };

    // Log to console (in development)
    if (process.env.NODE_ENV === 'development') {
      console[logLevel]('[RepoLens Error]', logData);
    }

    // In production, send to logging service
    if (process.env.NODE_ENV === 'production') {
      this.sendToLoggingService(logData);
    }
  }

  // Get appropriate console log level
  private getLogLevel(severity: ErrorSeverity): 'log' | 'warn' | 'error' {
    switch (severity) {
      case ErrorSeverity.Low:
        return 'log';
      case ErrorSeverity.Medium:
        return 'warn';
      case ErrorSeverity.High:
      case ErrorSeverity.Critical:
        return 'error';
      default:
        return 'warn';
    }
  }

  // Send error to logging service (placeholder for production)
  private async sendToLoggingService(logData: any): Promise<void> {
    try {
      // In a real implementation, this would send to your logging service
      // For now, we'll use localStorage for demonstration
      const existingLogs = JSON.parse(localStorage.getItem('repolens_error_logs') || '[]');
      existingLogs.unshift(logData);
      
      // Keep only last 50 errors in local storage
      const limitedLogs = existingLogs.slice(0, 50);
      localStorage.setItem('repolens_error_logs', JSON.stringify(limitedLogs));
      
      console.log('Error logged to localStorage for production monitoring');
    } catch (e) {
      console.error('Failed to log error:', e);
    }
  }

  // Handle specific error types with custom logic
  private handleSpecificErrorType(error: AppError): void {
    switch (error.type) {
      case ErrorType.Authentication:
        this.handleAuthenticationError(error);
        break;
      
      case ErrorType.Network:
        this.handleNetworkError(error);
        break;
      
      case ErrorType.RateLimit:
        this.handleRateLimitError(error);
        break;
      
      default:
        // Default handling
        break;
    }
  }

  // Handle authentication errors
  private handleAuthenticationError(error: AppError): void {
    // Clear auth state
    localStorage.removeItem('repolens_token');
    localStorage.removeItem('repolens_user');
    
    // Redirect to login if not already there
    if (!window.location.pathname.includes('/login') && !window.location.pathname.includes('/register')) {
      setTimeout(() => {
        window.location.href = '/login';
      }, 2000); // Give user time to see the error message
    }
  }

  // Handle network errors
  private handleNetworkError(error: AppError): void {
    if (error.code === 'NETWORK_ERROR' || error.code === 'REQUEST_FAILED') {
      // Check if we should auto-retry
      const retryKey = `${error.context?.endpoint || 'unknown'}_${error.context?.operation || 'unknown'}`;
      const currentRetries = this.state.retryAttempts.get(retryKey) || 0;
      
      if (currentRetries < this.state.maxRetries && error.retryable) {
        this.state.retryAttempts.set(retryKey, currentRetries + 1);
        
        // Auto-retry with exponential backoff
        const delay = Math.pow(2, currentRetries) * 1000; // 1s, 2s, 4s delays
        setTimeout(() => {
          this.retryLastOperation();
        }, delay);
      }
    }
  }

  // Handle rate limit errors
  private handleRateLimitError(error: AppError): void {
    // Implement exponential backoff for rate limited requests
    const retryAfter = error.context?.additionalData?.retryAfter || 60; // Default 1 minute
    
    setTimeout(() => {
      this.dismissError(error.id);
    }, retryAfter * 1000);
  }

  // Notify registered error handlers
  private notifyErrorHandlers(error: AppError): void {
    // Notify global handler
    if (this.state.globalErrorHandler) {
      this.state.globalErrorHandler(error);
    }
    
    // Notify component-specific handler
    if (error.context?.component) {
      const componentHandler = this.state.componentErrorHandlers.get(error.context.component);
      if (componentHandler) {
        componentHandler(error);
      }
    }
  }

  // Register global error handler
  setGlobalErrorHandler(handler: (error: AppError) => void): void {
    this.state.globalErrorHandler = handler;
  }

  // Register component-specific error handler
  registerComponentErrorHandler(component: string, handler: (error: AppError) => void): void {
    this.state.componentErrorHandlers.set(component, handler);
  }

  // Unregister component error handler
  unregisterComponentErrorHandler(component: string): void {
    this.state.componentErrorHandlers.delete(component);
  }

  // Dismiss an error
  dismissError = (errorId: string): void => {
    this.state.activeErrors.delete(errorId);
  }

  // Retry last operation (placeholder - would need to be implemented per operation)
  retryLastOperation = (): void => {
    console.log('Retrying last operation...');
    // In a real implementation, this would retry the failed operation
  }

  // Get active errors
  getActiveErrors(): AppError[] {
    return Array.from(this.state.activeErrors.values());
  }

  // Get error history
  getErrorHistory(): AppError[] {
    return [...this.state.errorHistory];
  }

  // Get errors by severity
  getErrorsBySeverity(severity: ErrorSeverity): AppError[] {
    return this.getActiveErrors().filter(error => error.severity === severity);
  }

  // Get errors by type
  getErrorsByType(type: ErrorType): AppError[] {
    return this.getActiveErrors().filter(error => error.type === type);
  }

  // Clear all errors
  clearAllErrors(): void {
    this.state.activeErrors.clear();
  }

  // Clear errors by type
  clearErrorsByType(type: ErrorType): void {
    const errorsToDelete: string[] = [];
    this.state.activeErrors.forEach((error, id) => {
      if (error.type === type) {
        errorsToDelete.push(id);
      }
    });
    
    errorsToDelete.forEach(id => this.state.activeErrors.delete(id));
  }

  // Get error statistics
  getErrorStatistics(): {
    total: number;
    byType: Record<ErrorType, number>;
    bySeverity: Record<ErrorSeverity, number>;
    retryable: number;
    actionable: number;
  } {
    const activeErrors = this.getActiveErrors();
    
    const byType = {} as Record<ErrorType, number>;
    const bySeverity = {} as Record<ErrorSeverity, number>;
    
    Object.values(ErrorType).forEach(type => byType[type] = 0);
    Object.values(ErrorSeverity).forEach(severity => bySeverity[severity] = 0);
    
    let retryable = 0;
    let actionable = 0;
    
    activeErrors.forEach(error => {
      byType[error.type]++;
      bySeverity[error.severity]++;
      if (error.retryable) retryable++;
      if (error.actionable) actionable++;
    });
    
    return {
      total: activeErrors.length,
      byType,
      bySeverity,
      retryable,
      actionable
    };
  }

  // Health check method for monitoring
  getServiceHealth(): {
    status: 'healthy' | 'degraded' | 'unhealthy';
    activeErrors: number;
    criticalErrors: number;
    lastError: Date | null;
    sessionId: string;
  } {
    const activeErrors = this.getActiveErrors();
    const criticalErrors = activeErrors.filter(e => e.severity === ErrorSeverity.Critical);
    
    let status: 'healthy' | 'degraded' | 'unhealthy' = 'healthy';
    
    if (criticalErrors.length > 0) {
      status = 'unhealthy';
    } else if (activeErrors.length > 5) {
      status = 'degraded';
    }
    
    return {
      status,
      activeErrors: activeErrors.length,
      criticalErrors: criticalErrors.length,
      lastError: activeErrors.length > 0 ? activeErrors[0].timestamp : null,
      sessionId: this.sessionId
    };
  }
}

// Create singleton instance
export const errorService = new ErrorService();

// Export convenience functions
export const createError = errorService.createError.bind(errorService);
export const handleError = errorService.handleError.bind(errorService);
export const handleApiError = errorService.handleApiError.bind(errorService);
export const dismissError = errorService.dismissError.bind(errorService);
export const getActiveErrors = errorService.getActiveErrors.bind(errorService);
export const clearAllErrors = errorService.clearAllErrors.bind(errorService);

export default errorService;
