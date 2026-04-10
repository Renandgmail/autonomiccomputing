/**
 * Repository API Service for L2 Repository Dashboard
 * Provides typed API integration for repository-specific insights
 * Focuses on 60-second decision time target for Engineering Managers
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

export interface RepositorySummary {
  healthScore: number;
  healthTrend: TrendIndicator;
  activeContributors: number;
  criticalIssues: number;
  technicalDebtHours: number;
  lastCalculated: string;
}

export enum HotspotSeverity {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4
}

export enum HotspotIssueType {
  Complexity = 'Complexity',
  Security = 'Security',
  TechnicalDebt = 'TechnicalDebt',
  TestCoverage = 'TestCoverage',
  Performance = 'Performance',
  Maintainability = 'Maintainability'
}

export interface QualityHotspot {
  fileId: number;
  filePath: string;
  repositoryId: number;
  severity: HotspotSeverity;
  issueType: HotspotIssueType;
  estimatedFixHours: number;
  hotspotScore: number;
  detectedAt: string;
  navigationRoute: string;
}

export interface QualityHotspotsResponse {
  hotspots: QualityHotspot[];
  totalCount: number;
  shownCount: number;
  hasMore: boolean;
}

export enum RepositoryActivityType {
  QualityGatePass = 'QualityGatePass',
  QualityGateFail = 'QualityGateFail',
  NewCriticalIssue = 'NewCriticalIssue',
  SecurityVulnerabilityDetected = 'SecurityVulnerabilityDetected',
  ComplexitySpike = 'ComplexitySpike',
  BuildSuccess = 'BuildSuccess',
  BuildFailure = 'BuildFailure',
  SyncComplete = 'SyncComplete',
  AnalysisComplete = 'AnalysisComplete'
}

export enum ActivitySeverity {
  Info = 0,
  Success = 1,
  Warning = 2,
  Error = 3,
  Critical = 4
}

export interface RepositoryActivity {
  id: string;
  type: RepositoryActivityType;
  description: string;
  timestamp: string;
  severity: ActivitySeverity;
  navigationRoute?: string;
  metadata: Record<string, any>;
}

export interface RepositoryActivityResponse {
  activities: RepositoryActivity[];
  totalCount: number;
  lastUpdated: string;
}

export interface QuickActionRoute {
  name: string;
  route: string;
  icon: string;
  description: string;
  isEnabled: boolean;
}

export interface RepositoryQuickActions {
  repositoryId: number;
  actions: QuickActionRoute[];
}

export interface RepositoryContext {
  repositoryId: number;
  name: string;
  url: string;
  healthScore: number;
  healthTrend: TrendIndicator;
  lastSync: string;
  syncStatus: 'synced' | 'syncing' | 'failed';
  breadcrumbPath: string;
}

/**
 * Repository API Service for L2 Repository Dashboard endpoints
 */
export class RepositoryApiService {
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
          localStorage.removeItem('repolens_token');
          localStorage.removeItem('repolens_user');
          if (!window.location.pathname.includes('/login')) {
            window.location.href = '/login';
          }
        }
        return Promise.reject(error);
      }
    );
  }

  /**
   * Zone 1: Get repository summary with 4 exact metrics
   * Health Score | Active Contributors | Critical Issues | Technical Debt Hours
   */
  async getRepositorySummary(repositoryId: number): Promise<RepositorySummary> {
    try {
      const response = await this.api.get(`/api/repositories/${repositoryId}/summary`);
      return response.data;
    } catch (error) {
      console.error('Failed to get repository summary:', error);
      throw new Error('Failed to load repository summary. Please try again.');
    }
  }

  /**
   * Zone 2: Get quality hotspots ranked by composite score (worst first)
   */
  async getQualityHotspots(
    repositoryId: number,
    options: {
      limit?: number;
      minSeverity?: HotspotSeverity;
      issueType?: HotspotIssueType;
    } = {}
  ): Promise<QualityHotspotsResponse> {
    try {
      const params = new URLSearchParams();
      
      if (options.limit) params.append('limit', options.limit.toString());
      if (options.minSeverity) params.append('minSeverity', options.minSeverity.toString());
      if (options.issueType) params.append('issueType', options.issueType);

      const queryString = params.toString();
      const url = queryString 
        ? `/api/repositories/${repositoryId}/hotspots?${queryString}`
        : `/api/repositories/${repositoryId}/hotspots`;
      
      const response = await this.api.get(url);
      return response.data;
    } catch (error) {
      console.error('Failed to get quality hotspots:', error);
      throw new Error('Failed to load quality hotspots. Please try again.');
    }
  }

  /**
   * Zone 3: Get repository activity feed (quality events only, NOT commits)
   */
  async getRepositoryActivity(
    repositoryId: number,
    options: {
      limit?: number;
      since?: Date;
    } = {}
  ): Promise<RepositoryActivityResponse> {
    try {
      const params = new URLSearchParams();
      
      if (options.limit) params.append('limit', options.limit.toString());
      if (options.since) params.append('since', options.since.toISOString());

      const queryString = params.toString();
      const url = queryString 
        ? `/api/repositories/${repositoryId}/activity?${queryString}`
        : `/api/repositories/${repositoryId}/activity`;
      
      const response = await this.api.get(url);
      return response.data;
    } catch (error) {
      console.error('Failed to get repository activity:', error);
      throw new Error('Failed to load repository activity. Please try again.');
    }
  }

  /**
   * Zone 4: Get quick actions for repository navigation
   */
  async getQuickActions(repositoryId: number): Promise<RepositoryQuickActions> {
    try {
      const response = await this.api.get(`/api/repositories/${repositoryId}/actions`);
      return response.data;
    } catch (error) {
      console.error('Failed to get quick actions:', error);
      throw new Error('Failed to load quick actions. Please try again.');
    }
  }

  /**
   * Get repository context for breadcrumb and navigation
   */
  async getRepositoryContext(repositoryId: number): Promise<RepositoryContext> {
    try {
      const response = await this.api.get(`/api/repositories/${repositoryId}/context`);
      return response.data;
    } catch (error) {
      console.error('Failed to get repository context:', error);
      throw new Error('Failed to load repository context. Please try again.');
    }
  }

  /**
   * Manual refresh of repository data
   */
  async refreshRepository(repositoryId: number): Promise<boolean> {
    try {
      const response = await this.api.post(`/api/repositories/${repositoryId}/refresh`);
      return response.data;
    } catch (error) {
      console.error('Failed to refresh repository:', error);
      throw new Error('Failed to refresh repository. Please try again.');
    }
  }

  /**
   * Check repository service health
   */
  async checkHealth(): Promise<{ service: string; status: string; timestamp: string }> {
    try {
      const response = await this.api.get('/api/repositories/health');
      return response.data;
    } catch (error) {
      console.error('Repository service health check failed:', error);
      throw new Error('Repository service is not available.');
    }
  }
}

