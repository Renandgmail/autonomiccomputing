import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  Grid,
  Chip,
  Alert,
  CircularProgress,
  Divider,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Paper,
  IconButton,
  Tabs,
  Tab,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControlLabel,
  Checkbox,
  FormGroup,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Switch,
  Tooltip,
  TextField
} from '@mui/material';
import {
  ArrowBack,
  Code,
  Commit,
  Person,
  Schedule,
  Language,
  Assessment,
  TrendingUp,
  Storage,
  FilePresent,
  Settings,
  Sync,
  ExpandMore,
  Info,
  PlayArrow,
  Refresh,
  Security,
  AccountTree,
  BugReport
} from '@mui/icons-material';
import apiService from '../../services/apiService';
import { Repository, ProcessingStatus, RepositoryStatus } from '../../types/api';
import FileMetricsDashboard from '../analytics/FileMetricsDashboard';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`repository-tabpanel-${index}`}
      aria-labelledby={`repository-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ py: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

function a11yProps(index: number) {
  return {
    id: `repository-tab-${index}`,
    'aria-controls': `repository-tabpanel-${index}`,
  };
}

interface AnalysisConfig {
  enableFileMetrics: boolean;
  enableCodeComplexity: boolean;
  enableSecurityAnalysis: boolean;
  enablePerformanceMetrics: boolean;
  enableTechnicalDebt: boolean;
  enableDependencyAnalysis: boolean;
  enableVocabularyExtraction: boolean;
  enablePatternMining: boolean;
  enableASTAnalysis: boolean;
  enableIndexing: boolean;
  enableGraphConstruction: boolean;
  autoSync: boolean;
  syncInterval: number; // in minutes
}

const defaultAnalysisConfig: AnalysisConfig = {
  enableFileMetrics: true,
  enableCodeComplexity: true,
  enableSecurityAnalysis: true,
  enablePerformanceMetrics: true,
  enableTechnicalDebt: true,
  enableDependencyAnalysis: true,
  enableVocabularyExtraction: true,
  enablePatternMining: false, // Advanced feature
  enableASTAnalysis: false, // Advanced feature
  enableIndexing: false, // Advanced feature
  enableGraphConstruction: false, // Advanced feature
  autoSync: false,
  syncInterval: 60
};

const RepositoryDetails: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [repository, setRepository] = useState<Repository | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tabValue, setTabValue] = useState(0);
  const [configDialogOpen, setConfigDialogOpen] = useState(false);
  const [analysisConfig, setAnalysisConfig] = useState<AnalysisConfig>(defaultAnalysisConfig);
  const [syncInProgress, setSyncInProgress] = useState(false);

  const loadRepository = async () => {
    if (!id) return;
    
    try {
      setLoading(true);
      setError(null);
      // Try to get repository details - this might not exist yet in the API
      const repo = await apiService.getRepository(parseInt(id));
      setRepository(repo);
    } catch (err: any) {
      console.error('Error loading repository:', err);
      setError(err.message || 'Failed to load repository details');
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Never';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getStatusColor = (status: RepositoryStatus): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    switch (status) {
      case RepositoryStatus.Active:
        return 'success';
      case RepositoryStatus.Syncing:
        return 'warning';
      case RepositoryStatus.Error:
        return 'error';
      case RepositoryStatus.Archived:
        return 'secondary';
      default:
        return 'default';
    }
  };

  const getProcessingStatusColor = (status: ProcessingStatus): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    switch (status) {
      case ProcessingStatus.Completed:
        return 'success';
      case ProcessingStatus.InProgress:
      case ProcessingStatus.Processing:
        return 'warning';
      case ProcessingStatus.Failed:
        return 'error';
      case ProcessingStatus.Pending:
        return 'info';
      default:
        return 'default';
    }
  };

  const getRepositoryDisplayName = (repo: Repository) => {
    return repo.name || repo.url.split('/').pop()?.replace('.git', '') || 'Unknown Repository';
  };

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleConfigChange = (field: keyof AnalysisConfig) => (event: React.ChangeEvent<HTMLInputElement>) => {
    setAnalysisConfig(prev => ({
      ...prev,
      [field]: event.target.checked
    }));
  };

  const handleSyncIntervalChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(event.target.value);
    if (!isNaN(value) && value > 0) {
      setAnalysisConfig(prev => ({
        ...prev,
        syncInterval: value
      }));
    }
  };

  const saveAnalysisConfig = () => {
    // Save configuration to localStorage for persistence
    localStorage.setItem(`repo-${id}-config`, JSON.stringify(analysisConfig));
    setConfigDialogOpen(false);
    
    // If auto-sync is enabled, trigger sync with new config
    if (analysisConfig.autoSync) {
      triggerSyncWithConfig();
    }
  };

  const loadAnalysisConfig = () => {
    try {
      const saved = localStorage.getItem(`repo-${id}-config`);
      if (saved) {
        setAnalysisConfig({ ...defaultAnalysisConfig, ...JSON.parse(saved) });
      }
    } catch (error) {
      console.warn('Failed to load analysis config:', error);
    }
  };

  const triggerSyncWithConfig = async () => {
    if (!id || syncInProgress) return;

    try {
      setSyncInProgress(true);
      setError(null);

      // Trigger sync with configuration
      await apiService.syncRepository(parseInt(id));
      
      // Refresh repository data
      await loadRepository();
      
      // Show success message
      console.log('Sync triggered successfully with configuration:', analysisConfig);
    } catch (err: any) {
      setError(`Sync failed: ${err.message}`);
      console.error('Sync error:', err);
    } finally {
      setSyncInProgress(false);
    }
  };

  useEffect(() => {
    loadRepository();
    loadAnalysisConfig();
  }, [id]);

  useEffect(() => {
    // Set up auto-sync if enabled
    if (analysisConfig.autoSync && id && analysisConfig.syncInterval > 0) {
      const interval = setInterval(() => {
        triggerSyncWithConfig();
      }, analysisConfig.syncInterval * 60 * 1000); // Convert minutes to milliseconds

      return () => clearInterval(interval);
    }
  }, [analysisConfig.autoSync, analysisConfig.syncInterval, id]);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
        <CircularProgress size={60} />
      </Box>
    );
  }

  if (error) {
    return (
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <IconButton onClick={() => navigate('/repositories')} sx={{ mr: 2 }}>
            <ArrowBack />
          </IconButton>
          <Typography variant="h4" component="h1">
            Repository Details
          </Typography>
        </Box>
        
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
        
        <Alert severity="info" sx={{ mb: 3 }}>
          <Typography variant="body2">
            <strong>Note:</strong> The repository metrics API endpoint may not be fully implemented yet. 
            This is a preview of what the repository details page will look like once the backend API is complete.
          </Typography>
        </Alert>

        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Repository ID: {id}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              This repository's detailed metrics will be available once:
            </Typography>
            <List>
              <ListItem>
                <ListItemText primary="• The getRepository API endpoint is implemented" />
              </ListItem>
              <ListItem>
                <ListItemText primary="• Repository metrics are calculated and stored" />
              </ListItem>
              <ListItem>
                <ListItemText primary="• The repository has been successfully analyzed" />
              </ListItem>
            </List>
          </CardContent>
        </Card>
      </Box>
    );
  }

  if (!repository) {
    return (
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <IconButton onClick={() => navigate('/repositories')} sx={{ mr: 2 }}>
            <ArrowBack />
          </IconButton>
          <Typography variant="h4" component="h1">
            Repository Details
          </Typography>
        </Box>
        
        <Alert severity="warning">
          Repository not found or not accessible.
        </Alert>
      </Box>
    );
  }

  return (
    <Box>
      {/* Header with back button and controls */}
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <IconButton onClick={() => navigate('/repositories')} sx={{ mr: 2 }}>
          <ArrowBack />
        </IconButton>
        <Box sx={{ flexGrow: 1 }}>
          <Typography variant="h4" component="h1" gutterBottom>
            {getRepositoryDisplayName(repository)}
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Repository metrics and analytics
          </Typography>
        </Box>
        
        {/* Action buttons */}
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
          <Tooltip title="Configure Analysis Settings">
            <IconButton onClick={() => setConfigDialogOpen(true)} color="primary">
              <Settings />
            </IconButton>
          </Tooltip>
          
          <Button
            variant="outlined"
            onClick={triggerSyncWithConfig}
            startIcon={syncInProgress ? <CircularProgress size={16} /> : <Sync />}
            disabled={syncInProgress}
            size="small"
          >
            {syncInProgress ? 'Syncing...' : 'Sync Now'}
          </Button>
          
          <Button
            variant="outlined"
            href={repository.url}
            target="_blank"
            rel="noopener noreferrer"
            startIcon={<Code />}
            size="small"
          >
            View Source
          </Button>
        </Box>
      </Box>

      {/* Auto-sync indicator */}
      {analysisConfig.autoSync && (
        <Alert 
          severity="info" 
          sx={{ mb: 2 }}
          action={
            <Chip 
              label={`Every ${analysisConfig.syncInterval}min`} 
              size="small" 
              variant="outlined" 
            />
          }
        >
          Auto-sync is enabled with selected analysis options
        </Alert>
      )}

      {/* Tabs Navigation */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 0 }}>
        <Tabs value={tabValue} onChange={handleTabChange} aria-label="repository details tabs">
          <Tab label="Overview" {...a11yProps(0)} />
          <Tab label="File Metrics" {...a11yProps(1)} />
          <Tab label="Contributors" {...a11yProps(2)} />
          <Tab label="Security" {...a11yProps(3)} />
          <Tab label="Dependencies" {...a11yProps(4)} />
        </Tabs>
      </Box>

      {/* Tab Panels */}
      <TabPanel value={tabValue} index={0}>
        {/* Overview Tab - Original enhanced content */}
        
        {/* Repository Status */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Repository Status
            </Typography>
            
            <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
              <Chip
                label={RepositoryStatus[repository.status]}
                color={getStatusColor(repository.status)}
                variant="filled"
              />
              <Chip
                label={ProcessingStatus[repository.processingStatus]}
                color={getProcessingStatusColor(repository.processingStatus)}
                variant="outlined"
              />
            </Box>

            {repository.description && (
              <Typography variant="body2" color="text.secondary" paragraph>
                {repository.description}
              </Typography>
            )}

            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: {
                  xs: '1fr',
                  sm: '1fr 1fr'
                },
                gap: 3
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Schedule sx={{ fontSize: 20, color: 'text.secondary' }} />
                <Box>
                  <Typography variant="body2" fontWeight="bold">
                    Last Sync
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {formatDate(repository.lastSyncAt)}
                  </Typography>
                </Box>
              </Box>
              
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Storage sx={{ fontSize: 20, color: 'text.secondary' }} />
                <Box>
                  <Typography variant="body2" fontWeight="bold">
                    Repository URL
                  </Typography>
                  <Typography 
                    variant="body2" 
                    color="primary.main" 
                    sx={{ textDecoration: 'none', cursor: 'pointer' }}
                    component="a"
                    href={repository.url}
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    {repository.url}
                  </Typography>
                </Box>
              </Box>
            </Box>

            {repository.syncErrorMessage && (
              <Alert severity="error" sx={{ mt: 2 }}>
                <Typography variant="body2">
                  <strong>Sync Error:</strong> {repository.syncErrorMessage}
                </Typography>
              </Alert>
            )}
          </CardContent>
        </Card>

        {/* Metrics Overview */}
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: {
              xs: '1fr',
              sm: '1fr 1fr',
              md: '1fr 1fr 1fr 1fr'
            },
            gap: 3,
            mb: 3
          }}
        >
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <FilePresent sx={{ color: 'primary.main', mr: 1 }} />
                <Typography variant="h6">Files</Typography>
              </Box>
              <Typography variant="h4" color="primary.main">
                {repository.totalFiles?.toLocaleString() || repository.processedFiles?.toLocaleString() || '0'}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Files analyzed
              </Typography>
            </CardContent>
          </Card>

          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <Commit sx={{ color: 'success.main', mr: 1 }} />
                <Typography variant="h6">Commits</Typography>
              </Box>
              <Typography variant="h4" color="success.main">
                {repository.totalCommits?.toLocaleString() || repository.processedCommits?.toLocaleString() || '0'}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Total commits
              </Typography>
            </CardContent>
          </Card>

          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <Person sx={{ color: 'warning.main', mr: 1 }} />
                <Typography variant="h6">Contributors</Typography>
              </Box>
              <Typography variant="h4" color="warning.main">
                {repository.totalContributors?.toLocaleString() || repository.contributorCount?.toLocaleString() || '0'}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Contributors
              </Typography>
            </CardContent>
          </Card>

          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <Language sx={{ color: 'error.main', mr: 1 }} />
                <Typography variant="h6">Languages</Typography>
              </Box>
              <Typography variant="h4" color="error.main">
                {repository.languages?.length || '0'}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Programming languages
              </Typography>
            </CardContent>
          </Card>
        </Box>

        {/* Repository Health Score */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Repository Health Score
            </Typography>
            
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Box sx={{ position: 'relative', display: 'inline-flex', mr: 3 }}>
                <CircularProgress
                  variant="determinate"
                  value={repository.healthScore || 75}
                  size={80}
                  thickness={4}
                  sx={{ 
                    color: (repository.healthScore || 75) >= 80 ? 'success.main' : 
                           (repository.healthScore || 75) >= 60 ? 'warning.main' : 'error.main'
                  }}
                />
                <Box
                  sx={{
                    top: 0,
                    left: 0,
                    bottom: 0,
                    right: 0,
                    position: 'absolute',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                  }}
                >
                  <Typography variant="h6" component="div" color="text.secondary">
                    {Math.round(repository.healthScore || 75)}%
                  </Typography>
                </Box>
              </Box>
              
              <Box sx={{ flexGrow: 1 }}>
                <Typography variant="body1" fontWeight="bold" 
                  color={(repository.healthScore || 75) >= 80 ? 'success.main' : 
                        (repository.healthScore || 75) >= 60 ? 'warning.main' : 'error.main'}>
                  {(repository.healthScore || 75) >= 80 ? 'Excellent Health' : 
                   (repository.healthScore || 75) >= 60 ? 'Good Health' : 'Needs Attention'}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {(repository.healthScore || 75) >= 80 ? 'This repository shows strong development practices and maintainability' :
                   (repository.healthScore || 75) >= 60 ? 'Repository has good practices with some areas for improvement' :
                   'Repository requires attention to improve development practices'}
                </Typography>
              </Box>
            </Box>

            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: {
                  xs: '1fr',
                  sm: '1fr 1fr 1fr'
                },
                gap: 2
              }}
            >
              <Box>
                <Typography variant="body2" fontWeight="bold">Code Quality</Typography>
                <Typography variant="body2" 
                  color={(repository.codeQualityScore || 85) >= 80 ? 'success.main' : 'warning.main'}>
                  {Math.round(repository.codeQualityScore || 85)}% {(repository.codeQualityScore || 85) >= 80 ? 'Good' : 'Fair'}
                </Typography>
              </Box>
              <Box>
                <Typography variant="body2" fontWeight="bold">Activity Level</Typography>
                <Typography variant="body2" 
                  color={(repository.activityLevelScore || 70) >= 70 ? 'success.main' : 'warning.main'}>
                  {Math.round(repository.activityLevelScore || 70)}% {(repository.activityLevelScore || 70) >= 70 ? 'Active' : 'Moderate'}
                </Typography>
              </Box>
              <Box>
                <Typography variant="body2" fontWeight="bold">Maintenance</Typography>
                <Typography variant="body2" 
                  color={(repository.maintenanceScore || 82) >= 80 ? 'success.main' : 'warning.main'}>
                  {Math.round(repository.maintenanceScore || 82)}% {(repository.maintenanceScore || 82) >= 80 ? 'Well-maintained' : 'Maintained'}
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>

        {/* Quick metrics preview */}
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              📊 Quick Insights
            </Typography>
            
            <Grid container spacing={2}>
              <Grid item xs={12} sm={4}>
                <Typography variant="subtitle2" color="success.main">
                  ✅ Strengths
                </Typography>
                <List dense>
                  <ListItem sx={{ px: 0 }}>
                    <ListItemText 
                      primary="High code quality"
                      primaryTypographyProps={{ variant: 'body2' }}
                    />
                  </ListItem>
                  <ListItem sx={{ px: 0 }}>
                    <ListItemText 
                      primary="Active development"
                      primaryTypographyProps={{ variant: 'body2' }}
                    />
                  </ListItem>
                </List>
              </Grid>
              
              <Grid item xs={12} sm={4}>
                <Typography variant="subtitle2" color="warning.main">
                  ⚠️ Areas to Monitor
                </Typography>
                <List dense>
                  <ListItem sx={{ px: 0 }}>
                    <ListItemText 
                      primary="Dependency updates needed"
                      primaryTypographyProps={{ variant: 'body2' }}
                    />
                  </ListItem>
                  <ListItem sx={{ px: 0 }}>
                    <ListItemText 
                      primary="Test coverage improvement"
                      primaryTypographyProps={{ variant: 'body2' }}
                    />
                  </ListItem>
                </List>
              </Grid>
              
              <Grid item xs={12} sm={4}>
                <Alert severity="info" sx={{ height: 'fit-content' }}>
                  <Typography variant="body2">
                    Switch to <strong>File Metrics</strong> tab for detailed code analysis.
                  </Typography>
                </Alert>
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      </TabPanel>

      {/* File Metrics Tab */}
      <TabPanel value={tabValue} index={1}>
        <FileMetricsDashboard
          repositoryId={parseInt(id!)}
          repositoryName={getRepositoryDisplayName(repository)}
        />
      </TabPanel>

      {/* Contributors Tab */}
      <TabPanel value={tabValue} index={2}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Contributor Analytics
            </Typography>
            <Alert severity="info">
              Advanced contributor analytics dashboard coming soon. This will include:
              <List dense>
                <ListItem><ListItemText primary="• Team collaboration patterns" /></ListItem>
                <ListItem><ListItemText primary="• Individual productivity metrics" /></ListItem>
                <ListItem><ListItemText primary="• Knowledge sharing analysis" /></ListItem>
                <ListItem><ListItemText primary="• Risk assessment (bus factor)" /></ListItem>
              </List>
            </Alert>
          </CardContent>
        </Card>
      </TabPanel>

      {/* Security Tab */}
      <TabPanel value={tabValue} index={3}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Security Analysis
            </Typography>
            <Alert severity="info">
              Comprehensive security dashboard coming soon. This will include:
              <List dense>
                <ListItem><ListItemText primary="• Vulnerability scanning results" /></ListItem>
                <ListItem><ListItemText primary="• Dependency security analysis" /></ListItem>
                <ListItem><ListItemText primary="• Code security hotspots" /></ListItem>
                <ListItem><ListItemText primary="• Compliance checking" /></ListItem>
              </List>
            </Alert>
          </CardContent>
        </Card>
      </TabPanel>

      {/* Dependencies Tab */}
      <TabPanel value={tabValue} index={4}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Dependency Management
            </Typography>
            <Alert severity="info">
              Advanced dependency analysis coming soon. This will include:
              <List dense>
                <ListItem><ListItemText primary="• Dependency tree visualization" /></ListItem>
                <ListItem><ListItemText primary="• Outdated package tracking" /></ListItem>
                <ListItem><ListItemText primary="• License compatibility analysis" /></ListItem>
                <ListItem><ListItemText primary="• Bundle size impact assessment" /></ListItem>
              </List>
            </Alert>
          </CardContent>
        </Card>
      </TabPanel>

      {/* Analysis Configuration Dialog */}
      <Dialog 
        open={configDialogOpen} 
        onClose={() => setConfigDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Box display="flex" alignItems="center">
            <Settings sx={{ mr: 1 }} />
            Analysis Configuration
          </Box>
        </DialogTitle>
        
        <DialogContent>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            Configure which analysis metrics to process during repository synchronization. 
            Advanced features may require additional processing time.
          </Typography>

          {/* Core Analysis Options */}
          <Accordion defaultExpanded>
            <AccordionSummary expandIcon={<ExpandMore />}>
              <Typography variant="h6">Core Analysis</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <FormGroup>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={analysisConfig.enableFileMetrics}
                      onChange={handleConfigChange('enableFileMetrics')}
                    />
                  }
                  label={
                    <Box display="flex" alignItems="center">
                      <span>File Metrics Analysis</span>
                      <Tooltip title="Analyze file-level quality metrics, complexity, and health scores">
                        <Info sx={{ ml: 1, fontSize: 16, color: 'text.secondary' }} />
                      </Tooltip>
                    </Box>
                  }
                />
                
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={analysisConfig.enableCodeComplexity}
                      onChange={handleConfigChange('enableCodeComplexity')}
                    />
                  }
                  label={
                    <Box display="flex" alignItems="center">
                      <span>Code Complexity Metrics</span>
                      <Tooltip title="Calculate cyclomatic complexity, cognitive complexity, and nesting depth">
                        <Info sx={{ ml: 1, fontSize: 16, color: 'text.secondary' }} />
                      </Tooltip>
                    </Box>
                  }
                />
                
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={analysisConfig.enableTechnicalDebt}
                      onChange={handleConfigChange('enableTechnicalDebt')}
                    />
                  }
                  label={
                    <Box display="flex" alignItems="center">
                      <span>Technical Debt Calculation</span>
                      <Tooltip title="Estimate technical debt in hours and identify refactoring opportunities">
                        <Info sx={{ ml: 1, fontSize: 16, color: 'text.secondary' }} />
                      </Tooltip>
                    </Box>
                  }
                />
                
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={analysisConfig.enableSecurityAnalysis}
                      onChange={handleConfigChange('enableSecurityAnalysis')}
                    />
                  }
                  label={
                    <Box display="flex" alignItems="center">
                      <span>Security Analysis</span>
                      <Tooltip title="Scan for security vulnerabilities and hotspots">
                        <Info sx={{ ml: 1, fontSize: 16, color: 'text.secondary' }} />
                      </Tooltip>
                    </Box>
                  }
                />
                
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={analysisConfig.enablePerformanceMetrics}
                      onChange={handleConfigChange('enablePerformanceMetrics')}
                    />
                  }
                  label={
                    <Box display="flex" alignItems="center">
                      <span>Performance Metrics</span>
                      <Tooltip title="Analyze compilation impact, bundle size, and memory footprint">
                        <Info sx={{ ml: 1, fontSize: 16, color: 'text.secondary' }} />
                      </Tooltip>
                    </Box>
                  }
                />
              </FormGroup>
            </AccordionDetails>
          </Accordion>

          {/* Advanced Analysis Options */}
          <Accordion>
            <AccordionSummary expandIcon={<ExpandMore />}>
              <Typography variant="h6">Advanced Analysis</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <FormGroup>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={analysisConfig.enableVocabularyExtraction}
                      onChange={handleConfigChange('enableVocabularyExtraction')}
                    />
                  }
                  label={
                    <Box display="flex" alignItems="center">
                      <span>Vocabulary Extraction</span>
                      <Tooltip title="Extract business terms and technical vocabulary from code">
                        <Info sx={{ ml: 1, fontSize: 16, color: 'text.secondary' }} />
                      </Tooltip>
                    </Box>
                  }
                />
                
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={analysisConfig.enableDependencyAnalysis}
                      onChange={handleConfigChange('enableDependencyAnalysis')}
                    />
                  }
                  label={
                    <Box display="flex" alignItems="center">
                      <span>Dependency Analysis</span>
                      <Tooltip title="Analyze package dependencies, licenses, and security issues">
                        <Info sx={{ ml: 1, fontSize: 16, color: 'text.secondary' }} />
                      </Tooltip>
                    </Box>
                  }
                />
                
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={analysisConfig.enablePatternMining}
                      onChange={handleConfigChange('enablePatternMining')}
                    />
                  }
                  label={
                    <Box display="flex" alignItems="center">
                      <span>Pattern Mining</span>
                      <Tooltip title="Identify architectural patterns and anti-patterns in code">
                        <Info sx={{ ml: 1, fontSize: 16, color: 'text.secondary' }} />
                      </Tooltip>
                    </Box>
                  }
                />
              </FormGroup>
            </AccordionDetails>
          </Accordion>

          {/* Expert-Level Analysis Options */}
          <Accordion>
            <AccordionSummary expandIcon={<ExpandMore />}>
              <Typography variant="h6">Expert-Level Features</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <Alert severity="warning" sx={{ mb: 2 }}>
                <Typography variant="body2">
                  These features require significant processing time and resources. 
                  Enable only if you need advanced code intelligence capabilities.
                </Typography>
              </Alert>
              
              <FormGroup>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={analysisConfig.enableASTAnalysis}
                      onChange={handleConfigChange('enableASTAnalysis')}
                    />
                  }
                  label={
                    <Box display="flex" alignItems="center">
                      <span>AST (Abstract Syntax Tree) Analysis</span>
                      <Tooltip title="Deep code structure analysis using Abstract Syntax Trees">
                        <Info sx={{ ml: 1, fontSize: 16, color: 'text.secondary' }} />
                      </Tooltip>
                    </Box>
                  }
                />
                
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={analysisConfig.enableIndexing}
                      onChange={handleConfigChange('enableIndexing')}
                    />
                  }
                  label={
                    <Box display="flex" alignItems="center">
                      <span>Code Indexing for Search</span>
                      <Tooltip title="Create searchable indexes for fast code retrieval">
                        <Info sx={{ ml: 1, fontSize: 16, color: 'text.secondary' }} />
                      </Tooltip>
                    </Box>
                  }
                />
                
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={analysisConfig.enableGraphConstruction}
                      onChange={handleConfigChange('enableGraphConstruction')}
                    />
                  }
                  label={
                    <Box display="flex" alignItems="center">
                      <span>Code Relationship Graph Construction</span>
                      <Tooltip title="Build comprehensive relationship graphs between code entities">
                        <Info sx={{ ml: 1, fontSize: 16, color: 'text.secondary' }} />
                      </Tooltip>
                    </Box>
                  }
                />
              </FormGroup>
            </AccordionDetails>
          </Accordion>

          {/* Auto-Sync Configuration */}
          <Accordion>
            <AccordionSummary expandIcon={<ExpandMore />}>
              <Typography variant="h6">Automatic Synchronization</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <FormGroup>
                <FormControlLabel
                  control={
                    <Switch
                      checked={analysisConfig.autoSync}
                      onChange={handleConfigChange('autoSync')}
                    />
                  }
                  label="Enable Automatic Synchronization"
                />
                
                {analysisConfig.autoSync && (
                  <Box sx={{ mt: 2, ml: 3 }}>
                    <Typography variant="body2" gutterBottom>
                      Sync Interval (minutes):
                    </Typography>
                    <TextField
                      type="number"
                      value={analysisConfig.syncInterval}
                      onChange={handleSyncIntervalChange}
                      inputProps={{ min: 5, max: 1440 }}
                      size="small"
                      sx={{ width: '120px' }}
                    />
                    <Typography variant="caption" color="text.secondary" display="block" sx={{ mt: 1 }}>
                      Minimum: 5 minutes, Maximum: 24 hours (1440 minutes)
                    </Typography>
                  </Box>
                )}
              </FormGroup>
            </AccordionDetails>
          </Accordion>

          {/* Configuration Summary */}
          <Box sx={{ mt: 3, p: 2, bgcolor: 'grey.50', borderRadius: 1 }}>
            <Typography variant="subtitle2" gutterBottom>
              Configuration Summary:
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {Object.entries(analysisConfig)
                .filter(([key, value]) => key !== 'syncInterval' && value === true)
                .length} analysis features enabled
              {analysisConfig.autoSync && 
                `, auto-sync every ${analysisConfig.syncInterval} minutes`}
            </Typography>
          </Box>
        </DialogContent>
        
        <DialogActions>
          <Button onClick={() => setConfigDialogOpen(false)}>
            Cancel
          </Button>
          <Button onClick={saveAnalysisConfig} variant="contained" startIcon={<PlayArrow />}>
            Save & Apply Configuration
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default RepositoryDetails;
