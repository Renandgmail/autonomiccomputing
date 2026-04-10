import React, { useState, useEffect, useMemo } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
  Alert,
  Card,
  CardContent,
  Chip,
  LinearProgress,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  IconButton,
  Tooltip,
  TextField,
  InputAdornment,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Divider,
  Badge,
  Switch,
  FormControlLabel
} from '@mui/material';
import {
  Code,
  BugReport,
  Security,
  Speed,
  Assessment,
  Warning,
  TrendingUp,
  TrendingDown,
  Search,
  Visibility,
  FilterList,
  Refresh,
  GetApp,
  Timeline,
  Psychology,
  Build,
  CheckCircle,
  Error as ErrorIcon,
  Info as InfoIcon
} from '@mui/icons-material';
import apiService from '../../services/apiService';

// File metrics data interfaces
interface FileMetric {
  id: number;
  filePath: string;
  fileName: string;
  language: string;
  size: {
    lines: number;
    bytes: number;
    effectiveLines: number;
  };
  complexity: {
    cyclomatic: number;
    cognitive: number;
    maxNesting: number;
    averageMethodLength: number;
  };
  quality: {
    maintainabilityIndex: number;
    codeSmells: number;
    technicalDebt: number;
    commentDensity: number;
    duplicationPercentage: number;
  };
  security: {
    hotspots: number;
    vulnerabilityRisk: string;
  };
  health: {
    overallScore: number;
    bugProneness: number;
    stabilityScore: number;
    maturityScore: number;
  };
  change: {
    churnRate: number;
    changeFrequency: number;
    lastModified: string;
  };
  analysis: {
    lastAnalyzed: string;
    version: string;
  };
}

interface FileDetails extends FileMetric {
  file: {
    id: number;
    repositoryId: number;
    filePath: string;
    fileName: string;
    directory: string;
    language: string;
    fileHash: string;
  };
  recommendations: {
    total: number;
    suggestions: string[];
    priority: 'High' | 'Medium' | 'Low';
  };
  performance: {
    compilationImpact: number;
    bundleSize: number;
    cpuIntensiveOperations: number;
    memoryFootprint: number;
  };
}

interface QualityHotspot {
  filePath: string;
  fileName: string;
  language: string;
  hotspotScore: number;
  priority: 'Critical' | 'High' | 'Medium' | 'Low';
  issues: {
    healthScore: number;
    complexity: number;
    technicalDebt: number;
    codeSmells: number;
    securityHotspots: number;
    churnRate: number;
    bugProneness: number;
  };
  impact: {
    linesOfCode: number;
    changeFrequency: number;
    stabilityScore: number;
  };
  recommendations: string[];
}

interface FileMetricsData {
  files: FileMetric[];
  pagination: {
    page: number;
    pageSize: number;
    total: number;
    totalPages: number;
  };
  summary: {
    averageHealth: number;
    averageComplexity: number;
    totalTechnicalDebt: number;
    highRiskFiles: number;
    languageBreakdown: { [key: string]: number };
  };
}

interface HotspotsData {
  hotspots: QualityHotspot[];
  summary: {
    totalHotspots: number;
    criticalIssues: number;
    highPriorityIssues: number;
    averageHotspotScore: number;
    recommendedActions: string[];
  };
}

interface FileMetricsDashboardProps {
  repositoryId: number;
  repositoryName: string;
}

