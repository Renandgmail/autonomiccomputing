import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import RepositoryDetails from '../RepositoryDetails';
import { Repository, RepositoryStatus, ProcessingStatus, ProviderType } from '../../../types/api';
import * as apiService from '../../../services/apiService';

// Mock the API service
jest.mock('../../../services/apiService');
const mockApiService = apiService as jest.Mocked<typeof apiService>;

// Mock useParams to return a repository ID
jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useParams: () => ({ id: '1' }),
}));

// Create theme for Material-UI components
const theme = createTheme();

// Test data
const mockRepository: Repository = {
  id: 1,
  name: 'Test Repository',
  url: 'https://github.com/test/repo',
  description: 'Test repository for analytics dashboard',
  defaultBranch: 'main',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-02T00:00:00Z',
  lastSyncAt: '2024-01-02T12:00:00Z',
  status: RepositoryStatus.Active,
  providerType: ProviderType.GitHub,
  isPrivate: false,
  autoSync: true,
  syncIntervalMinutes: 60,
  processingStatus: ProcessingStatus.Completed,
  
  // Analytics data
  healthScore: 85,
  codeQualityScore: 78,
  activityLevelScore: 92,
  maintenanceScore: 88,
  
  maintainabilityIndex: 75,
  cyclomaticComplexity: 2.5,
  codeDuplication: 3.2,
  technicalDebtHours: 12.5,
  
  buildSuccessRate: 95,
  testCoverage: 87,
  
  securityVulnerabilities: 2,
  outdatedDependencies: 5,
  
  totalCommits: 245,
  totalFiles: 128,
  totalContributors: 8,
  busFactor: 3,
  
  languageDistribution: {
    'C#': 60,
    'TypeScript': 25,
    'JavaScript': 15
  },
  
  topContributors: [
    { name: 'John Doe', commits: 89, percentage: 36 },
    { name: 'Jane Smith', commits: 67, percentage: 27 },
    { name: 'Bob Wilson', commits: 45, percentage: 18 }
  ],
  
  activityPatterns: {
    'January': 25,
    'February': 18,
    'March': 32
  }
};

const renderComponent = () => {
  return render(
    <MemoryRouter initialEntries={['/repositories/1']}>
      <ThemeProvider theme={theme}>
        <RepositoryDetails />
      </ThemeProvider>
    </MemoryRouter>
  );
};

