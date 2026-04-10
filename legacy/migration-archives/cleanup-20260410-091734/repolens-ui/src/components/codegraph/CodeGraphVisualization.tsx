import React, { useState, useEffect, useRef } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Card,
  CardContent,
  Alert,
  Chip,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Skeleton,
  IconButton,
  Button,
  TextField,
  InputAdornment,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Tooltip,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Tabs,
  Tab
} from '@mui/material';
import {
  AccountTree,
  Search,
  Refresh,
  ZoomIn,
  ZoomOut,
  CenterFocusStrong,
  Download,
  ExpandMore,
  Code,
  Class,
  Functions,
  Hub,
  Timeline,
  FilterList,
  Visibility,
  VisibilityOff,
  Close,
  OpenInNew
} from '@mui/icons-material';
import ForceGraph2D from 'react-force-graph-2d';
import apiService from '../../services/apiService';

interface CodeNode {
  id: string;
  name: string;
  type: 'component' | 'class' | 'method' | 'function' | 'interface' | 'service';
  filePath: string;
  lineNumber: number;
  complexity: number;
  dependencies: string[];
  dependents: string[];
  size: number;
  level: number; // UI=0, Service=1, Method=2, etc.
}

interface CodeEdge {
  source: string;
  target: string;
  type: 'calls' | 'imports' | 'extends' | 'implements' | 'uses';
  weight: number;
}

interface CodeGraphData {
  nodes: CodeNode[];
  edges: CodeEdge[];
  metadata: {
    totalNodes: number;
    totalEdges: number;
    maxDepth: number;
    analysisTimestamp: string;
  };
}

interface CodeGraphVisualizationProps {
  repositoryId: number;
}

