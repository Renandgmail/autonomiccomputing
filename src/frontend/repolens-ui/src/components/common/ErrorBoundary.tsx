import React, { Component, ErrorInfo, ReactNode } from 'react';
import { Box, Typography, Button, Alert, AlertTitle, Collapse, IconButton } from '@mui/material';
import { ExpandMore, ExpandLess, Refresh, Home, BugReport } from '@mui/icons-material';
import { errorService, ErrorType, ErrorSeverity, AppError } from '../../services/errorService';

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
  onError?: (error: AppError) => void;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error?: AppError;
  errorInfo?: ErrorInfo;
  showDetails: boolean;
}

class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = {
      hasError: false,
      showDetails: false
    };
  }

  static getDerivedStateFromError(error: Error): Partial<ErrorBoundaryState> {
    // Create structured error from caught error
    const appError = errorService.createError({
      type: ErrorType.ClientError,
      code: 'REACT_COMPONENT_ERROR',
      message: error.message || 'React component error',
      context: {
        component: 'ErrorBoundary',
        operation: 'component_render',
        additionalData: {
          stack: error.stack,
          name: error.name
        }
      },
      severity: ErrorSeverity.High
    });

    return {
      hasError: true,
      error: appError
    };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    // Handle the error through our error service
    const appError = errorService.createError({
      type: ErrorType.ClientError,
      code: 'REACT_COMPONENT_ERROR',
      message: error.message || 'React component error',
      context: {
        component: errorInfo.componentStack?.split('\n')[1]?.trim() || 'Unknown',
        operation: 'component_render',
        additionalData: {
          stack: error.stack,
          componentStack: errorInfo.componentStack,
          name: error.name
        }
      },
      severity: ErrorSeverity.High
    });

    // Process through error service
    errorService.handleError(appError);

    // Store error info for display
    this.setState({
      error: appError,
      errorInfo
    });

    // Notify parent component if callback provided
    if (this.props.onError) {
      this.props.onError(appError);
    }
  }

  handleRetry = (): void => {
    this.setState({
      hasError: false,
      error: undefined,
      errorInfo: undefined,
      showDetails: false
    });
  };

  handleReportIssue = (): void => {
    if (this.state.error) {
      // In a real implementation, this would open a support ticket
      // For now, we'll copy error details to clipboard
      const errorDetails = {
        errorId: this.state.error.id,
        message: this.state.error.message,
        timestamp: this.state.error.timestamp,
        url: window.location.href,
        userAgent: navigator.userAgent,
        stack: this.state.error.context?.additionalData?.stack
      };

      navigator.clipboard.writeText(JSON.stringify(errorDetails, null, 2))
        .then(() => {
          alert('Error details copied to clipboard. Please send this to support.');
        })
        .catch(() => {
          alert(`Error details:\n\n${JSON.stringify(errorDetails, null, 2)}`);
        });
    }
  };

  handleGoHome = (): void => {
    window.location.href = '/';
  };

  toggleDetails = (): void => {
    this.setState(prevState => ({
      showDetails: !prevState.showDetails
    }));
  };

  render(): ReactNode {
    if (this.state.hasError && this.state.error) {
      // Use custom fallback if provided
      if (this.props.fallback) {
        return this.props.fallback;
      }

      // Default error UI
      return (
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            minHeight: '50vh',
            padding: 3,
            textAlign: 'center'
          }}
        >
          <Alert 
            severity="error" 
            sx={{ 
              width: '100%', 
              maxWidth: 600,
              marginBottom: 2 
            }}
          >
            <AlertTitle>Something went wrong</AlertTitle>
            {this.state.error.userMessage}
          </Alert>

          <Box sx={{ display: 'flex', gap: 2, marginBottom: 2 }}>
            <Button
              variant="contained"
              color="primary"
              startIcon={<Refresh />}
              onClick={this.handleRetry}
            >
              Try Again
            </Button>
            
            <Button
              variant="outlined"
              startIcon={<Home />}
              onClick={this.handleGoHome}
            >
              Go to Dashboard
            </Button>
            
            <Button
              variant="outlined"
              color="warning"
              startIcon={<BugReport />}
              onClick={this.handleReportIssue}
            >
              Report Issue
            </Button>
          </Box>

          {/* Error details toggle */}
          <Box sx={{ width: '100%', maxWidth: 600 }}>
            <Button
              variant="text"
              size="small"
              onClick={this.toggleDetails}
              endIcon={this.state.showDetails ? <ExpandLess /> : <ExpandMore />}
              sx={{ marginBottom: 1 }}
            >
              {this.state.showDetails ? 'Hide' : 'Show'} Technical Details
            </Button>

            <Collapse in={this.state.showDetails}>
              <Alert 
                severity="info" 
                sx={{ textAlign: 'left' }}
              >
                <Typography variant="subtitle2" gutterBottom>
                  Error Information
                </Typography>
                <Typography variant="body2" component="div">
                  <strong>Error ID:</strong> {this.state.error.id}<br />
                  <strong>Type:</strong> {this.state.error.type}<br />
                  <strong>Code:</strong> {this.state.error.code}<br />
                  <strong>Time:</strong> {this.state.error.timestamp.toLocaleString()}<br />
                  <strong>Component:</strong> {this.state.error.context?.component || 'Unknown'}<br />
                  
                  {this.state.error.context?.additionalData?.stack && (
                    <>
                      <br />
                      <strong>Stack Trace:</strong>
                      <pre style={{ 
                        fontSize: '0.75rem', 
                        marginTop: '8px',
                        overflow: 'auto',
                        maxHeight: '200px',
                        background: 'rgba(0,0,0,0.05)',
                        padding: '8px',
                        borderRadius: '4px'
                      }}>
                        {this.state.error.context.additionalData.stack}
                      </pre>
                    </>
                  )}
                </Typography>
              </Alert>
            </Collapse>
          </Box>

          <Typography 
            variant="caption" 
            color="textSecondary"
            sx={{ marginTop: 2 }}
          >
            Error ID: {this.state.error.id} | Session: {this.state.error.context?.sessionId?.slice(-8)}
          </Typography>
        </Box>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
