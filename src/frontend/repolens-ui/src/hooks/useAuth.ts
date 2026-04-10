/**
 * useAuth Hook
 * Wrapper around existing apiService authentication functionality
 * Provides React-friendly interface for authentication state
 */

import { useState, useEffect } from 'react';
import apiService from '../services/apiService';

interface User {
  firstName?: string;
  lastName?: string;
  email?: string;
  name?: string;
  avatarUrl?: string;
}

interface UseAuthReturn {
  user: User | null;
  loading: boolean;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
}

export const useAuth = (): UseAuthReturn => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  // Initialize user state from apiService
  useEffect(() => {
    const initializeAuth = () => {
      try {
        const isAuth = apiService.isAuthenticated();
        if (isAuth) {
          const currentUser = apiService.getCurrentUser();
          if (currentUser) {
            // Adapt to our interface
            setUser({
              firstName: currentUser.firstName,
              lastName: currentUser.lastName,
              email: currentUser.email,
              name: `${currentUser.firstName || ''} ${currentUser.lastName || ''}`.trim() || currentUser.email,
            });
          }
        }
      } catch (error) {
        console.error('Failed to initialize auth state:', error);
      } finally {
        setLoading(false);
      }
    };

    initializeAuth();
  }, []);

  const login = async (email: string, password: string): Promise<void> => {
    setLoading(true);
    try {
      await apiService.login({ email, password });
      const currentUser = apiService.getCurrentUser();
      if (currentUser) {
        setUser({
          firstName: currentUser.firstName,
          lastName: currentUser.lastName,
          email: currentUser.email,
          name: `${currentUser.firstName || ''} ${currentUser.lastName || ''}`.trim() || currentUser.email,
        });
      }
    } finally {
      setLoading(false);
    }
  };

  const logout = async (): Promise<void> => {
    setLoading(true);
    try {
      await apiService.logout();
      setUser(null);
    } finally {
      setLoading(false);
    }
  };

  return {
    user,
    loading,
    isAuthenticated: apiService.isAuthenticated() && !!user,
    login,
    logout
  };
};

export default useAuth;
