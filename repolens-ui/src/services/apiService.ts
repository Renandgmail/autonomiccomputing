import axios, { AxiosInstance, AxiosResponse } from 'axios';
import {
  ApiResponse,
  Repository,
  DashboardStats,
  RepositoryStats,
  SearchRequest,
  SearchResults,
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  User
} from '../types/api';
import ConfigService from '../config/ConfigService';

class ApiService {
  private api: AxiosInstance;
  private baseURL: string;
  private diagnosticMode: boolean = true; // Enable for development

  constructor() {
    // Use unified configuration service
    this.baseURL = ConfigService.apiUrl;
    
    this.log('🚀 ApiService initializing...', { baseURL: this.baseURL });
    
    this.api = axios.create({
      baseURL: this.baseURL,
      timeout: 120000, // Increased to 2 minutes for metrics collection
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor to add auth token and logging
    this.api.interceptors.request.use(
      (config) => {
        const token = this.getAuthToken();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        this.log('📤 API Request', {
          method: config.method?.toUpperCase(),
          url: config.url,
          hasAuth: !!token,
          data: config.data
        });
        return config;
      },
      (error) => {
        this.logError('❌ Request Error', error);
        return Promise.reject(error);
      }
    );

    // Response interceptor for error handling and logging
    this.api.interceptors.response.use(
      (response) => {
        this.log('✅ API Response', {
          status: response.status,
          url: response.config.url,
          data: response.data
        });
        return response;
      },
      (error) => {
        this.logError('❌ API Error', {
          status: error.response?.status,
          url: error.config?.url,
          message: error.message,
          response: error.response?.data
        });
        
        if (error.response?.status === 401) {
          this.log('🔒 Unauthorized response received', { 
            url: error.config?.url,
            currentPath: window.location.pathname 
          });
          
          // Only redirect if we're not already on login page and not during initial load
          if (!window.location.pathname.includes('/login') && 
              !window.location.pathname.includes('/register')) {
            this.log('🔒 Clearing auth and redirecting to login');
            this.clearAuth();
            // Use React Router navigation instead of hard redirect when possible
            if (window.history && window.history.pushState) {
              window.history.pushState({}, '', '/login');
              window.dispatchEvent(new PopStateEvent('popstate'));
            } else {
              window.location.href = '/login';
            }
          } else {
            this.log('🔒 Already on auth page, clearing auth only');
            this.clearAuth();
          }
        }
        return Promise.reject(error);
      }
    );
  }

  // Diagnostic logging methods
  private log(message: string, data?: any): void {
    if (this.diagnosticMode) {
      console.log(`[RepoLens API] ${message}`, data || '');
    }
  }

  private logError(message: string, error?: any): void {
    if (this.diagnosticMode) {
      console.error(`[RepoLens API] ${message}`, error || '');
    }
  }

  // Authentication methods
  private getAuthToken(): string | null {
    return localStorage.getItem('repolens_token');
  }

  private setAuthToken(token: string): void {
    localStorage.setItem('repolens_token', token);
  }

  private clearAuth(): void {
    localStorage.removeItem('repolens_token');
    localStorage.removeItem('repolens_user');
  }

  // Token validation method - Production grade with fallback strategies
  private validateToken(): boolean {
    const token = this.getAuthToken();
    if (!token) {
      this.log('🔒 No token found');
      return false;
    }
    
    try {
      // Check if token looks like a JWT (has 3 parts separated by dots)
      const parts = token.split('.');
      if (parts.length === 3) {
        // Try to decode JWT payload
        try {
          const payload = JSON.parse(atob(parts[1]));
          const now = Date.now() / 1000;
          
          // Check expiration if present
          if (payload.exp && payload.exp < now) {
            this.log('🔒 JWT token expired', { exp: payload.exp, now });
            this.clearAuth();
            return false;
          }
          
          this.log('✅ JWT token validated successfully', { 
            exp: payload.exp ? new Date(payload.exp * 1000) : 'no expiration',
            user: payload.sub || payload.email || 'unknown'
          });
          return true;
        } catch (jwtError) {
          this.log('⚠️ JWT parsing failed, treating as opaque token', { error: jwtError });
          // Fallback: treat as opaque token, validate by presence only
          return true;
        }
      } else {
        // Not a JWT format, treat as opaque token
        this.log('✅ Opaque token validated by presence', { tokenLength: token.length });
        return true;
      }
    } catch (error) {
      this.logError('❌ Token validation error', error);
      // In production, be conservative but don't immediately clear auth
      // Could be a temporary parsing issue
      this.log('⚠️ Token validation failed but keeping auth for this session');
      return true; // Be permissive for now, let backend handle validation
    }
  }

  // Helper method to handle API responses
  private handleResponse<T>(response: AxiosResponse<ApiResponse<T>>): T {
    if (response.data.success && response.data.data !== undefined) {
      return response.data.data;
    }
    throw new Error(response.data.errorMessage || 'API request failed');
  }

  // Auth API methods
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    try {
      const response = await this.api.post<AuthResponse>('/api/auth/login', credentials);
      if (response.data.success && response.data.token) {
        this.setAuthToken(response.data.token);
        if (response.data.user) {
          localStorage.setItem('repolens_user', JSON.stringify(response.data.user));
        }
      }
      return response.data;
    } catch (error: any) {
      return {
        success: false,
        errorMessage: error.response?.data?.errorMessage || 'Login failed'
      };
    }
  }

  async register(userData: RegisterRequest): Promise<AuthResponse> {
    try {
      this.log('📝 Registration attempt', { email: userData.email });
      
      const response = await this.api.post<AuthResponse>('/api/auth/register', userData);
      
      this.log('✅ Registration response received', {
        success: response.data.success,
        hasToken: !!response.data.token,
        hasUser: !!response.data.user
      });
      
      if (response.data.success && response.data.token) {
        this.log('🎉 Registration successful, storing auth data');
        this.setAuthToken(response.data.token);
        if (response.data.user) {
          localStorage.setItem('repolens_user', JSON.stringify(response.data.user));
        }
      }
      return response.data;
    } catch (error: any) {
      this.logError('❌ Registration failed', {
        error: error.message,
        response: error.response?.data,
        status: error.response?.status,
        userData: { email: userData.email } // Don't log password
      });
      
      return {
        success: false,
        errorMessage: error.response?.data?.errorMessage || error.message || 'Registration failed'
      };
    }
  }

  async logout(): Promise<void> {
    try {
      await this.api.post('/api/auth/logout');
    } catch (error) {
      console.warn('Logout request failed, clearing local auth anyway');
    } finally {
      this.clearAuth();
    }
  }

  getCurrentUser(): User | null {
    const userStr = localStorage.getItem('repolens_user');
    return userStr ? JSON.parse(userStr) : null;
  }

  isAuthenticated(): boolean {
    return this.validateToken();
  }

  // Dashboard API methods
  async getDashboardStats(): Promise<DashboardStats> {
    const response = await this.api.get<ApiResponse<DashboardStats>>('/api/dashboard/stats');
    return this.handleResponse(response);
  }

  // Repository API methods
  async getRepositories(): Promise<Repository[]> {
    const response = await this.api.get<ApiResponse<Repository[]>>('/api/repositories');
    return this.handleResponse(response);
  }

  async getRepository(id: number): Promise<Repository> {
    const response = await this.api.get<ApiResponse<Repository>>(`/api/repositories/${id}`);
    return this.handleResponse(response);
  }

  async addRepository(url: string, name?: string): Promise<Repository> {
    const response = await this.api.post<ApiResponse<Repository>>('/api/repositories', {
      url,
      name
    });
    return this.handleResponse(response);
  }

  async updateRepository(id: number, data: Partial<Repository>): Promise<Repository> {
    const response = await this.api.put<ApiResponse<Repository>>(`/api/repositories/${id}`, data);
    return this.handleResponse(response);
  }

  async deleteRepository(id: number): Promise<void> {
    const response = await this.api.delete<ApiResponse<void>>(`/api/repositories/${id}`);
    this.handleResponse(response);
  }

  async syncRepository(id: number): Promise<boolean> {
    const response = await this.api.post<ApiResponse<boolean>>(`/api/repositories/${id}/sync`);
    return this.handleResponse(response);
  }

  async getRepositoryStats(id: number): Promise<RepositoryStats> {
    const response = await this.api.get<ApiResponse<RepositoryStats>>(`/api/repositories/${id}/stats`);
    return this.handleResponse(response);
  }

  // Search API methods
  async search(searchRequest: SearchRequest): Promise<SearchResults> {
    const response = await this.api.post<ApiResponse<SearchResults>>('/api/search', searchRequest);
    return this.handleResponse(response);
  }

  async getFileContent(artifactId: number): Promise<string> {
    const response = await this.api.get<ApiResponse<string>>(`/api/files/${artifactId}`);
    return this.handleResponse(response);
  }

  // System Health API methods
  async getSystemHealth(): Promise<any> {
    const response = await this.api.get<ApiResponse<any>>('/api/system/health');
    return this.handleResponse(response);
  }

  // Analytics API methods
  async getAnalyticsSummary(): Promise<any> {
    const response = await this.api.get<ApiResponse<any>>('/api/analytics/summary');
    return this.handleResponse(response);
  }
}

// Create singleton instance
const apiService = new ApiService();

export default apiService;

// Export individual methods for easier imports
export const {
  login,
  register,
  logout,
  getCurrentUser,
  isAuthenticated,
  getDashboardStats,
  getRepositories,
  getRepository,
  addRepository,
  updateRepository,
  deleteRepository,
  syncRepository,
  getRepositoryStats,
  search,
  getFileContent,
  getSystemHealth,
  getAnalyticsSummary
} = apiService;
