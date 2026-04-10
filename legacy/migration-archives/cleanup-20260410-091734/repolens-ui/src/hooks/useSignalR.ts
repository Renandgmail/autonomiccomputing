import { useEffect, useState, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import ConfigService from '../config/ConfigService';

interface SignalRHookOptions {
  hubUrl: string;
  autoConnect?: boolean;
  reconnectAttempts?: number;
  reconnectInterval?: number;
}

interface ConnectionState {
  isConnected: boolean;
  isConnecting: boolean;
  error: string | null;
  connectionId: string | null;
}

interface UseSignalRReturn {
  connectionState: ConnectionState;
  connection: signalR.HubConnection | null;
  connect: () => Promise<void>;
  disconnect: () => Promise<void>;
  invoke: (methodName: string, ...args: any[]) => Promise<any>;
  on: (methodName: string, callback: (...args: any[]) => void) => void;
  off: (methodName: string, callback?: (...args: any[]) => void) => void;
  send: (methodName: string, ...args: any[]) => Promise<void>;
}

export const useSignalR = (options: SignalRHookOptions): UseSignalRReturn => {
  const {
    hubUrl,
    autoConnect = true,
    reconnectAttempts = 5,
    reconnectInterval = 5000
  } = options;

  const [connectionState, setConnectionState] = useState<ConnectionState>({
    isConnected: false,
    isConnecting: false,
    error: null,
    connectionId: null
  });

  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const reconnectTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const reconnectCountRef = useRef(0);
  const mountedRef = useRef(true);

  const updateConnectionState = useCallback((updates: Partial<ConnectionState>) => {
    if (mountedRef.current) {
      setConnectionState(prev => ({ ...prev, ...updates }));
    }
  }, []);

  const buildConnection = useCallback(() => {
    const fullUrl = `${ConfigService.apiUrl}${hubUrl}`;
    
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(fullUrl, {
        accessTokenFactory: () => {
          const token = localStorage.getItem('repolens_token');
          return token || '';
        },
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
        skipNegotiation: false
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff with jitter
          const baseDelay = Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
          const jitter = Math.random() * 1000;
          return baseDelay + jitter;
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Connection event handlers
    connection.onreconnecting(() => {
      console.log('[SignalR] Connection lost, attempting to reconnect...');
      updateConnectionState({
        isConnected: false,
        isConnecting: true,
        error: null
      });
    });

    connection.onreconnected((connectionId) => {
      console.log('[SignalR] Successfully reconnected:', connectionId);
      updateConnectionState({
        isConnected: true,
        isConnecting: false,
        error: null,
        connectionId
      });
      reconnectCountRef.current = 0;
    });

    connection.onclose((error) => {
      console.log('[SignalR] Connection closed:', error?.message || 'Unknown reason');
      updateConnectionState({
        isConnected: false,
        isConnecting: false,
        error: error?.message || 'Connection closed',
        connectionId: null
      });

      // Attempt manual reconnection if auto-reconnect fails
      if (error && reconnectCountRef.current < reconnectAttempts) {
        reconnectCountRef.current++;
        console.log(`[SignalR] Attempting manual reconnection ${reconnectCountRef.current}/${reconnectAttempts}`);
        
        if (reconnectTimeoutRef.current) {
          clearTimeout(reconnectTimeoutRef.current);
        }
        
        reconnectTimeoutRef.current = setTimeout(() => {
          if (mountedRef.current && connectionRef.current) {
            connect();
          }
        }, reconnectInterval);
      }
    });

    return connection;
  }, [hubUrl, reconnectAttempts, reconnectInterval, updateConnectionState]);

  const connect = useCallback(async () => {
    if (!mountedRef.current) return;

    try {
      updateConnectionState({ isConnecting: true, error: null });

      if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
        console.log('[SignalR] Already connected');
        updateConnectionState({
          isConnected: true,
          isConnecting: false,
          connectionId: connectionRef.current.connectionId
        });
        return;
      }

      if (!connectionRef.current) {
        connectionRef.current = buildConnection();
      }

      await connectionRef.current.start();
      
      console.log('[SignalR] Connected successfully:', connectionRef.current.connectionId);
      updateConnectionState({
        isConnected: true,
        isConnecting: false,
        error: null,
        connectionId: connectionRef.current.connectionId
      });
      
      reconnectCountRef.current = 0;
    } catch (error: any) {
      console.error('[SignalR] Connection failed:', error);
      updateConnectionState({
        isConnected: false,
        isConnecting: false,
        error: error.message || 'Connection failed'
      });
    }
  }, [buildConnection, updateConnectionState]);

  const disconnect = useCallback(async () => {
    if (reconnectTimeoutRef.current) {
      clearTimeout(reconnectTimeoutRef.current);
      reconnectTimeoutRef.current = null;
    }

    if (connectionRef.current) {
      try {
        await connectionRef.current.stop();
        console.log('[SignalR] Disconnected successfully');
      } catch (error: any) {
        console.error('[SignalR] Error during disconnect:', error);
      } finally {
        connectionRef.current = null;
        updateConnectionState({
          isConnected: false,
          isConnecting: false,
          error: null,
          connectionId: null
        });
      }
    }
  }, [updateConnectionState]);

  const invoke = useCallback(async (methodName: string, ...args: any[]) => {
    if (!connectionRef.current || connectionRef.current.state !== signalR.HubConnectionState.Connected) {
      throw new Error('SignalR connection is not established');
    }
    
    try {
      return await connectionRef.current.invoke(methodName, ...args);
    } catch (error: any) {
      console.error(`[SignalR] Failed to invoke ${methodName}:`, error);
      throw error;
    }
  }, []);

  const send = useCallback(async (methodName: string, ...args: any[]) => {
    if (!connectionRef.current || connectionRef.current.state !== signalR.HubConnectionState.Connected) {
      throw new Error('SignalR connection is not established');
    }
    
    try {
      await connectionRef.current.send(methodName, ...args);
    } catch (error: any) {
      console.error(`[SignalR] Failed to send ${methodName}:`, error);
      throw error;
    }
  }, []);

  const on = useCallback((methodName: string, callback: (...args: any[]) => void) => {
    if (connectionRef.current) {
      connectionRef.current.on(methodName, callback);
    }
  }, []);

  const off = useCallback((methodName: string, callback?: (...args: any[]) => void) => {
    if (connectionRef.current) {
      if (callback) {
        connectionRef.current.off(methodName, callback);
      } else {
        connectionRef.current.off(methodName);
      }
    }
  }, []);

  // Auto-connect on mount
  useEffect(() => {
    if (autoConnect) {
      connect();
    }

    // Cleanup on unmount
    return () => {
      mountedRef.current = false;
      disconnect();
    };
  }, [autoConnect, connect, disconnect]);

  // Cleanup timeouts on unmount
  useEffect(() => {
    return () => {
      if (reconnectTimeoutRef.current) {
        clearTimeout(reconnectTimeoutRef.current);
      }
    };
  }, []);

  return {
    connectionState,
    connection: connectionRef.current,
    connect,
    disconnect,
    invoke,
    on,
    off,
    send
  };
};

// Convenience hook for metrics updates
export const useMetricsSignalR = () => {
  const signalR = useSignalR({
    hubUrl: '/hubs/metrics',
    autoConnect: true,
    reconnectAttempts: 5,
    reconnectInterval: 3000
  });

  const subscribeToRepository = useCallback((repositoryId: number) => {
    if (signalR.connectionState.isConnected) {
      signalR.invoke('JoinRepositoryGroup', repositoryId.toString())
        .catch(error => console.error('Failed to join repository group:', error));
    }
  }, [signalR]);

  const unsubscribeFromRepository = useCallback((repositoryId: number) => {
    if (signalR.connectionState.isConnected) {
      signalR.invoke('LeaveRepositoryGroup', repositoryId.toString())
        .catch(error => console.error('Failed to leave repository group:', error));
    }
  }, [signalR]);

  return {
    ...signalR,
    subscribeToRepository,
    unsubscribeFromRepository
  };
};

export default useSignalR;
