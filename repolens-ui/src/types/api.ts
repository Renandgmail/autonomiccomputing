// API Types for RepoLens Backend Integration
export interface Repository {
  id: number;
  name: string;
  url: string;
  description?: string;
  defaultBranch?: string;
  createdAt: string;
  updatedAt?: string;
  lastSyncAt?: string;
  lastAnalysisAt?: string;
  status: RepositoryStatus;
  isPrivate: boolean;
  autoSync: boolean;
  syncIntervalMinutes: number;
  syncErrorMessage?: string;
  ownerName?: string;
  organizationName?: string;
  processingStatus: ProcessingStatus;
  
  // Analytics properties
  processedCommits?: number;
  processedFiles?: number;
  contributorCount?: number;
  sizeInBytes?: number;
  
  // Metrics data properties
  totalCommits?: number;
  totalFiles?: number;
  totalContributors?: number;
  languages?: string[];
}

export enum RepositoryStatus {
  Active = 1,
  Syncing = 2,
  Error = 3,
  Archived = 4,
  Paused = 5
}

export enum ProcessingStatus {
  Pending = 1,
  InProgress = 2,
  Processing = 2, // Alias
  Completed = 3,
  Failed = 4,
  Cancelled = 5
}

export interface RepositoryStats {
  totalCommits: number;
  totalFiles: number;
  totalContributors: number;
  sizeInBytes: number;
  lastCommitDate?: string;
  languageDistribution: Record<string, number>;
  codeQualityScore: number;
  projectHealthScore: number;
}

export interface DashboardStats {
  totalRepositories: number;
  totalArtifacts: number;
  totalStorageBytes: number;
  processingStatus: ProcessingStatus;
  recentActivity: ActivityItem[];
  errorMessage?: string;
}

export interface ActivityItem {
  id: string;
  type: ActivityType;
  message: string;
  timestamp: string;
  status: ProcessingStatus;
  details?: string;
}

export enum ActivityType {
  RepositoryAdded = 1,
  RepositorySynced = 2,
  RepositoryAnalyzed = 3,
  RepositoryError = 4,
  UserActivity = 5,
  SystemEvent = 6,
  RepositoryProcessed = 7
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  errorMessage?: string;
  validationErrors?: string[];
}

export interface SearchRequest {
  query: string;
  repositoryId?: number;
  fileTypes?: string[];
  fromDate?: string;
  toDate?: string;
  pageSize?: number;
  page?: number;
}

export interface SearchResults {
  query: string;
  totalResults: number;
  results: SearchResultItem[];
  errorMessage?: string;
}

export interface SearchResultItem {
  repositoryName: string;
  filePath: string;
  artifactId: number;
  matchType: SearchMatchType;
  contextLines: string[];
}

export enum SearchMatchType {
  FileName,
  CodeContent,
  Comment,
  ClassName,
  MethodName
}

// Authentication types
export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  createdAt: string;
}

export interface AuthResponse {
  success: boolean;
  token?: string;
  user?: User;
  errorMessage?: string;
  debugInfo?: string;
}