// Export singleton instance
export const repositoryApiService = new RepositoryApiService();
export default repositoryApiService;

// Severity Color Mapping (matching backend colors)
export const HOTSPOT_SEVERITY_COLORS = {
  [HotspotSeverity.Critical]: '#DC2626', // Red
  [HotspotSeverity.High]: '#EA580C',     // Orange
  [HotspotSeverity.Medium]: '#D97706',   // Amber
  [HotspotSeverity.Low]: '#16A34A',      // Green
} as const;

// Severity Labels
export const HOTSPOT_SEVERITY_LABELS = {
  [HotspotSeverity.Critical]: 'Critical',
  [HotspotSeverity.High]: 'High',
  [HotspotSeverity.Medium]: 'Medium',
  [HotspotSeverity.Low]: 'Low',
} as const;

// Issue Type Labels
export const HOTSPOT_ISSUE_TYPE_LABELS = {
  [HotspotIssueType.Complexity]: 'Complexity',
  [HotspotIssueType.Security]: 'Security',
  [HotspotIssueType.TechnicalDebt]: 'Technical Debt',
  [HotspotIssueType.TestCoverage]: 'Test Coverage',
  [HotspotIssueType.Performance]: 'Performance',
  [HotspotIssueType.Maintainability]: 'Maintainability',
} as const;

// Activity Type Icons
export const ACTIVITY_TYPE_ICONS = {
  [RepositoryActivityType.QualityGatePass]: '✅',
  [RepositoryActivityType.QualityGateFail]: '❌',
  [RepositoryActivityType.NewCriticalIssue]: '🔴',
  [RepositoryActivityType.SecurityVulnerabilityDetected]: '🛡️',
  [RepositoryActivityType.ComplexitySpike]: '📈',
  [RepositoryActivityType.BuildSuccess]: '✅',
  [RepositoryActivityType.BuildFailure]: '❌',
  [RepositoryActivityType.SyncComplete]: '🔄',
  [RepositoryActivityType.AnalysisComplete]: '🔍',
} as const;

// Activity Severity Colors
export const ACTIVITY_SEVERITY_COLORS = {
  [ActivitySeverity.Info]: '#6B7280',
  [ActivitySeverity.Success]: '#16A34A',
  [ActivitySeverity.Warning]: '#D97706',
  [ActivitySeverity.Error]: '#EA580C',
  [ActivitySeverity.Critical]: '#DC2626',
} as const;

// Utility functions
export const getHotspotSeverityColor = (severity: HotspotSeverity): string => {
  return HOTSPOT_SEVERITY_COLORS[severity] || '#6B7280';
};

export const getActivityIcon = (activityType: RepositoryActivityType): string => {
  return ACTIVITY_TYPE_ICONS[activityType] || 'ℹ️';
};

export const getActivitySeverityColor = (severity: ActivitySeverity): string => {
  return ACTIVITY_SEVERITY_COLORS[severity] || '#6B7280';
};

export const formatEstimatedTime = (hours: number): string => {
  if (hours < 1) {
    const minutes = Math.round(hours * 60);
    return `~${minutes}m`;
  } else if (hours < 8) {
    return `~${hours.toFixed(1)}h`;
  } else {
    const days = Math.ceil(hours / 8);
    return `~${days}d`;
  }
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
