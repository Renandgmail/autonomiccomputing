import React, { useState, useEffect, useRef, useMemo } from 'react';
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
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  InputAdornment,
  Switch,
  FormControlLabel,
  Tooltip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItem,
  ListItemText,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Slider,
  Paper
} from '@mui/material';
import {
  AccountTree,
  Search,
  FilterList,
  Refresh,
  ZoomIn,
  ZoomOut,
  CenterFocusStrong,
  Download,
  Settings,
  Visibility,
  VisibilityOff,
  Info,
  Code,
  Class,
  Functions,
  DataObject,
  Api,
  Storage,
  Security,
  ExpandMore,
  PlayArrow,
  Pause
} from '@mui/icons-material';
import apiService from '../../services/apiService';

// D3.js-like visualization interfaces (for future D3 integration)
interface GraphNode {
  id: string;
  name: string;
  type: 'namespace' | 'class' | 'interface' | 'method' | 'property' | 'service' | 'controller' | 'entity' | 'repository';
  level: number;
  size: number;
  complexity: number;
  dependencies: string[];
  dependents: string[];
  filePath: string;
  lineNumber: number;
  metadata: {
    visibility: 'public' | 'private' | 'protected' | 'internal';
    isStatic: boolean;
    isAbstract: boolean;
    parameters?: string[];
    returnType?: string;
    attributes?: string[];
  };
  metrics: {
    cyclomaticComplexity: number;
    linesOfCode: number;
    cognitiveComplexity: number;
    fanIn: number;
    fanOut: number;
  };
  position?: { x: number; y: number };
}

interface GraphEdge {
  id: string;
  source: string;
  target: string;
  type: 'calls' | 'implements' | 'extends' | 'uses' | 'contains' | 'references';
  weight: number;
  metadata: {
    callCount?: number;
    isDirectDependency: boolean;
    relationshipStrength: number;
  };
}

interface CodeGraph {
  nodes: GraphNode[];
  edges: GraphEdge[];
  metadata: {
    totalNodes: number;
    totalEdges: number;
    maxDepth: number;
    entryPoints: string[];
    orphanNodes: string[];
    circularDependencies: string[][];
  };
}

interface GraphFilter {
  nodeTypes: string[];
  visibilityLevels: string[];
  complexityRange: [number, number];
  showOnlyConnected: boolean;
  hidePrivate: boolean;
  maxDepth: number;
  searchTerm: string;
}

interface CodeGraphVisualizationProps {
  repositoryId: number;
  repositoryName: string;
}

const defaultFilter: GraphFilter = {
  nodeTypes: ['namespace', 'class', 'interface', 'method', 'service', 'controller', 'entity', 'repository'],
  visibilityLevels: ['public', 'protected', 'internal'],
  complexityRange: [0, 100],
  showOnlyConnected: true,
  hidePrivate: true,
  maxDepth: 10,
  searchTerm: ''
};

const nodeTypeColors = {
  namespace: '#2196f3',
  class: '#4caf50',
  interface: '#ff9800',
  method: '#9c27b0',
  property: '#607d8b',
  service: '#e91e63',
  controller: '#3f51b5',
  entity: '#009688',
  repository: '#795548'
};

const nodeTypeIcons = {
  namespace: <DataObject />,
  class: <Class />,
  interface: <Api />,
  method: <Functions />,
  property: <Storage />,
  service: <Settings />,
  controller: <Code />,
  entity: <Security />,
  repository: <Storage />
};