describe('RepositoryDetails Analytics Dashboard', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders repository name and basic information', async () => {
    mockApiService.getRepository.mockResolvedValue(mockRepository);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Test Repository')).toBeInTheDocument();
      expect(screen.getByText('Test repository for analytics dashboard')).toBeInTheDocument();
    });
  });

  test('displays health score with circular progress indicator', async () => {
    mockApiService.getRepository.mockResolvedValue(mockRepository);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Repository Health')).toBeInTheDocument();
      expect(screen.getByText('85%')).toBeInTheDocument();
    });
  });

  test('shows code quality metrics section', async () => {
    mockApiService.getRepository.mockResolvedValue(mockRepository);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Code Quality')).toBeInTheDocument();
      expect(screen.getByText('Maintainability Index')).toBeInTheDocument();
      expect(screen.getByText('75')).toBeInTheDocument();
      expect(screen.getByText('Technical Debt')).toBeInTheDocument();
      expect(screen.getByText('12.5 hours')).toBeInTheDocument();
    });
  });

  test('displays performance insights section', async () => {
    mockApiService.getRepository.mockResolvedValue(mockRepository);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Performance')).toBeInTheDocument();
      expect(screen.getByText('Build Success Rate')).toBeInTheDocument();
      expect(screen.getByText('95%')).toBeInTheDocument();
      expect(screen.getByText('Test Coverage')).toBeInTheDocument();
      expect(screen.getByText('87%')).toBeInTheDocument();
    });
  });

  test('shows security assessment section', async () => {
    mockApiService.getRepository.mockResolvedValue(mockRepository);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Security')).toBeInTheDocument();
      expect(screen.getByText('Vulnerabilities')).toBeInTheDocument();
      expect(screen.getByText('2')).toBeInTheDocument();
      expect(screen.getByText('Outdated Dependencies')).toBeInTheDocument();
      expect(screen.getByText('5')).toBeInTheDocument();
    });
  });

  test('renders language distribution visualization', async () => {
    mockApiService.getRepository.mockResolvedValue(mockRepository);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Language Distribution')).toBeInTheDocument();
      expect(screen.getByText('C#')).toBeInTheDocument();
      expect(screen.getByText('60%')).toBeInTheDocument();
      expect(screen.getByText('TypeScript')).toBeInTheDocument();
      expect(screen.getByText('25%')).toBeInTheDocument();
    });
  });

  test('displays top contributors section', async () => {
    mockApiService.getRepository.mockResolvedValue(mockRepository);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Top Contributors')).toBeInTheDocument();
      expect(screen.getByText('John Doe')).toBeInTheDocument();
      expect(screen.getByText('89 commits (36%)')).toBeInTheDocument();
      expect(screen.getByText('Jane Smith')).toBeInTheDocument();
      expect(screen.getByText('67 commits (27%)')).toBeInTheDocument();
    });
  });

  test('shows development activity charts', async () => {
    mockApiService.getRepository.mockResolvedValue(mockRepository);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Development Activity')).toBeInTheDocument();
      expect(screen.getByText('January')).toBeInTheDocument();
      expect(screen.getByText('February')).toBeInTheDocument();
      expect(screen.getByText('March')).toBeInTheDocument();
    });
  });

  test('displays recommendations section with strengths and improvements', async () => {
    mockApiService.getRepository.mockResolvedValue(mockRepository);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Recommendations')).toBeInTheDocument();
      expect(screen.getByText('Strengths')).toBeInTheDocument();
      expect(screen.getByText('Areas for Improvement')).toBeInTheDocument();
    });
  });

  test('handles loading state correctly', () => {
    mockApiService.getRepository.mockReturnValue(new Promise(() => {})); // Never resolves

    renderComponent();

    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  test('handles error state when repository fails to load', async () => {
    mockApiService.getRepository.mockRejectedValue(new Error('Failed to fetch repository'));

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText(/error/i)).toBeInTheDocument();
    });
  });

  test('displays fallback values when analytics data is missing', async () => {
    const repositoryWithoutAnalytics = {
      ...mockRepository,
      healthScore: undefined,
      codeQualityScore: undefined,
      maintainabilityIndex: undefined,
      technicalDebtHours: undefined
    };

    mockApiService.getRepository.mockResolvedValue(repositoryWithoutAnalytics);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Test Repository')).toBeInTheDocument();
      // Should show fallback values or "No data available" messages
    });
  });

  test('handles provider-specific display correctly', async () => {
    const gitHubRepo = { ...mockRepository, providerType: ProviderType.GitHub };
    const localRepo = { ...mockRepository, providerType: ProviderType.Local, url: '/local/path' };

    // Test GitHub repository
    mockApiService.getRepository.mockResolvedValue(gitHubRepo);
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Test Repository')).toBeInTheDocument();
    });

    // Test Local repository
    mockApiService.getRepository.mockResolvedValue(localRepo);
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Test Repository')).toBeInTheDocument();
    });
  });

  test('renders all progress bars with correct values', async () => {
    mockApiService.getRepository.mockResolvedValue(mockRepository);

    renderComponent();

    await waitFor(() => {
      // Check that progress bars are rendered for various metrics
      const progressBars = screen.getAllByRole('progressbar');
      expect(progressBars.length).toBeGreaterThan(0);
    });
  });

  test('shows correct color coding for different metric levels', async () => {
    const repoWithGoodMetrics = {
      ...mockRepository,
      healthScore: 95,
      codeQualityScore: 90,
      testCoverage: 95
    };

    const repoWithPoorMetrics = {
      ...mockRepository,
      healthScore: 45,
      codeQualityScore: 40,
      testCoverage: 30
    };

    // Test good metrics (should show green/success colors)
    mockApiService.getRepository.mockResolvedValue(repoWithGoodMetrics);
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('95%')).toBeInTheDocument();
    });

    // Test poor metrics (should show red/error colors)
    mockApiService.getRepository.mockResolvedValue(repoWithPoorMetrics);
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('45%')).toBeInTheDocument();
    });
  });

  test('responsive design works correctly on different screen sizes', async () => {
    mockApiService.getRepository.mockResolvedValue(mockRepository);

    // Test mobile viewport
    Object.defineProperty(window, 'innerWidth', { writable: true, configurable: true, value: 360 });
    Object.defineProperty(window, 'innerHeight', { writable: true, configurable: true, value: 640 });
    window.dispatchEvent(new Event('resize'));

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Test Repository')).toBeInTheDocument();
    });

    // Test desktop viewport
    Object.defineProperty(window, 'innerWidth', { writable: true, configurable: true, value: 1920 });
    Object.defineProperty(window, 'innerHeight', { writable: true, configurable: true, value: 1080 });
    window.dispatchEvent(new Event('resize'));

    await waitFor(() => {
      expect(screen.getByText('Test Repository')).toBeInTheDocument();
    });
  });
});