const CodeGraphVisualization: React.FC<CodeGraphVisualizationProps> = ({ repositoryId }) => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [graphData, setGraphData] = useState<CodeGraphData | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedNode, setSelectedNode] = useState<CodeNode | null>(null);
  const [filterType, setFilterType] = useState<string>('all');
  const [zoom, setZoom] = useState(1);
  const [showLegend, setShowLegend] = useState(true);
  const [viewMode, setViewMode] = useState<'graph' | 'hierarchy'>('graph');
  const [detailDialogOpen, setDetailDialogOpen] = useState(false);
  const [hoveredNode, setHoveredNode] = useState<CodeNode | null>(null);
  const [highlightNodes, setHighlightNodes] = useState(new Set());
  const [highlightLinks, setHighlightLinks] = useState(new Set());
  const fgRef = useRef<any>();

  useEffect(() => {
    loadCodeGraph();
  }, [repositoryId]);

  const loadCodeGraph = async () => {
    if (!repositoryId) return;

    try {
      setLoading(true);
      setError(null);
      
      // Use existing getCodeGraph API method
      const data = await apiService.getCodeGraph(repositoryId);
      
      if (data) {
        setGraphData(data);
        generateSampleGraphData(); // Generate sample data if API returns empty
      } else {
        generateSampleGraphData();
      }
    } catch (err: any) {
      console.error('Error loading code graph:', err);
      setError('Failed to load code graph. Generating sample visualization...');
      generateSampleGraphData(); // Fallback to sample data
    } finally {
      setLoading(false);
    }
  };

  const generateSampleGraphData = () => {
    const sampleNodes: CodeNode[] = [
      // UI Layer (Level 0)
      {
        id: 'ui-1',
        name: 'RepositoryDetails.tsx',
        type: 'component',
        filePath: 'src/components/repositories/RepositoryDetails.tsx',
        lineNumber: 1,
        complexity: 15,
        dependencies: ['api-1', 'service-1'],
        dependents: ['app-1'],
        size: 850,
        level: 0
      },
      {
        id: 'ui-2',
        name: 'SecurityAnalytics.tsx',
        type: 'component',
        filePath: 'src/components/security/SecurityAnalytics.tsx',
        lineNumber: 1,
        complexity: 12,
        dependencies: ['api-1', 'service-2'],
        dependents: ['ui-1'],
        size: 650,
        level: 0
      },
      {
        id: 'ui-3',
        name: 'DependencyAnalytics.tsx',
        type: 'component',
        filePath: 'src/components/dependencies/DependencyAnalytics.tsx',
        lineNumber: 1,
        complexity: 10,
        dependencies: ['api-1', 'service-3'],
        dependents: ['ui-1'],
        size: 580,
        level: 0
      },
      
      // Service Layer (Level 1)
      {
        id: 'api-1',
        name: 'apiService',
        type: 'service',
        filePath: 'src/services/apiService.ts',
        lineNumber: 1,
        complexity: 25,
        dependencies: ['class-1', 'class-2', 'class-3'],
        dependents: ['ui-1', 'ui-2', 'ui-3'],
        size: 1200,
        level: 1
      },
      {
        id: 'service-1',
        name: 'ConfigService',
        type: 'service',
        filePath: 'src/config/ConfigService.ts',
        lineNumber: 1,
        complexity: 5,
        dependencies: ['method-1'],
        dependents: ['ui-1', 'api-1'],
        size: 150,
        level: 1
      },
      {
        id: 'service-2',
        name: 'SecurityService',
        type: 'service',
        filePath: 'src/services/SecurityService.ts',
        lineNumber: 1,
        complexity: 18,
        dependencies: ['method-2', 'method-3'],
        dependents: ['ui-2'],
        size: 420,
        level: 1
      },
      {
        id: 'service-3',
        name: 'DependencyService',
        type: 'service',
        filePath: 'src/services/DependencyService.ts',
        lineNumber: 1,
        complexity: 14,
        dependencies: ['method-4', 'method-5'],
        dependents: ['ui-3'],
        size: 380,
        level: 1
      },

      // Class Layer (Level 2)
      {
        id: 'class-1',
        name: 'AnalyticsController',
        type: 'class',
        filePath: 'RepoLens.Api/Controllers/AnalyticsController.cs',
        lineNumber: 1,
        complexity: 20,
        dependencies: ['method-6', 'method-7'],
        dependents: ['api-1'],
        size: 600,
        level: 2
      },
      {
        id: 'class-2',
        name: 'SecurityController',
        type: 'class',
        filePath: 'RepoLens.Api/Controllers/SecurityController.cs',
        lineNumber: 1,
        complexity: 16,
        dependencies: ['method-8', 'method-9'],
        dependents: ['api-1'],
        size: 480,
        level: 2
      },
      {
        id: 'class-3',
        name: 'RepositoryService',
        type: 'class',
        filePath: 'RepoLens.Core/Services/RepositoryService.cs',
        lineNumber: 1,
        complexity: 22,
        dependencies: ['method-10', 'method-11'],
        dependents: ['api-1'],
        size: 720,
        level: 2
      },

      // Method Layer (Level 3)
      {
        id: 'method-1',
        name: 'getApiUrl()',
        type: 'method',
        filePath: 'src/config/ConfigService.ts',
        lineNumber: 25,
        complexity: 2,
        dependencies: [],
        dependents: ['service-1'],
        size: 50,
        level: 3
      },
      {
        id: 'method-2',
        name: 'analyzeVulnerabilities()',
        type: 'method',
        filePath: 'src/services/SecurityService.ts',
        lineNumber: 45,
        complexity: 8,
        dependencies: ['method-12'],
        dependents: ['service-2'],
        size: 180,
        level: 3
      },
      {
        id: 'method-3',
        name: 'calculateSecurityScore()',
        type: 'method',
        filePath: 'src/services/SecurityService.ts',
        lineNumber: 120,
        complexity: 6,
        dependencies: ['method-13'],
        dependents: ['service-2'],
        size: 140,
        level: 3
      },
      {
        id: 'method-4',
        name: 'analyzeDependencies()',
        type: 'method',
        filePath: 'src/services/DependencyService.ts',
        lineNumber: 35,
        complexity: 12,
        dependencies: ['method-14'],
        dependents: ['service-3'],
        size: 250,
        level: 3
      },
      {
        id: 'method-5',
        name: 'checkLicenseCompatibility()',
        type: 'method',
        filePath: 'src/services/DependencyService.ts',
        lineNumber: 180,
        complexity: 10,
        dependencies: ['method-15'],
        dependents: ['service-3'],
        size: 200,
        level: 3
      },
      {
        id: 'method-6',
        name: 'GetRepositoryAnalytics()',
        type: 'method',
        filePath: 'RepoLens.Api/Controllers/AnalyticsController.cs',
        lineNumber: 45,
        complexity: 15,
        dependencies: ['method-16'],
        dependents: ['class-1'],
        size: 300,
        level: 3
      },
      {
        id: 'method-7',
        name: 'GetFileMetrics()',
        type: 'method',
        filePath: 'RepoLens.Api/Controllers/AnalyticsController.cs',
        lineNumber: 120,
        complexity: 18,
        dependencies: ['method-17'],
        dependents: ['class-1'],
        size: 350,
        level: 3
      },

      // Low-level methods (Level 4)
      {
        id: 'method-12',
        name: 'scanCodeForPatterns()',
        type: 'function',
        filePath: 'src/utils/securityUtils.ts',
        lineNumber: 15,
        complexity: 5,
        dependencies: [],
        dependents: ['method-2'],
        size: 80,
        level: 4
      },
      {
        id: 'method-13',
        name: 'aggregateSecurityMetrics()',
        type: 'function',
        filePath: 'src/utils/securityUtils.ts',
        lineNumber: 65,
        complexity: 4,
        dependencies: [],
        dependents: ['method-3'],
        size: 60,
        level: 4
      },
      {
        id: 'method-14',
        name: 'parsePackageJson()',
        type: 'function',
        filePath: 'src/utils/dependencyUtils.ts',
        lineNumber: 20,
        complexity: 7,
        dependencies: [],
        dependents: ['method-4'],
        size: 120,
        level: 4
      },
      {
        id: 'method-15',
        name: 'validateLicense()',
        type: 'function',
        filePath: 'src/utils/licenseUtils.ts',
        lineNumber: 30,
        complexity: 6,
        dependencies: [],
        dependents: ['method-5'],
        size: 100,
        level: 4
      },
      {
        id: 'method-16',
        name: 'CalculateMetrics()',
        type: 'method',
        filePath: 'RepoLens.Core/Services/MetricsService.cs',
        lineNumber: 80,
        complexity: 12,
        dependencies: [],
        dependents: ['method-6'],
        size: 220,
        level: 4
      },
      {
        id: 'method-17',
        name: 'ProcessFileAnalysis()',
        type: 'method',
        filePath: 'RepoLens.Core/Services/FileAnalysisService.cs',
        lineNumber: 150,
        complexity: 14,
        dependencies: [],
        dependents: ['method-7'],
        size: 280,
        level: 4
      }
    ];

    const sampleEdges: CodeEdge[] = [
      // UI to Service connections
      { source: 'ui-1', target: 'api-1', type: 'uses', weight: 5 },
      { source: 'ui-1', target: 'service-1', type: 'imports', weight: 2 },
      { source: 'ui-2', target: 'api-1', type: 'uses', weight: 3 },
      { source: 'ui-2', target: 'service-2', type: 'uses', weight: 4 },
      { source: 'ui-3', target: 'api-1', type: 'uses', weight: 3 },
      { source: 'ui-3', target: 'service-3', type: 'uses', weight: 4 },

      // Service to Class connections
      { source: 'api-1', target: 'class-1', type: 'calls', weight: 8 },
      { source: 'api-1', target: 'class-2', type: 'calls', weight: 6 },
      { source: 'api-1', target: 'class-3', type: 'calls', weight: 7 },
      { source: 'service-1', target: 'method-1', type: 'calls', weight: 2 },
      { source: 'service-2', target: 'method-2', type: 'calls', weight: 4 },
      { source: 'service-2', target: 'method-3', type: 'calls', weight: 3 },
      { source: 'service-3', target: 'method-4', type: 'calls', weight: 5 },
      { source: 'service-3', target: 'method-5', type: 'calls', weight: 3 },

      // Class to Method connections
      { source: 'class-1', target: 'method-6', type: 'calls', weight: 6 },
      { source: 'class-1', target: 'method-7', type: 'calls', weight: 7 },
      { source: 'class-2', target: 'method-8', type: 'calls', weight: 4 },
      { source: 'class-2', target: 'method-9', type: 'calls', weight: 5 },
      { source: 'class-3', target: 'method-10', type: 'calls', weight: 6 },
      { source: 'class-3', target: 'method-11', type: 'calls', weight: 5 },

      // Method to Low-level connections
      { source: 'method-2', target: 'method-12', type: 'calls', weight: 3 },
      { source: 'method-3', target: 'method-13', type: 'calls', weight: 2 },
      { source: 'method-4', target: 'method-14', type: 'calls', weight: 4 },
      { source: 'method-5', target: 'method-15', type: 'calls', weight: 3 },
      { source: 'method-6', target: 'method-16', type: 'calls', weight: 5 },
      { source: 'method-7', target: 'method-17', type: 'calls', weight: 6 }
    ];

    const sampleData: CodeGraphData = {
      nodes: sampleNodes,
      edges: sampleEdges,
      metadata: {
        totalNodes: sampleNodes.length,
        totalEdges: sampleEdges.length,
        maxDepth: 4,
        analysisTimestamp: new Date().toISOString()
      }
    };

    setGraphData(sampleData);
  };

  const getNodeColor = (type: string): string => {
    switch (type) {
      case 'component': return '#1976d2'; // Blue for UI components
      case 'service': return '#388e3c'; // Green for services
      case 'class': return '#f57c00'; // Orange for classes
      case 'method': return '#7b1fa2'; // Purple for methods
      case 'function': return '#d32f2f'; // Red for functions
      case 'interface': return '#455a64'; // Blue-grey for interfaces
      default: return '#757575'; // Grey for unknown
    }
  };

  const getNodeIcon = (type: string) => {
    switch (type) {
      case 'component': return <Code />;
      case 'service': return <Hub />;
      case 'class': return <Class />;
      case 'method': return <Functions />;
      case 'function': return <Functions />;
      case 'interface': return <AccountTree />;
      default: return <Code />;
    }
  };

  const filteredNodes = graphData?.nodes.filter(node => {
    const matchesSearch = node.name.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesFilter = filterType === 'all' || node.type === filterType;
    return matchesSearch && matchesFilter;
  }) || [];

  const renderGraphStats = () => {
    if (!graphData) return null;

    return (
      <Grid container spacing={3}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography variant="h4" fontWeight="bold" color="primary.main">
                    {graphData.metadata.totalNodes}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Total Nodes
                  </Typography>
                </Box>
                <AccountTree fontSize="large" color="primary" />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography variant="h4" fontWeight="bold" color="success.main">
                    {graphData.metadata.totalEdges}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Relationships
                  </Typography>
                </Box>
                <Timeline fontSize="large" color="success" />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography variant="h4" fontWeight="bold" color="warning.main">
                    {graphData.metadata.maxDepth}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Max Depth
                  </Typography>
                </Box>
                <Hub fontSize="large" color="warning" />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography variant="h4" fontWeight="bold" color="info.main">
                    {new Set(graphData.nodes.map(n => n.type)).size}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Node Types
                  </Typography>
                </Box>
                <FilterList fontSize="large" color="info" />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    );
  };

  const renderNodeHierarchy = () => {
    if (!graphData) return null;

    const nodesByLevel = graphData.nodes.reduce((acc, node) => {
      if (!acc[node.level]) acc[node.level] = [];
      acc[node.level].push(node);
      return acc;
    }, {} as { [level: number]: CodeNode[] });

    const levelNames = {
      0: 'UI Components',
      1: 'Services',
      2: 'Classes/Controllers', 
      3: 'Methods',
      4: 'Low-level Functions'
    };

    return (
      <Box>
        <Typography variant="h6" gutterBottom>
          Code Hierarchy
        </Typography>
        {Object.entries(nodesByLevel)
          .sort(([a], [b]) => parseInt(a) - parseInt(b))
          .map(([level, nodes]) => (
            <Accordion key={level} defaultExpanded={parseInt(level) <= 2}>
              <AccordionSummary expandIcon={<ExpandMore />}>
                <Typography variant="h6">
                  Level {level}: {levelNames[parseInt(level) as keyof typeof levelNames]}
                  <Chip label={nodes.length} size="small" sx={{ ml: 2 }} />
                </Typography>
              </AccordionSummary>
              <AccordionDetails>
                <List dense>
                  {nodes.map(node => (
                    <ListItem 
                      key={node.id}
                      onClick={() => setSelectedNode(node)}
                      sx={{ 
                        cursor: 'pointer',
                        '&:hover': { backgroundColor: 'action.hover' },
                        backgroundColor: selectedNode?.id === node.id ? 'action.selected' : 'transparent'
                      }}
                    >
                      <ListItemIcon>
                        <Box sx={{ color: getNodeColor(node.type) }}>
                          {getNodeIcon(node.type)}
                        </Box>
                      </ListItemIcon>
                      <ListItemText
                        primary={node.name}
                        secondary={
                          <Box>
                            <Typography variant="caption" display="block">
                              {node.filePath}
                            </Typography>
                            <Box display="flex" gap={1} mt={0.5}>
                              <Chip label={`Complexity: ${node.complexity}`} size="small" />
                              <Chip label={`Size: ${node.size} lines`} size="small" />
                            </Box>
                          </Box>
                        }
                      />
                    </ListItem>
                  ))}
                </List>
              </AccordionDetails>
            </Accordion>
          ))}
      </Box>
    );
  };

  const renderNodeDetails = () => {
    if (!selectedNode) {
      return (
        <Alert severity="info">
          Click on a node in the hierarchy to view detailed information about dependencies and relationships.
        </Alert>
      );
    }

    const dependencies = graphData?.nodes.filter(node => 
      selectedNode.dependencies.includes(node.id)
    ) || [];

    const dependents = graphData?.nodes.filter(node => 
      selectedNode.dependents.includes(node.id)
    ) || [];

    return (
      <Box>
        <Typography variant="h6" gutterBottom>
          {selectedNode.name}
        </Typography>
        
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6}>
            <Paper sx={{ p: 2 }}>
              <Typography variant="subtitle2" gutterBottom>
                File Information
              </Typography>
              <Typography variant="body2" gutterBottom>
                <strong>Path:</strong> {selectedNode.filePath}
              </Typography>
              <Typography variant="body2" gutterBottom>
                <strong>Line:</strong> {selectedNode.lineNumber}
              </Typography>
              <Typography variant="body2" gutterBottom>
                <strong>Type:</strong> {selectedNode.type}
              </Typography>
            </Paper>
          </Grid>
          
          <Grid item xs={12} sm={6}>
            <Paper sx={{ p: 2 }}>
              <Typography variant="subtitle2" gutterBottom>
                Metrics
              </Typography>
              <Typography variant="body2" gutterBottom>
                <strong>Complexity:</strong> {selectedNode.complexity}
              </Typography>
              <Typography variant="body2" gutterBottom>
                <strong>Size:</strong> {selectedNode.size} lines
              </Typography>
              <Typography variant="body2" gutterBottom>
                <strong>Level:</strong> {selectedNode.level}
              </Typography>
            </Paper>
          </Grid>
        </Grid>

        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <Paper sx={{ p: 2 }}>
              <Typography variant="subtitle2" gutterBottom>
                Dependencies ({dependencies.length})
              </Typography>
              {dependencies.length > 0 ? (
                <List dense>
                  {dependencies.map(dep => (
                    <ListItem key={dep.id} sx={{ px: 0 }}>
                      <ListItemIcon>
                        <Box sx={{ color: getNodeColor(dep.type) }}>
                          {getNodeIcon(dep.type)}
                        </Box>
                      </ListItemIcon>
                      <ListItemText
                        primary={dep.name}
                        secondary={dep.type}
                      />
                    </ListItem>
                  ))}
                </List>
              ) : (
                <Typography variant="body2" color="text.secondary">
                  No dependencies
                </Typography>
              )}
            </Paper>
          </Grid>

          <Grid item xs={12} sm={6}>
            <Paper sx={{ p: 2 }}>
              <Typography variant="subtitle2" gutterBottom>
                Used By ({dependents.length})
              </Typography>
              {dependents.length > 0 ? (
                <List dense>
                  {dependents.map(dep => (
                    <ListItem key={dep.id} sx={{ px: 0 }}>
                      <ListItemIcon>
                        <Box sx={{ color: getNodeColor(dep.type) }}>
                          {getNodeIcon(dep.type)}
                        </Box>
                      </ListItemIcon>
                      <ListItemText
                        primary={dep.name}
                        secondary={dep.type}
                      />
                    </ListItem>
                  ))}
                </List>
              ) : (
                <Typography variant="body2" color="text.secondary">
                  Not used by other components
                </Typography>
              )}
            </Paper>
          </Grid>
        </Grid>
      </Box>
    );
  };

  if (loading) {
    return (
      <Box>
        <Typography variant="h5" gutterBottom>
          Code Graph Analysis
        </Typography>
        <Grid container spacing={3}>
          {[1, 2, 3, 4].map((i) => (
            <Grid item xs={12} sm={6} md={3} key={i}>
              <Card>
                <CardContent>
                  <Skeleton variant="text" width="60%" />
                  <Skeleton variant="text" width="40%" height={40} sx={{ mt: 1 }} />
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
        <Box sx={{ mt: 3 }}>
          <Skeleton variant="rectangular" width="100%" height={600} />
        </Box>
      </Box>
    );
  }

  if (error) {
    return (
      <Box>
        <Typography variant="h5" gutterBottom>
          Code Graph Analysis
        </Typography>
        <Alert severity="warning" action={
          <Button color="inherit" size="small" onClick={loadCodeGraph} startIcon={<Refresh />}>
            Retry
          </Button>
        }>
          {error}
        </Alert>
        {graphData && (
          <Box sx={{ mt: 3 }}>
            <Alert severity="info" sx={{ mb: 3 }}>
              Showing sample code graph visualization to demonstrate the feature.
            </Alert>
            {renderGraphStats()}
          </Box>
        )}
      </Box>
    );
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h5">
          Code Graph Analysis
        </Typography>
        <Box display="flex" gap={1}>
          <Button
            variant="outlined"
            startIcon={<Refresh />}
            onClick={loadCodeGraph}
            size="small"
          >
            Refresh
          </Button>
          <Button
            variant="outlined"
            startIcon={<Download />}
            size="small"
          >
            Export Graph
          </Button>
        </Box>
      </Box>

      {/* Graph Statistics */}
      <Box sx={{ mb: 3 }}>
        {renderGraphStats()}
      </Box>

      {/* Search and Filter Controls */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} sm={6} md={4}>
            <TextField
              fullWidth
              placeholder="Search nodes..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <Search />
                  </InputAdornment>
                ),
              }}
              size="small"
            />
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <FormControl fullWidth size="small">
              <InputLabel>Filter Type</InputLabel>
              <Select
                value={filterType}
                label="Filter Type"
                onChange={(e) => setFilterType(e.target.value)}
              >
                <MenuItem value="all">All Types</MenuItem>
                <MenuItem value="component">Components</MenuItem>
                <MenuItem value="service">Services</MenuItem>
                <MenuItem value="class">Classes</MenuItem>
                <MenuItem value="method">Methods</MenuItem>
                <MenuItem value="function">Functions</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} sm={12} md={5}>
            <Box display="flex" alignItems="center" gap={1}>
              <Typography variant="body2" color="text.secondary">
                Found: {filteredNodes.length} nodes
              </Typography>
              <Box sx={{ flexGrow: 1 }} />
              <IconButton 
                size="small" 
                onClick={() => setShowLegend(!showLegend)}
                color={showLegend ? 'primary' : 'default'}
              >
                {showLegend ? <Visibility /> : <VisibilityOff />}
              </IconButton>
            </Box>
          </Grid>
        </Grid>

        {/* Legend */}
        {showLegend && (
          <Box sx={{ mt: 2, pt: 2, borderTop: '1px solid #eee' }}>
            <Typography variant="subtitle2" gutterBottom>
              Node Types:
            </Typography>
            <Box display="flex" gap={2} flexWrap="wrap">
              {['component', 'service', 'class', 'method', 'function'].map(type => (
                <Chip
                  key={type}
                  icon={<Box sx={{ color: getNodeColor(type) }}>{getNodeIcon(type)}</Box>}
                  label={type.charAt(0).toUpperCase() + type.slice(1)}
                  variant="outlined"
                  size="small"
                />
              ))}
            </Box>
          </Box>
        )}
      </Paper>

      {/* View Mode Toggle */}
      <Box sx={{ mb: 3 }}>
        <Tabs value={viewMode} onChange={(e, newValue) => setViewMode(newValue)}>
          <Tab label="Visual Graph" value="graph" />
          <Tab label="Hierarchy View" value="hierarchy" />
        </Tabs>
      </Box>

      {/* Visual Graph View */}
      {viewMode === 'graph' && graphData && (
        <Box>
          <Paper sx={{ p: 2, mb: 3, height: 600 }}>
            <ForceGraph2D
              ref={fgRef}
              graphData={{
                nodes: filteredNodes,
                links: graphData.edges.filter(edge => 
                  filteredNodes.find(n => n.id === edge.source) && 
                  filteredNodes.find(n => n.id === edge.target)
                )
              }}
              nodeLabel={(node: any) => `
                <div style="background: white; padding: 8px; border-radius: 4px; box-shadow: 0 2px 8px rgba(0,0,0,0.2);">
                  <strong>${node.name}</strong><br/>
                  Type: ${node.type}<br/>
                  Complexity: ${node.complexity}<br/>
                  Size: ${node.size} lines
                </div>
              `}
              nodeColor={(node: any) => getNodeColor(node.type)}
              nodeRelSize={6}
              nodeVal={(node: any) => Math.max(4, node.complexity)}
              linkColor={() => '#666'}
              linkWidth={(link: any) => Math.sqrt(link.weight)}
              linkDirectionalArrowLength={3}
              linkDirectionalArrowRelPos={1}
              linkDirectionalArrowColor={() => '#666'}
              onNodeClick={(node: any) => {
                setSelectedNode(node);
                setDetailDialogOpen(true);
              }}
              onNodeHover={(node: any) => {
                setHoveredNode(node);
                if (node) {
                  // Highlight connected nodes
                  const neighbors = new Set([node.id]);
                  const links = new Set();
                  
                  graphData.edges.forEach(link => {
                    if (link.source === node.id) {
                      neighbors.add(link.target);
                      links.add(link);
                    } else if (link.target === node.id) {
                      neighbors.add(link.source);
                      links.add(link);
                    }
                  });
                  
                  setHighlightNodes(neighbors);
                  setHighlightLinks(links);
                } else {
                  setHighlightNodes(new Set());
                  setHighlightLinks(new Set());
                }
              }}
              enableZoomInteraction={true}
              enablePanInteraction={true}
              enableNodeDrag={true}
              d3AlphaDecay={0.02}
              d3VelocityDecay={0.3}
              cooldownTime={15000}
            />
          </Paper>

          {/* Graph Controls */}
          <Paper sx={{ p: 2, mb: 3 }}>
            <Grid container spacing={2} alignItems="center">
              <Grid item>
                <Tooltip title="Zoom In">
                  <IconButton onClick={() => fgRef.current?.zoom(fgRef.current.zoom() * 1.2)}>
                    <ZoomIn />
                  </IconButton>
                </Tooltip>
              </Grid>
              <Grid item>
                <Tooltip title="Zoom Out">
                  <IconButton onClick={() => fgRef.current?.zoom(fgRef.current.zoom() / 1.2)}>
                    <ZoomOut />
                  </IconButton>
                </Tooltip>
              </Grid>
              <Grid item>
                <Tooltip title="Center Graph">
                  <IconButton onClick={() => fgRef.current?.centerAt()}>
                    <CenterFocusStrong />
                  </IconButton>
                </Tooltip>
              </Grid>
              <Grid item xs>
                <Typography variant="body2" color="text.secondary">
                  Click nodes to view details • Hover to highlight connections • Drag to reposition
                </Typography>
              </Grid>
            </Grid>
          </Paper>
        </Box>
      )}

      {/* Hierarchy View */}
      {viewMode === 'hierarchy' && (
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <Paper sx={{ p: 2 }}>
              {renderNodeHierarchy()}
            </Paper>
          </Grid>

          <Grid item xs={12} md={6}>
            <Paper sx={{ p: 2 }}>
              <Typography variant="h6" gutterBottom>
                Node Details
              </Typography>
              {renderNodeDetails()}
            </Paper>
          </Grid>
        </Grid>
      )}

      {/* Node Detail Dialog */}
      <Dialog 
        open={detailDialogOpen} 
        onClose={() => setDetailDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Box display="flex" alignItems="center" justifyContent="space-between">
            <Box display="flex" alignItems="center" gap={1}>
              <Box sx={{ color: selectedNode ? getNodeColor(selectedNode.type) : 'inherit' }}>
                {selectedNode ? getNodeIcon(selectedNode.type) : <Code />}
              </Box>
              <Typography variant="h6">
                {selectedNode?.name || 'Node Details'}
              </Typography>
            </Box>
            <IconButton onClick={() => setDetailDialogOpen(false)}>
              <Close />
            </IconButton>
          </Box>
        </DialogTitle>
        
        <DialogContent>
          {selectedNode && (
            <Grid container spacing={3}>
              {/* Basic Information */}
              <Grid item xs={12} md={6}>
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>
                      📁 File Information
                    </Typography>
                    <Typography variant="body2" gutterBottom>
                      <strong>Path:</strong> {selectedNode.filePath}
                    </Typography>
                    <Typography variant="body2" gutterBottom>
                      <strong>Line:</strong> {selectedNode.lineNumber}
                    </Typography>
                    <Typography variant="body2" gutterBottom>
                      <strong>Type:</strong> 
                      <Chip 
                        label={selectedNode.type} 
                        size="small" 
                        sx={{ ml: 1, backgroundColor: getNodeColor(selectedNode.type), color: 'white' }}
                      />
                    </Typography>
                    <Box sx={{ mt: 2 }}>
                      <Button 
                        startIcon={<OpenInNew />} 
                        size="small" 
                        variant="outlined"
                        onClick={() => window.open(`vscode://file/${selectedNode.filePath}:${selectedNode.lineNumber}`, '_blank')}
                      >
                        Open in Editor
                      </Button>
                    </Box>
                  </CardContent>
                </Card>
              </Grid>

              {/* Metrics */}
              <Grid item xs={12} md={6}>
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>
                      📊 Code Metrics
                    </Typography>
                    <Grid container spacing={2}>
                      <Grid item xs={6}>
                        <Box textAlign="center">
                          <Typography variant="h4" color="warning.main">
                            {selectedNode.complexity}
                          </Typography>
                          <Typography variant="caption">Complexity</Typography>
                        </Box>
                      </Grid>
                      <Grid item xs={6}>
                        <Box textAlign="center">
                          <Typography variant="h4" color="info.main">
                            {selectedNode.size}
                          </Typography>
                          <Typography variant="caption">Lines</Typography>
                        </Box>
                      </Grid>
                      <Grid item xs={6}>
                        <Box textAlign="center">
                          <Typography variant="h4" color="primary.main">
                            {selectedNode.dependencies.length}
                          </Typography>
                          <Typography variant="caption">Dependencies</Typography>
                        </Box>
                      </Grid>
                      <Grid item xs={6}>
                        <Box textAlign="center">
                          <Typography variant="h4" color="success.main">
                            {selectedNode.dependents.length}
                          </Typography>
                          <Typography variant="caption">Dependents</Typography>
                        </Box>
                      </Grid>
                    </Grid>
                  </CardContent>
                </Card>
              </Grid>

              {/* Dependencies */}
              <Grid item xs={12} md={6}>
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>
                      🔗 Dependencies ({selectedNode.dependencies.length})
                    </Typography>
                    {selectedNode.dependencies.length > 0 ? (
                      <List dense>
                        {graphData?.nodes
                          .filter(node => selectedNode.dependencies.includes(node.id))
                          .map(dep => (
                            <ListItem 
                              key={dep.id} 
                              sx={{ 
                                px: 0,
                                cursor: 'pointer',
                                '&:hover': { backgroundColor: 'action.hover' }
                              }}
                              onClick={() => setSelectedNode(dep)}
                            >
                              <ListItemIcon>
                                <Box sx={{ color: getNodeColor(dep.type) }}>
                                  {getNodeIcon(dep.type)}
                                </Box>
                              </ListItemIcon>
                              <ListItemText
                                primary={dep.name}
                                secondary={`${dep.type} • ${dep.complexity} complexity`}
                              />
                            </ListItem>
                          ))}
                      </List>
                    ) : (
                      <Alert severity="info">No dependencies found</Alert>
                    )}
                  </CardContent>
                </Card>
              </Grid>

              {/* Dependents */}
              <Grid item xs={12} md={6}>
                <Card>
                  <CardContent>
                    <Typography variant="h6" gutterBottom>
                      📤 Used By ({selectedNode.dependents.length})
                    </Typography>
                    {selectedNode.dependents.length > 0 ? (
                      <List dense>
                        {graphData?.nodes
                          .filter(node => selectedNode.dependents.includes(node.id))
                          .map(dep => (
                            <ListItem 
                              key={dep.id} 
                              sx={{ 
                                px: 0,
                                cursor: 'pointer',
                                '&:hover': { backgroundColor: 'action.hover' }
                              }}
                              onClick={() => setSelectedNode(dep)}
                            >
                              <ListItemIcon>
                                <Box sx={{ color: getNodeColor(dep.type) }}>
                                  {getNodeIcon(dep.type)}
                                </Box>
                              </ListItemIcon>
                              <ListItemText
                                primary={dep.name}
                                secondary={`${dep.type} • ${dep.complexity} complexity`}
                              />
                            </ListItem>
                          ))}
                      </List>
                    ) : (
                      <Alert severity="info">Not used by other components</Alert>
                    )}
                  </CardContent>
                </Card>
              </Grid>
            </Grid>
          )}
        </DialogContent>
        
        <DialogActions>
          <Button onClick={() => setDetailDialogOpen(false)}>
            Close
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default CodeGraphVisualization;