const CodeGraphVisualization: React.FC<CodeGraphVisualizationProps> = ({
  repositoryId,
  repositoryName
}) => {
  // State management
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [codeGraph, setCodeGraph] = useState<CodeGraph | null>(null);
  const [filter, setFilter] = useState<GraphFilter>(defaultFilter);
  const [selectedNode, setSelectedNode] = useState<GraphNode | null>(null);
  const [viewMode, setViewMode] = useState<'hierarchical' | 'force' | 'circular'>('hierarchical');
  const [showDetails, setShowDetails] = useState(false);
  const [animationEnabled, setAnimationEnabled] = useState(true);
  const [zoomLevel, setZoomLevel] = useState(1);
  
  // Refs for visualization container
  const svgRef = useRef<SVGSVGElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  // Load code graph data
  const loadCodeGraph = async () => {
    try {
      setLoading(true);
      setError(null);

      // Call API to get code graph data
      const data = await apiService.getCodeGraph?.(repositoryId);
      setCodeGraph(data);
    } catch (err) {
      setError('Failed to load code graph. This feature may require AST analysis to be enabled.');
      console.error('Error loading code graph:', err);
    } finally {
      setLoading(false);
    }
  };

  // Filter nodes based on current filter settings
  const filteredNodes = useMemo(() => {
    if (!codeGraph) return [];

    return codeGraph.nodes.filter(node => {
      // Type filter
      if (!filter.nodeTypes.includes(node.type)) return false;

      // Visibility filter
      if (filter.hidePrivate && node.metadata.visibility === 'private') return false;
      if (!filter.visibilityLevels.includes(node.metadata.visibility)) return false;

      // Complexity filter
      if (node.metrics.cyclomaticComplexity < filter.complexityRange[0] || 
          node.metrics.cyclomaticComplexity > filter.complexityRange[1]) return false;

      // Depth filter
      if (node.level > filter.maxDepth) return false;

      // Search filter
      if (filter.searchTerm && !node.name.toLowerCase().includes(filter.searchTerm.toLowerCase())) return false;

      // Connected nodes filter
      if (filter.showOnlyConnected) {
        const hasConnections = codeGraph.edges.some(edge => 
          edge.source === node.id || edge.target === node.id
        );
        if (!hasConnections && !codeGraph.metadata.entryPoints.includes(node.id)) return false;
      }

      return true;
    });
  }, [codeGraph, filter]);

  // Get filtered edges
  const filteredEdges = useMemo(() => {
    if (!codeGraph) return [];

    const filteredNodeIds = new Set(filteredNodes.map(n => n.id));
    return codeGraph.edges.filter(edge => 
      filteredNodeIds.has(edge.source) && filteredNodeIds.has(edge.target)
    );
  }, [codeGraph, filteredNodes]);

  // Handle filter changes
  const updateFilter = (key: keyof GraphFilter, value: any) => {
    setFilter(prev => ({ ...prev, [key]: value }));
  };

  // Handle node selection
  const handleNodeClick = (node: GraphNode) => {
    setSelectedNode(node);
    setShowDetails(true);
  };

  // Zoom controls
  const handleZoom = (delta: number) => {
    setZoomLevel(prev => Math.max(0.1, Math.min(3, prev + delta)));
  };

  // Export functionality
  const exportGraph = () => {
    // TODO: Implement graph export (SVG, PNG, or data format)
    console.log('Exporting graph...', { filteredNodes, filteredEdges });
  };

  // Find related nodes
  const findRelatedNodes = (nodeId: string): GraphNode[] => {
    if (!codeGraph) return [];

    const relatedIds = new Set<string>();
    
    // Add direct dependencies
    codeGraph.edges.forEach(edge => {
      if (edge.source === nodeId) relatedIds.add(edge.target);
      if (edge.target === nodeId) relatedIds.add(edge.source);
    });

    return filteredNodes.filter(node => relatedIds.has(node.id));
  };

  useEffect(() => {
    loadCodeGraph();
  }, [repositoryId]);

  // Render loading state
  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
        <Box textAlign="center">
          <CircularProgress size={60} />
          <Typography variant="h6" sx={{ mt: 2 }}>
            Analyzing Code Structure...
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Building comprehensive relationship graph
          </Typography>
        </Box>
      </Box>
    );
  }

  // Render error state
  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 3 }}>
        {error}
        <Box sx={{ mt: 2 }}>
          <Typography variant="body2">
            To enable code graph visualization:
          </Typography>
          <List dense>
            <ListItem>
              <ListItemText primary="• Enable AST Analysis in repository configuration" />
            </ListItem>
            <ListItem>
              <ListItemText primary="• Enable Graph Construction for relationship mapping" />
            </ListItem>
            <ListItem>
              <ListItemText primary="• Run a sync to process the code structure" />
            </ListItem>
          </List>
        </Box>
      </Alert>
    );
  }

  if (!codeGraph) {
    return (
      <Alert severity="info">
        No code graph data available. Enable AST analysis and graph construction to see code relationships.
      </Alert>
    );
  }

  return (
    <Box>
      {/* Header with controls */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Box>
          <Typography variant="h5" gutterBottom>
            Code Relationship Graph
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Interactive visualization of complete codebase structure and relationships
          </Typography>
        </Box>

        <Box display="flex" gap={1}>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Layout</InputLabel>
            <Select
              value={viewMode}
              label="Layout"
              onChange={(e) => setViewMode(e.target.value as any)}
            >
              <MenuItem value="hierarchical">Hierarchical</MenuItem>
              <MenuItem value="force">Force-directed</MenuItem>
              <MenuItem value="circular">Circular</MenuItem>
            </Select>
          </FormControl>

          <Tooltip title="Refresh Graph">
            <IconButton onClick={loadCodeGraph}>
              <Refresh />
            </IconButton>
          </Tooltip>

          <Tooltip title="Export Graph">
            <IconButton onClick={exportGraph}>
              <Download />
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      {/* Graph Statistics */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={6} sm={3}>
          <Card variant="outlined">
            <CardContent sx={{ py: 1.5 }}>
              <Typography variant="h6" color="primary">
                {filteredNodes.length}
              </Typography>
              <Typography variant="caption">
                Nodes ({codeGraph.metadata.totalNodes} total)
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={6} sm={3}>
          <Card variant="outlined">
            <CardContent sx={{ py: 1.5 }}>
              <Typography variant="h6" color="secondary">
                {filteredEdges.length}
              </Typography>
              <Typography variant="caption">
                Relationships ({codeGraph.metadata.totalEdges} total)
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={6} sm={3}>
          <Card variant="outlined">
            <CardContent sx={{ py: 1.5 }}>
              <Typography variant="h6" color="warning.main">
                {codeGraph.metadata.maxDepth}
              </Typography>
              <Typography variant="caption">
                Max Depth
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={6} sm={3}>
          <Card variant="outlined">
            <CardContent sx={{ py: 1.5 }}>
              <Typography variant="h6" color={codeGraph.metadata.orphanNodes.length > 0 ? 'error.main' : 'success.main'}>
                {codeGraph.metadata.orphanNodes.length}
              </Typography>
              <Typography variant="caption">
                Orphan Nodes
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Filters Panel */}
      <Accordion sx={{ mb: 3 }}>
        <AccordionSummary expandIcon={<ExpandMore />}>
          <Typography variant="h6">Graph Filters & Controls</Typography>
          <Chip 
            label={`${filteredNodes.length} nodes visible`} 
            size="small" 
            sx={{ ml: 2 }} 
          />
        </AccordionSummary>
        <AccordionDetails>
          <Grid container spacing={3}>
            {/* Search */}
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                size="small"
                placeholder="Search nodes..."
                value={filter.searchTerm}
                onChange={(e) => updateFilter('searchTerm', e.target.value)}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Search />
                    </InputAdornment>
                  )
                }}
              />
            </Grid>

            {/* Node Types */}
            <Grid item xs={12} md={4}>
              <FormControl fullWidth size="small">
                <InputLabel>Node Types</InputLabel>
                <Select
                  multiple
                  value={filter.nodeTypes}
                  label="Node Types"
                  onChange={(e) => updateFilter('nodeTypes', e.target.value)}
                  renderValue={(selected) => (
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                      {(selected as string[]).map((value) => (
                        <Chip 
                          key={value} 
                          label={value} 
                          size="small"
                          sx={{ 
                            backgroundColor: nodeTypeColors[value as keyof typeof nodeTypeColors],
                            color: 'white'
                          }}
                        />
                      ))}
                    </Box>
                  )}
                >
                  {Object.keys(nodeTypeColors).map((type) => (
                    <MenuItem key={type} value={type}>
                      <Box display="flex" alignItems="center" gap={1}>
                        {nodeTypeIcons[type as keyof typeof nodeTypeIcons]}
                        {type}
                      </Box>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            {/* Visibility */}
            <Grid item xs={12} md={4}>
              <FormControl fullWidth size="small">
                <InputLabel>Visibility</InputLabel>
                <Select
                  multiple
                  value={filter.visibilityLevels}
                  label="Visibility"
                  onChange={(e) => updateFilter('visibilityLevels', e.target.value)}
                >
                  <MenuItem value="public">Public</MenuItem>
                  <MenuItem value="protected">Protected</MenuItem>
                  <MenuItem value="internal">Internal</MenuItem>
                  <MenuItem value="private">Private</MenuItem>
                </Select>
              </FormControl>
            </Grid>

            {/* Complexity Range */}
            <Grid item xs={12} md={6}>
              <Typography variant="body2" gutterBottom>
                Complexity Range: {filter.complexityRange[0]} - {filter.complexityRange[1]}
              </Typography>
              <Slider
                value={filter.complexityRange}
                onChange={(_, value) => updateFilter('complexityRange', value)}
                valueLabelDisplay="auto"
                min={0}
                max={100}
                marks={[
                  { value: 0, label: '0' },
                  { value: 25, label: '25' },
                  { value: 50, label: '50' },
                  { value: 75, label: '75' },
                  { value: 100, label: '100+' }
                ]}
              />
            </Grid>

            {/* Options */}
            <Grid item xs={12} md={6}>
              <FormControlLabel
                control={
                  <Switch
                    checked={filter.showOnlyConnected}
                    onChange={(e) => updateFilter('showOnlyConnected', e.target.checked)}
                  />
                }
                label="Show Only Connected Nodes"
              />
              <FormControlLabel
                control={
                  <Switch
                    checked={filter.hidePrivate}
                    onChange={(e) => updateFilter('hidePrivate', e.target.checked)}
                  />
                }
                label="Hide Private Members"
              />
              <FormControlLabel
                control={
                  <Switch
                    checked={animationEnabled}
                    onChange={(e) => setAnimationEnabled(e.target.checked)}
                  />
                }
                label="Enable Animations"
              />
            </Grid>
          </Grid>
        </AccordionDetails>
      </Accordion>

      {/* Main Visualization Area */}
      <Paper sx={{ p: 2, mb: 3, minHeight: '600px', position: 'relative' }}>
        {/* Zoom Controls */}
        <Box sx={{ position: 'absolute', top: 16, right: 16, zIndex: 10 }}>
          <Box display="flex" flexDirection="column" gap={1}>
            <IconButton size="small" onClick={() => handleZoom(0.1)}>
              <ZoomIn />
            </IconButton>
            <Typography variant="caption" textAlign="center">
              {Math.round(zoomLevel * 100)}%
            </Typography>
            <IconButton size="small" onClick={() => handleZoom(-0.1)}>
              <ZoomOut />
            </IconButton>
            <IconButton size="small" onClick={() => setZoomLevel(1)}>
              <CenterFocusStrong />
            </IconButton>
          </Box>
        </Box>

        {/* Graph Visualization Container */}
        <Box 
          ref={containerRef}
          sx={{ 
            width: '100%', 
            height: '600px',
            overflow: 'hidden',
            border: '1px solid #e0e0e0',
            borderRadius: 1,
            position: 'relative'
          }}
        >
          {/* SVG for graph visualization */}
          <svg
            ref={svgRef}
            width="100%"
            height="100%"
            style={{ transform: `scale(${zoomLevel})` }}
          >
            {/* TODO: Implement actual D3.js graph rendering */}
            {/* For now, showing a placeholder representation */}
            <g transform="translate(50, 50)">
              {filteredNodes.map((node, index) => (
                <g key={node.id} transform={`translate(${(index % 10) * 80}, ${Math.floor(index / 10) * 60})`}>
                  <circle
                    r={Math.max(8, Math.min(20, node.metrics.linesOfCode / 10))}
                    fill={nodeTypeColors[node.type]}
                    stroke="#fff"
                    strokeWidth="2"
                    style={{ cursor: 'pointer' }}
                    onClick={() => handleNodeClick(node)}
                  />
                  <text
                    x="0"
                    y="25"
                    textAnchor="middle"
                    fontSize="10"
                    fill="#333"
                  >
                    {node.name.length > 8 ? `${node.name.slice(0, 8)}...` : node.name}
                  </text>
                </g>
              ))}
            </g>
          </svg>

          {/* Overlay message for placeholder */}
          <Box
            sx={{
              position: 'absolute',
              top: '50%',
              left: '50%',
              transform: 'translate(-50%, -50%)',
              textAlign: 'center',
              bgcolor: 'rgba(255, 255, 255, 0.9)',
              p: 3,
              borderRadius: 2,
              border: '1px solid #e0e0e0'
            }}
          >
            <AccountTree sx={{ fontSize: 48, color: 'primary.main', mb: 2 }} />
            <Typography variant="h6" gutterBottom>
              Interactive Graph Visualization
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Full D3.js implementation in progress. Currently showing placeholder layout.
            </Typography>
            <Button 
              variant="outlined" 
              startIcon={<PlayArrow />}
              sx={{ mt: 2 }}
              onClick={() => console.log('Demo mode:', { filteredNodes, filteredEdges })}
            >
              View Demo Data
            </Button>
          </Box>
        </Box>

        {/* Legend */}
        <Box sx={{ mt: 2, display: 'flex', flexWrap: 'wrap', gap: 1 }}>
          {Object.entries(nodeTypeColors).map(([type, color]) => (
            <Chip
              key={type}
              label={type}
              size="small"
              sx={{ backgroundColor: color, color: 'white' }}
              icon={nodeTypeIcons[type as keyof typeof nodeTypeIcons]}
            />
          ))}
        </Box>
      </Paper>

      {/* Warning about potential issues */}
      {codeGraph.metadata.circularDependencies.length > 0 && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          <Typography variant="body2">
            <strong>Circular Dependencies Detected:</strong> {codeGraph.metadata.circularDependencies.length} circular dependency cycles found. 
            This may indicate architectural issues that should be addressed.
          </Typography>
        </Alert>
      )}

      {codeGraph.metadata.orphanNodes.length > 0 && (
        <Alert severity="info" sx={{ mb: 2 }}>
          <Typography variant="body2">
            <strong>Orphan Nodes:</strong> {codeGraph.metadata.orphanNodes.length} nodes have no relationships. 
            Consider reviewing these for potential dead code or missing dependencies.
          </Typography>
        </Alert>
      )}

      {/* Node Details Dialog */}
      <Dialog
        open={showDetails}
        onClose={() => setShowDetails(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Box display="flex" alignItems="center" gap={1}>
            {selectedNode && nodeTypeIcons[selectedNode.type]}
            {selectedNode?.name || 'Node Details'}
            <Chip 
              label={selectedNode?.type} 
              size="small" 
              sx={{ 
                backgroundColor: selectedNode ? nodeTypeColors[selectedNode.type] : 'grey',
                color: 'white'
              }} 
            />
          </Box>
        </DialogTitle>

        <DialogContent>
          {selectedNode && (
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" gutterBottom>Basic Information</Typography>
                <List dense>
                  <ListItem>
                    <ListItemText 
                      primary="File Path" 
                      secondary={selectedNode.filePath}
                    />
                  </ListItem>
                  <ListItem>
                    <ListItemText 
                      primary="Line Number" 
                      secondary={selectedNode.lineNumber}
                    />
                  </ListItem>
                  <ListItem>
                    <ListItemText 
                      primary="Visibility" 
                      secondary={selectedNode.metadata.visibility}
                    />
                  </ListItem>
                  <ListItem>
                    <ListItemText 
                      primary="Level" 
                      secondary={`Depth ${selectedNode.level}`}
                    />
                  </ListItem>
                </List>
              </Grid>

              <Grid item xs={12} md={6}>
                <Typography variant="subtitle2" gutterBottom>Metrics</Typography>
                <List dense>
                  <ListItem>
                    <ListItemText 
                      primary="Lines of Code" 
                      secondary={selectedNode.metrics.linesOfCode}
                    />
                  </ListItem>
                  <ListItem>
                    <ListItemText 
                      primary="Cyclomatic Complexity" 
                      secondary={selectedNode.metrics.cyclomaticComplexity}
                    />
                  </ListItem>
                  <ListItem>
                    <ListItemText 
                      primary="Fan In / Fan Out" 
                      secondary={`${selectedNode.metrics.fanIn} / ${selectedNode.metrics.fanOut}`}
                    />
                  </ListItem>
                </List>
              </Grid>

              <Grid item xs={12}>
                <Typography variant="subtitle2" gutterBottom>
                  Related Nodes ({findRelatedNodes(selectedNode.id).length})
                </Typography>
                <Box display="flex" flexWrap="wrap" gap={1}>
                  {findRelatedNodes(selectedNode.id).slice(0, 10).map((relatedNode) => (
                    <Chip
                      key={relatedNode.id}
                      label={relatedNode.name}
                      size="small"
                      onClick={() => {
                        setSelectedNode(relatedNode);
                      }}
                      sx={{ 
                        backgroundColor: nodeTypeColors[relatedNode.type],
                        color: 'white',
                        cursor: 'pointer'
                      }}
                    />
                  ))}
                  {findRelatedNodes(selectedNode.id).length > 10 && (
                    <Chip label={`+${findRelatedNodes(selectedNode.id).length - 10} more`} size="small" />
                  )}
                </Box>
              </Grid>
            </Grid>
          )}
        </DialogContent>

        <DialogActions>
          <Button onClick={() => setShowDetails(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default CodeGraphVisualization;
