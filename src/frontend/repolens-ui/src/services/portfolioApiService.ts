/**
 * Portfolio API Service for L1 Dashboard
 * Provides typed API integration for Engineering Manager focused endpoints
 */

import axios from 'axios';
import ConfigService from '../config/ConfigService';

// Type definitions matching backend models
export interface TrendIndicator {
  direction: 'up' | 'down' | 'flat';
  delta: string;
  context: string;
  positiveDirection: 'up' | 'down' | 'flat';
}

export interface PortfolioSummary {
  totalRepositories: number;
  averageHealthScore: number;
  healthScoreTrend: TrendIndicator;
  criticalIssuesCount: number;
  activeTeamsCount: number;
  lastCalculated: string;
}

export enum RepositoryHealthBand {
  Critical = 1,
  Poor = 2,
  Fair = 3,
  Good = 4,
  Excellent = 5
}

export enum CriticalIssueType {
  Security = 'Security',
  TechnicalDebt = 'TechnicalDebt',
  TestCoverage = 'TestCoverage',
  HealthCritical = 'HealthCritical',
  Stale = 'Stale',
  Performance = 'Performance',
  Compliance = 'Compliance'
}

export enum IssueSeverity {
  Info = 0,
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4
}

export interface RepositoryIssues {
  critical: number;
  high: number;
  medium: number;
  low: number;
  total: number;
  hasCritical: boolean;
}

export interface PortfolioRepository {
  id: number;
  name: string;
  url: string;
  healthScore: number;
  healthBand: RepositoryHealthBand;
  healthTrend: TrendIndicator;
  primaryLanguage: string;
  issues: RepositoryIssues;
  lastSync: string;
  isStarred: boolean;
  teamName: string;
  hasCriticalIssues: boolean;
}

export interface CriticalIssue {
  id: string;
  repositoryId: number;
  repositoryName: string;
  description: string;
  type: CriticalIssueType;
  severity: IssueSeverity;
  detectedAt: string;
  navigationRoute: string;
}

export interface PortfolioFilters {
  healthBands: RepositoryHealthBand[];
  languages: string[];
  teams: string[];
  hasCriticalIssuesOnly: boolean;
  starredOnly: boolean;
}

export interface PortfolioFilterOptions {
  languages: string[];
  teams: string[];
  healthBandCounts: Record<RepositoryHealthBand, number>;
}

export interface PortfolioRepositoryListResponse {
  repositories: PortfolioRepository[];
  totalCount: number;
  filteredCount: number;
  filterOptions: PortfolioFilterOptions;
}

export interface StarRepositoryRequest {
  repositoryId: number;
  isStarred: boolean;
}

/**
 * Portfolio API Service for L1 Dashboard Engineering Manager endpoints
 */
export class PortfolioApiService {
  private readonly baseUrl = '/api/portfolio';
  private readonly api = axios.create({
    baseURL: ConfigService.apiUrl,
    timeout: 30000,
    headers: {
      'Content-Type': 'application/json',
    },
  });

  constructor() {
    // Add request interceptor for auth token
    this.api.interceptors.request.use((config) => {
      const token = localStorage.getItem('repolens_token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    // Add response interceptor for error handling
    this.api.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          // Clear auth on unauthorized
          localStorage.removeItem('repolens_token');
          localStorage.removeItem('repolens_user');
          // Redirect to login if needed
          if (!window.location.pathname.includes('/login')) {
            window.location.href = '/login';
          }
        }
        return Promise.reject(error);
      }
    );
  }

  /**
   * Get Zone 1 summary metrics for Engineering Manager portfolio overview
   * Returns exactly 4 metrics: repositories, avg health, critical issues, teams
   */
  async getPortfolioSummary(): Promise<PortfolioSummary> {
    try {
      const response = await this.api.get(`${this.baseUrl}/summary`);
      return response.data;
    } catch (error) {
      console.error('Failed to get portfolio summary:', error);
      throw new Error('Failed to load portfolio summary. Please try again.');
    }
  }

  /**
   * Get Zone 2 repository list with filtering and sorting for Engineering Manager decision making
   * Default sort: starred repositories first, then health score ascending (worst first)
   */
  async getRepositoryList(filters: Partial<PortfolioFilters> = {}): Promise<PortfolioRepositoryListResponse> {
    try {
      const params = new URLSearchParams();
      
      // Add health bands filter
      if (filters.healthBands?.length) {
        filters.healthBands.forEach(band => {
          params.append('healthBands', band.toString());
        });
      }
      
      // Add languages filter
      if (filters.languages?.length) {
        filters.languages.forEach(lang => {
          params.append('languages', lang);
        });
      }
      
      // Add teams filter
      if (filters.teams?.length) {
        filters.teams.forEach(team => {
          params.append('teams', team);
        });
      }
      
      // Add boolean filters
      if (filters.hasCriticalIssuesOnly) {
        params.append('hasCriticalIssuesOnly', 'true');
      }
      
      if (filters.starredOnly) {
        params.append('starredOnly', 'true');
      }

      const queryString = params.toString();
      const url = queryString ? `${this.baseUrl}/repositories?${queryString}` : `${this.baseUrl}/repositories`;
      
      const response = await this.api.get(url);
      return response.data;
    } catch (error) {
      console.error('Failed to get repository list:', error);
      throw new Error('Failed to load repositories. Please try again.');
    }
  }