const FileMetricsDashboard: React.FC<FileMetricsDashboardProps> = ({
  repositoryId,
  repositoryName
}) => {
  // State management
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [metricsData, setMetricsData] = useState<FileMetricsData | null>(null);
  const [hotspotsData, setHotspotsData] = useState<HotspotsData | null>(null);
  const [selectedFile, setSelectedFile] = useState<FileDetails | null>(null);
  const [fileDetailsOpen, setFileDetailsOpen] = useState(false);
  const [autoRefresh, setAutoRefresh] = useState(false);
  
  // Table state
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const [sortBy, setSortBy] = useState<'complexity' | 'quality' | 'debt' | 'health'>('health');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');
  const [searchTerm, setSearchTerm] = useState('');
  const [languageFilter, setLanguageFilter] = useState<string>('');

  // Load data on component mount and when parameters change
  useEffect(() => {
    loadFileMetrics();
    loadQualityHotspots();
  }, [repositoryId, page, pageSize, sortBy, sortOrder]);

  // Auto-refresh functionality
  useEffect(() => {
    if (autoRefresh) {
      const interval = setInterval(() => {
        loadFileMetrics();
        loadQualityHotspots();
      }, 30000); // Refresh every 30 seconds

      return () => clearInterval(interval);
    }
  }, [autoRefresh, repositoryId, page, pageSize, sortBy, sortOrder]);

  const loadFileMetrics = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const data = await apiService.getFileMetrics?.(
        repositoryId,
        page + 1, // API expects 1-based pagination
        pageSize,
        sortBy,
        sortOrder
      );
      
      setMetricsData(data);
    } catch (err) {
      setError('Failed to load file metrics. Please try again.');
      console.error('Error loading file metrics:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadQualityHotspots = async () => {
    try {
      const data = await apiService.getQualityHotspots?.(repositoryId, 10);
      setHotspotsData(data);
    } catch (err) {
      console.error('Error loading quality hotspots:', err);
    }
  };

  const loadFileDetails = async (filePath: string) => {
    try {
      const encodedPath = encodeURIComponent(filePath);
      const data = await apiService.getFileDetails?.(repositoryId, encodedPath);
      setSelectedFile(data);
      setFileDetailsOpen(true);
    } catch (err) {
      setError(`Failed to load file details for ${filePath}`);
      console.error('Error loading file details:', err);
    }
  };

  // Filtered and sorted data
  const filteredFiles = useMemo(() => {
    if (!metricsData?.files) return [];
    
    return metricsData.files.filter(file => {
      const matchesSearch = !searchTerm || 
        file.filePath.toLowerCase().includes(searchTerm.toLowerCase()) ||
        file.fileName.toLowerCase().includes(searchTerm.toLowerCase());
      
      const matchesLanguage = !languageFilter || file.language === languageFilter;
      
      return matchesSearch && matchesLanguage;
    });
  }, [metricsData?.files, searchTerm, languageFilter]);

  const getHealthColor = (score: number): string => {
    if (score >= 4.0) return '#4caf50'; // Excellent - Green
    if (score >= 3.0) return '#ff9800'; // Good - Orange
    if (score >= 2.0) return '#ff5722'; // Fair - Deep Orange
    if (score >= 1.0) return '#f44336'; // Poor - Red
    return '#9c27b0'; // Critical - Purple
  };

  const getHealthLabel = (score: number): string => {
    if (score >= 4.0) return 'Excellent';
    if (score >= 3.0) return 'Good';
    if (score >= 2.0) return 'Fair';
    if (score >= 1.0) return 'Poor';
    return 'Critical';
  };

  const getPriorityColor = (priority: string): string => {
    switch (priority) {
      case 'Critical': return '#d32f2f';
      case 'High': return '#f57c00';
      case 'Medium': return '#1976d2';
      case 'Low': return '#388e3c';
      default: return '#757575';
    }
  };

  const renderSummaryCards = () => {
    if (!metricsData?.summary) return null;

    const { summary } = metricsData;

    return (
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={1}>
                <Typography variant="h6" component="h3" color="text.secondary" fontSize="0.9rem">
                  Average Health Score
                </Typography>
                <Assessment sx={{ color: getHealthColor(summary.averageHealth) }} />
              </Box>
              <Typography variant="h4" component="h2" fontWeight="bold" mb={1}>
                {summary.averageHealth.toFixed(1)}
              </Typography>
              <Chip 
                label={getHealthLabel(summary.averageHealth)}
                size="small"
                sx={{ 
                  backgroundColor: getHealthColor(summary.averageHealth),
                  color: 'white'
                }}
              />
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={1}>
                <Typography variant="h6" component="h3" color="text.secondary" fontSize="0.9rem">
                  Average Complexity
                </Typography>
                <Psychology sx={{ color: '#ff9800' }} />
              </Box>
              <Typography variant="h4" component="h2" fontWeight="bold" mb={1}>
                {summary.averageComplexity.toFixed(1)}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Cyclomatic complexity
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={1}>
                <Typography variant="h6" component="h3" color="text.secondary" fontSize="0.9rem">
                  Technical Debt
                </Typography>
                <Build sx={{ color: '#f44336' }} />
              </Box>
              <Typography variant="h4" component="h2" fontWeight="bold" mb={1}>
                {summary.totalTechnicalDebt.toFixed(1)}h
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Total estimated hours
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={1}>
                <Typography variant="h6" component="h3" color="text.secondary" fontSize="0.9rem">
                  High Risk Files
                </Typography>
                <Warning sx={{ color: '#f44336' }} />
              </Box>
              <Typography variant="h4" component="h2" fontWeight="bold" mb={1}>
                {summary.highRiskFiles}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Files needing attention
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    );
  };

  const renderQualityHotspots = () => {
    if (!hotspotsData?.hotspots.length) return null;

    return (
      <Paper sx={{ p: 3, mb: 4 }}>
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
          <Typography variant="h6">
            Quality Hotspots
          </Typography>
          <Chip 
            label={`${hotspotsData.summary.totalHotspots} issues found`}
            color="warning"
            variant="outlined"
          />
        </Box>

        <Grid container spacing={2}>
          {hotspotsData.hotspots.slice(0, 5).map((hotspot, index) => (
            <Grid item xs={12} key={index}>
              <Card variant="outlined" sx={{ cursor: 'pointer' }} 
                    onClick={() => loadFileDetails(hotspot.filePath)}>
                <CardContent sx={{ py: 2 }}>
                  <Box display="flex" justifyContent="space-between" alignItems="flex-start">
                    <Box flex={1}>
                      <Box display="flex" alignItems="center" gap={1} mb={1}>
                        <Typography variant="subtitle1" fontWeight="bold">
                          {hotspot.fileName}
                        </Typography>
                        <Chip 
                          label={hotspot.priority}
                          size="small"
                          sx={{ 
                            backgroundColor: getPriorityColor(hotspot.priority),
                            color: 'white'
                          }}
                        />
                        <Chip label={hotspot.language} size="small" variant="outlined" />
                      </Box>
                      
                      <Typography variant="body2" color="text.secondary" mb={1}>
                        {hotspot.filePath}
                      </Typography>
                      
                      <Box display="flex" gap={2}>
                        <Typography variant="caption">
                          Health: {hotspot.issues.healthScore.toFixed(1)}
                        </Typography>
                        <Typography variant="caption">
                          Complexity: {hotspot.issues.complexity.toFixed(1)}
                        </Typography>
                        <Typography variant="caption">
                          Debt: {hotspot.issues.technicalDebt.toFixed(1)}h
                        </Typography>
                        {hotspot.issues.securityHotspots > 0 && (
                          <Typography variant="caption" color="error">
                            Security: {hotspot.issues.securityHotspots} issues
                          </Typography>
                        )}
                      </Box>
                    </Box>
                    
                    <Box textAlign="right">
                      <Typography variant="h6" fontWeight="bold" color="error">
                        {hotspot.hotspotScore.toFixed(1)}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        Risk Score
                      </Typography>
                    </Box>
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>

        {hotspotsData.summary.recommendedActions.length > 0 && (
          <Box mt={3} p={2} bgcolor="grey.50" borderRadius={1}>
            <Typography variant="subtitle2" gutterBottom>
              Recommended Actions:
            </Typography>
            <List dense>
              {hotspotsData.summary.recommendedActions.map((action, index) => (
                <ListItem key={index}>
                  <ListItemIcon>
                    <CheckCircle fontSize="small" color="primary" />
                  </ListItemIcon>
                  <ListItemText primary={action} />
                </ListItem>
              ))}
            </List>
          </Box>
        )}
      </Paper>
    );
  };

  const renderFileTable = () => {
    if (!metricsData) return null;

    return (
      <Paper sx={{ width: '100%', overflow: 'hidden' }}>
        {/* Table Controls */}
        <Box p={3} borderBottom={1} borderColor="divider">
          <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
            <Typography variant="h6">
              File Metrics ({filteredFiles.length} files)
            </Typography>
            
            <Box display="flex" gap={2} alignItems="center">
              <FormControlLabel
                control={
                  <Switch 
                    checked={autoRefresh} 
                    onChange={(e) => setAutoRefresh(e.target.checked)}
                    size="small"
                  />
                }
                label="Auto-refresh"
              />
              
              <Button
                startIcon={<Refresh />}
                onClick={loadFileMetrics}
                size="small"
                variant="outlined"
              >
                Refresh
              </Button>
            </Box>
          </Box>

          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                size="small"
                placeholder="Search files..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Search />
                    </InputAdornment>
                  )
                }}
              />
            </Grid>
            
            <Grid item xs={12} sm={2}>
              <FormControl fullWidth size="small">
                <InputLabel>Language</InputLabel>
                <Select
                  value={languageFilter}
                  label="Language"
                  onChange={(e) => setLanguageFilter(e.target.value)}
                >
                  <MenuItem value="">All Languages</MenuItem>
                  {metricsData.summary?.languageBreakdown && 
                    Object.keys(metricsData.summary.languageBreakdown).map(lang => (
                      <MenuItem key={lang} value={lang}>{lang}</MenuItem>
                    ))
                  }
                </Select>
              </FormControl>
            </Grid>

            <Grid item xs={12} sm={3}>
              <FormControl fullWidth size="small">
                <InputLabel>Sort By</InputLabel>
                <Select
                  value={sortBy}
                  label="Sort By"
                  onChange={(e) => setSortBy(e.target.value as any)}
                >
                  <MenuItem value="health">Health Score</MenuItem>
                  <MenuItem value="complexity">Complexity</MenuItem>
                  <MenuItem value="quality">Quality</MenuItem>
                  <MenuItem value="debt">Technical Debt</MenuItem>
                </Select>
              </FormControl>
            </Grid>

            <Grid item xs={12} sm={3}>
              <FormControl fullWidth size="small">
                <InputLabel>Order</InputLabel>
                <Select
                  value={sortOrder}
                  label="Order"
                  onChange={(e) => setSortOrder(e.target.value as any)}
                >
                  <MenuItem value="desc">Highest First</MenuItem>
                  <MenuItem value="asc">Lowest First</MenuItem>
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </Box>

        {/* Table */}
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>File</TableCell>
                <TableCell>Language</TableCell>
                <TableCell align="center">Health Score</TableCell>
                <TableCell align="center">Complexity</TableCell>
                <TableCell align="center">Quality Index</TableCell>
                <TableCell align="center">Tech Debt</TableCell>
                <TableCell align="center">Security</TableCell>
                <TableCell align="center">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loading ? (
                Array.from({ length: pageSize }).map((_, index) => (
                  <TableRow key={index}>
                    {Array.from({ length: 8 }).map((_, cellIndex) => (
                      <TableCell key={cellIndex}>
                        <LinearProgress />
                      </TableCell>
                    ))}
                  </TableRow>
                ))
              ) : filteredFiles.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={8} align="center">
                    <Box py={4}>
                      <Code sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
                      <Typography variant="h6" color="text.secondary">
                        No files found
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Try adjusting your search criteria or filters.
                      </Typography>
                    </Box>
                  </TableCell>
                </TableRow>
              ) : (
                filteredFiles.map((file) => (
                  <TableRow key={file.id} hover>
                    <TableCell>
                      <Box>
                        <Typography variant="subtitle2" fontWeight="bold">
                          {file.fileName}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {file.filePath}
                        </Typography>
                      </Box>
                    </TableCell>
                    
                    <TableCell>
                      <Chip label={file.language} size="small" variant="outlined" />
                    </TableCell>
                    
                    <TableCell align="center">
                      <Box display="flex" alignItems="center" justifyContent="center" gap={1}>
                        <Typography 
                          variant="body2" 
                          fontWeight="bold"
                          sx={{ color: getHealthColor(file.health.overallScore) }}
                        >
                          {file.health.overallScore.toFixed(1)}
                        </Typography>
                        <Chip 
                          label={getHealthLabel(file.health.overallScore)}
                          size="small"
                          sx={{ 
                            fontSize: '0.6rem',
                            backgroundColor: getHealthColor(file.health.overallScore),
                            color: 'white'
                          }}
                        />
                      </Box>
                    </TableCell>
                    
                    <TableCell align="center">
                      <Typography variant="body2">
                        {file.complexity.cyclomatic.toFixed(1)}
                      </Typography>
                    </TableCell>
                    
                    <TableCell align="center">
                      <Typography variant="body2">
                        {file.quality.maintainabilityIndex.toFixed(1)}
                      </Typography>
                    </TableCell>
                    
                    <TableCell align="center">
                      <Typography 
                        variant="body2"
                        color={file.quality.technicalDebt > 60 ? 'error' : 'text.primary'}
                      >
                        {(file.quality.technicalDebt / 60).toFixed(1)}h
                      </Typography>
                    </TableCell>
                    
                    <TableCell align="center">
                      {file.security.hotspots > 0 ? (
                        <Badge badgeContent={file.security.hotspots} color="error">
                          <Security color="error" />
                        </Badge>
                      ) : (
                        <CheckCircle color="success" />
                      )}
                    </TableCell>
                    
                    <TableCell align="center">
                      <Tooltip title="View Details">
                        <IconButton 
                          size="small"
                          onClick={() => loadFileDetails(file.filePath)}
                        >
                          <Visibility />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>

        {/* Pagination */}
        {metricsData.pagination && (
          <TablePagination
            component="div"
            count={metricsData.pagination.total}
            page={page}
            onPageChange={(_, newPage) => setPage(newPage)}
            rowsPerPage={pageSize}
            onRowsPerPageChange={(e) => {
              setPageSize(parseInt(e.target.value, 10));
              setPage(0);
            }}
            rowsPerPageOptions={[10, 20, 50, 100]}
          />
        )}
      </Paper>
    );
  };

  const renderFileDetailsDialog = () => {
    if (!selectedFile) return null;

    return (
      <Dialog 
        open={fileDetailsOpen} 
        onClose={() => setFileDetailsOpen(false)}
        maxWidth="lg"
        fullWidth
      >
        <DialogTitle>
          <Box display="flex" justifyContent="space-between" alignItems="center">
            <Box>
              <Typography variant="h6">{selectedFile.file.fileName}</Typography>
              <Typography variant="body2" color="text.secondary">
                {selectedFile.file.filePath}
              </Typography>
            </Box>
            <Chip 
              label={selectedFile.recommendations.priority}
              color={selectedFile.recommendations.priority === 'High' ? 'error' : 'warning'}
            />
          </Box>
        </DialogTitle>
        
        <DialogContent>
          <Grid container spacing={3}>
            {/* File Info */}
            <Grid item xs={12} md={6}>
              <Paper sx={{ p: 2 }}>
                <Typography variant="h6" gutterBottom>File Information</Typography>
                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Language</Typography>
                    <Typography variant="body1">{selectedFile.file.language}</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Size</Typography>
                    <Typography variant="body1">{selectedFile.size.lines} lines</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Last Modified</Typography>
                    <Typography variant="body1">
                      {new Date(selectedFile.change.lastModified).toLocaleDateString()}
                    </Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Churn Rate</Typography>
                    <Typography variant="body1">{selectedFile.change.churnRate.toFixed(2)}</Typography>
                  </Grid>
                </Grid>
              </Paper>
            </Grid>

            {/* Health Scores */}
            <Grid item xs={12} md={6}>
              <Paper sx={{ p: 2 }}>
                <Typography variant="h6" gutterBottom>Health Metrics</Typography>
                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Overall Health</Typography>
                    <Typography variant="h5" sx={{ color: getHealthColor(selectedFile.health.overallScore) }}>
                      {selectedFile.health.overallScore.toFixed(1)}
                    </Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Bug Proneness</Typography>
                    <Typography variant="h6">{selectedFile.health.bugProneness.toFixed(1)}</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Stability</Typography>
                    <Typography variant="h6">{selectedFile.health.stabilityScore.toFixed(1)}</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Maturity</Typography>
                    <Typography variant="h6">{selectedFile.health.maturityScore.toFixed(1)}</Typography>
                  </Grid>
                </Grid>
              </Paper>
            </Grid>

            {/* Complexity Metrics */}
            <Grid item xs={12} md={6}>
              <Paper sx={{ p: 2 }}>
                <Typography variant="h6" gutterBottom>Complexity Analysis</Typography>
                <Box mb={2}>
                  <Box display="flex" justifyContent="space-between" mb={1}>
                    <Typography variant="body2">Cyclomatic Complexity</Typography>
                    <Typography variant="body2">{selectedFile.complexity.cyclomatic.toFixed(1)}</Typography>
                  </Box>
                  <LinearProgress 
                    variant="determinate" 
                    value={Math.min(selectedFile.complexity.cyclomatic * 5, 100)} 
                    color={selectedFile.complexity.cyclomatic > 10 ? "error" : "primary"}
                  />
                </Box>
                <Box mb={2}>
                  <Box display="flex" justifyContent="space-between" mb={1}>
                    <Typography variant="body2">Cognitive Complexity</Typography>
                    <Typography variant="body2">{selectedFile.complexity.cognitive.toFixed(1)}</Typography>
                  </Box>
                  <LinearProgress 
                    variant="determinate" 
                    value={Math.min(selectedFile.complexity.cognitive * 3.33, 100)} 
                    color={selectedFile.complexity.cognitive > 15 ? "error" : "primary"}
                  />
                </Box>
              </Paper>
            </Grid>

            {/* Quality Metrics */}
            <Grid item xs={12} md={6}>
              <Paper sx={{ p: 2 }}>
                <Typography variant="h6" gutterBottom>Quality Assessment</Typography>
                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Maintainability Index</Typography>
                    <Typography variant="h6">{selectedFile.quality.maintainabilityIndex.toFixed(1)}</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Code Smells</Typography>
                    <Typography variant="h6" color={selectedFile.quality.codeSmells > 0 ? 'error' : 'success'}>
                      {selectedFile.quality.codeSmells}
                    </Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Technical Debt</Typography>
                    <Typography variant="h6">{(selectedFile.quality.technicalDebt / 60).toFixed(1)}h</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body2" color="text.secondary">Comment Density</Typography>
                    <Typography variant="h6">{selectedFile.quality.commentDensity.toFixed(1)}%</Typography>
                  </Grid>
                </Grid>
              </Paper>
            </Grid>

            {/* Security Issues */}
            {selectedFile.security.hotspots > 0 && (
              <Grid item xs={12}>
                <Alert severity="warning">
                  <Typography variant="h6" gutterBottom>
                    Security Hotspots Detected
                  </Typography>
                  <Typography>
                    This file has {selectedFile.security.hotspots} security hotspot(s) that need attention. 
                    Vulnerability risk level: {selectedFile.security.vulnerabilityRisk}
                  </Typography>
                </Alert>
              </Grid>
            )}

            {/* Recommendations */}
            {selectedFile.recommendations.suggestions.length > 0 && (
              <Grid item xs={12}>
                <Paper sx={{ p: 2 }}>
                  <Typography variant="h6" gutterBottom>
                    Improvement Recommendations ({selectedFile.recommendations.total})
                  </Typography>
                  <List>
                    {selectedFile.recommendations.suggestions.map((suggestion, index) => (
                      <ListItem key={index}>
                        <ListItemIcon>
                          {selectedFile.recommendations.priority === 'High' ? 
                            <ErrorIcon color="error" /> : <InfoIcon color="primary" />
                          }
                        </ListItemIcon>
                        <ListItemText primary={suggestion} />
                      </ListItem>
                    ))}
                  </List>
                </Paper>
              </Grid>
            )}
          </Grid>
        </DialogContent>
        
        <DialogActions>
          <Button onClick={() => setFileDetailsOpen(false)}>
            Close
          </Button>
        </DialogActions>
      </Dialog>
    );
  };

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 3 }}>
        {error}
      </Alert>
    );
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={4}>
        <Box>
          <Typography variant="h4" component="h1" gutterBottom>
            File Metrics Dashboard
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Detailed analysis for {repositoryName}
          </Typography>
        </Box>
      </Box>

      {renderSummaryCards()}
      {renderQualityHotspots()}
      {renderFileTable()}
      {renderFileDetailsDialog()}
    </Box>
  );
};

export default FileMetricsDashboard;