  /**
   * Get Zone 3 critical issues requiring immediate Engineering Manager attention
   * Conditional display: only shown when >= 1 repository has critical issues
   * Maximum 5 items returned before "See all X critical issues" link
   */
  async getCriticalIssues(): Promise<CriticalIssue[]> {
    try {
      const response = await this.api.get(`${this.baseUrl}/critical-issues`);
      return response.data;
    } catch (error) {
      console.error('Failed to get critical issues:', error);
      throw new Error('Failed to load critical issues. Please try again.');
    }
  }

  /**
   * Toggle repository star status for current user
   * Starred repositories appear at the top of the repository list
   */
  async toggleRepositoryStar(repositoryId: number, isStarred: boolean): Promise<boolean> {
    try {
      const request: StarRepositoryRequest = {
        repositoryId,
        isStarred
      };

      const response = await this.api.post(`${this.baseUrl}/repositories/${repositoryId}/star`, request);
      return response.data;
    } catch (error) {
      console.error('Failed to toggle repository star:', error);
      throw new Error('Failed to update repository favorite status. Please try again.');
    }
  }

  /**
   * Remove star from repository (alternative endpoint)
   */
  async unstarRepository(repositoryId: number): Promise<boolean> {
    try {
      const response = await this.api.delete(`${this.baseUrl}/repositories/${repositoryId}/star`);
      return response.data;
    } catch (error) {
      console.error('Failed to unstar repository:', error);
      throw new Error('Failed to remove favorite. Please try again.');
    }
  }

  /**
   * Get available filter options for the repository list
   * Used to populate filter dropdowns in the UI
   */
  async getFilterOptions(): Promise<PortfolioFilterOptions> {
    try {
      const response = await this.api.get(`${this.baseUrl}/filter-options`);
      return response.data;
    } catch (error) {
      console.error('Failed to get filter options:', error);
      throw new Error('Failed to load filter options. Please try again.');
    }
  }

  /**
   * Check portfolio service health
   */
  async checkHealth(): Promise<{ service: string; status: string; timestamp: string; version: string }> {
    try {
      const response = await this.api.get(`${this.baseUrl}/health`);
      return response.data;
    } catch (error) {
      console.error('Portfolio service health check failed:', error);
      throw new Error('Portfolio service is not available.');
    }
  }
}

// Export singleton instance
export const portfolioApiService = new PortfolioApiService();
export default portfolioApiService;

// Health Band Color Mapping (from requirements)
export const HEALTH_BAND_COLORS = {
  [RepositoryHealthBand.Excellent]: '#16A34A', // Green - No action required
  [RepositoryHealthBand.Good]: '#0D9488',      // Teal - Monitor, low priority
  [RepositoryHealthBand.Fair]: '#D97706',      // Amber - Plan improvement sprint
  [RepositoryHealthBand.Poor]: '#EA580C',      // Orange - Immediate attention needed
  [RepositoryHealthBand.Critical]: '#DC2626'   // Red - Escalate to leadership
} as const;

// Health Band Labels
export const HEALTH_BAND_LABELS = {
  [RepositoryHealthBand.Excellent]: 'Excellent',
  [RepositoryHealthBand.Good]: 'Good',
  [RepositoryHealthBand.Fair]: 'Fair',
  [RepositoryHealthBand.Poor]: 'Poor',
  [RepositoryHealthBand.Critical]: 'Critical'
} as const;

// Health Band Descriptions
export const HEALTH_BAND_DESCRIPTIONS = {
  [RepositoryHealthBand.Excellent]: 'No action required',
  [RepositoryHealthBand.Good]: 'Monitor, low priority',
  [RepositoryHealthBand.Fair]: 'Plan improvement sprint',
  [RepositoryHealthBand.Poor]: 'Immediate attention needed',
  [RepositoryHealthBand.Critical]: 'Escalate to leadership'
} as const;

// Critical Issue Type Labels
export const CRITICAL_ISSUE_TYPE_LABELS = {
  [CriticalIssueType.Security]: 'Security',
  [CriticalIssueType.TechnicalDebt]: 'Technical Debt',
  [CriticalIssueType.TestCoverage]: 'Test Coverage',
  [CriticalIssueType.HealthCritical]: 'Health Critical',
  [CriticalIssueType.Stale]: 'Stale Repository',
  [CriticalIssueType.Performance]: 'Performance',
  [CriticalIssueType.Compliance]: 'Compliance'
} as const;

// Utility functions
export const getHealthBandColor = (healthScore: number): string => {
  if (healthScore >= 90) return HEALTH_BAND_COLORS[RepositoryHealthBand.Excellent];
  if (healthScore >= 70) return HEALTH_BAND_COLORS[RepositoryHealthBand.Good];
  if (healthScore >= 50) return HEALTH_BAND_COLORS[RepositoryHealthBand.Fair];
  if (healthScore >= 30) return HEALTH_BAND_COLORS[RepositoryHealthBand.Poor];
  return HEALTH_BAND_COLORS[RepositoryHealthBand.Critical];
};

export const getHealthBandFromScore = (healthScore: number): RepositoryHealthBand => {
  if (healthScore >= 90) return RepositoryHealthBand.Excellent;
  if (healthScore >= 70) return RepositoryHealthBand.Good;
  if (healthScore >= 50) return RepositoryHealthBand.Fair;
  if (healthScore >= 30) return RepositoryHealthBand.Poor;
  return RepositoryHealthBand.Critical;
};

export const formatRelativeTime = (dateString: string): string => {
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  
  const diffMinutes = Math.floor(diffMs / (1000 * 60));
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
  
  if (diffMinutes < 1) return 'Just now';
  if (diffMinutes < 60) return `${diffMinutes}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 30) return `${diffDays}d ago`;
  
  return date.toLocaleDateString();
};
