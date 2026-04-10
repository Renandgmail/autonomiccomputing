import React, { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Paper,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  TextField,
  InputAdornment,
  IconButton,
  Button,
  Switch,
  FormControlLabel,
  Chip,
  Alert,
  useTheme,
  useMediaQuery,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Drawer,
  Card,
  CardContent,
  Grid
} from '@mui/material';
import {
  Search as SearchIcon,
  ZoomIn,
  ZoomOut,
  CenterFocusStrong,
  GetApp,
  Close,
  Code,
  Class,
  Functions,
  AccountTree,
  Warning,
  Info,
  ArrowForward
} from '@mui/icons-material';
import ForceGraph2D from 'react-force-graph-2d';
import apiService from '../../services/apiService';

// Interfaces adapted from existing component but aligned to L3 spec
interface CodeNode {
  id: string;
  name: string;
  type: 'file' | 'class' | 'method' | 'function' | 'interface';
  filePath: string;
  complexity: number; // 0-10 scale
  quality: number; // 0-100 percentage
  dependenciesIn: number;
  dependenciesOut: number;
  lastChanged: string;
  issues: {
    critical: number;
    high: number;
    medium: number;
    low: number;
  };
  isOrphan?: boolean;
  isInCircularDep?: boolean;
}

interface CodeEdge {
  source: string;
  target: string;
  type: 'imports' | 'calls' | 'extends' | 'implements';
  isCircular?: boolean;
}

interface L3CodeGraphProps {
  repositoryId?: number;
}

type LayoutType = 'hierarchical' | 'force' | 'circular';

const L3CodeGraph: React.FC<L3CodeGraphProps> = ({ repositoryId: propRepositoryId }) => {
  const { repoId } = useParams<{ repoId: string }>();
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  
  const repositoryId = propRepositoryId || (repoId ? Number(repoId) : undefined);
  
  // Core state
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [nodes, setNodes] = useState<CodeNode[]>([]);
  const [edges, setEdges] = useState<CodeEdge[]>([]);
  
  // Controls state following L3_CODE_GRAPH.md spec
  const [layout, setLayout] = useState<LayoutType>('hierarchical');
  const [searchQuery, setSearchQuery] = useState('');
  const [fileTypeFilter, setFileTypeFilter] = useState<string>('all');
  const [showCircularDeps, setShowCircularDeps] = useState(false);
  const [showOrphans, setShowOrphans] = useState(false);
  
  // Node detail panel state
  const [selectedNode, setSelectedNode] = useState<CodeNode | null>(null);
  const [detailPanelOpen, setDetailPanelOpen] = useState(false);
  
  // Graph control refs
  const fgRef = useRef<any>();

  useEffect(() => {
    if (repositoryId) {
      loadCodeGraph();
    }
  }, [repositoryId]);

  const loadCodeGraph = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Load real data from backend API
      const data = await apiService.getCodeGraph(repositoryId!);
      
      // Handle backend response structure correctly
      if (data && Array.isArray(data.nodes)) {
        setNodes(data.nodes);
        setEdges(data.edges || []);
        console.log('✅ Code graph loaded successfully from backend', { 
          nodeCount: data.nodes.length, 
          edgeCount: (data.edges || []).length 
        });
      } else if (data && data.nodes && data.nodes.length === 0) {
        // Backend returned empty result - repository has no analyzable files
        setNodes([]);
        setEdges([]);
        setError('No code graph data available for this repository. Repository may be empty or analysis is pending.');
      } else {
        // Backend returned unexpected structure
        console.error('Unexpected backend response structure:', data);
        setError('Code graph data is not available in expected format. Please try again or contact support.');
        setNodes([]);
        setEdges([]);
      }
    } catch (err: any) {
      console.error('❌ Failed to load code graph from backend:', err);
      
      // Production-ready error handling - no mock data fallback
      if (err.response?.status === 404) {
        setError('Repository not found. Please check the repository ID.');
      } else if (err.response?.status === 500) {
        setError('Server error occurred while generating code graph. Please try again later.');
      } else if (err.message?.includes('timeout')) {
        setError('Code graph generation timed out. This may happen for large repositories. Please try again.');
      } else {
        setError(`Failed to load code graph: ${err.message || 'Unknown error occurred'}`);
      }
      
      setNodes([]);
      setEdges([]);
    } finally {
      setLoading(false);
    }
  };


  // Filter nodes based on search and file type
  const filteredNodes = nodes.filter(node => {
    const matchesSearch = searchQuery === '' || 
      node.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      node.filePath.toLowerCase().includes(searchQuery.toLowerCase());
    
    const matchesFileType = fileTypeFilter === 'all' || 
      node.filePath.includes(`.${fileTypeFilter}`);
    
    return matchesSearch && matchesFileType;
  });

  // Get filtered edges
  const filteredEdges = edges.filter(edge => 
    filteredNodes.find(n => n.id === edge.source) && 
    filteredNodes.find(n => n.id === edge.target)
  );

  // Handle node click - opens detail panel
  const handleNodeClick = (node: CodeNode) => {
    setSelectedNode(node);
    setDetailPanelOpen(true);
  };

  // Handle escape key to close detail panel
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && detailPanelOpen) {
        setDetailPanelOpen(false);
        setSelectedNode(null);
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [detailPanelOpen]);

  // Get node color based on type and overlays
  const getNodeColor = (node: CodeNode): string => {
    if (showOrphans && node.isOrphan) return theme.palette.grey[400];
    if (showCircularDeps && node.isInCircularDep) return theme.palette.error.main;
    
    switch (node.type) {
      case 'file': return theme.palette.primary.main;
      case 'class': return theme.palette.secondary.main;
      case 'method': return theme.palette.success.main;
      case 'function': return theme.palette.warning.main;
      case 'interface': return theme.palette.info.main;
      default: return theme.palette.grey[500];
    }
  };

  // Get link color - highlight circular dependencies
  const getLinkColor = (edge: CodeEdge): string => {
    if (showCircularDeps && edge.isCircular) return theme.palette.error.main;
    return theme.palette.grey[600];
  };

  // Export PNG functionality
  const handleExportPNG = () => {
    if (fgRef.current) {
      // This would capture the current canvas
      const canvas = fgRef.current.renderer().domElement;
      const link = document.createElement('a');
      link.download = `code-graph-${repositoryId}.png`;
      link.href = canvas.toDataURL();
      link.click();
    }
  };

  // Navigate to L4 File Detail
  const handleViewFileDetail = (node: CodeNode) => {
    navigate(`/repos/${repositoryId}/files/${encodeURIComponent(node.filePath)}`);
  };

  // Count overlays
  const circularDepCount = edges.filter(e => e.isCircular).length;
  const orphanCount = nodes.filter(n => n.isOrphan).length;

  // Mobile list view as per specification
  if (isMobile) {
    return (
      <Box sx={{ p: 2 }}>
        <Alert severity="info" sx={{ mb: 3 }}>
          Interactive graph requires a desktop browser.
        </Alert>
        
        <Typography variant="h6" gutterBottom>
          Repository Files
        </Typography>
        
        {/* Search for mobile */}
        <TextField
          fullWidth
          size="small"
          placeholder="Search files..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          InputProps={{
            startAdornment: <InputAdornment position="start"><SearchIcon /></InputAdornment>
          }}
          sx={{ mb: 2 }}
        />
        
        <List>
          {filteredNodes.map((node) => (
            <ListItem 
              key={node.id}
              onClick={() => handleViewFileDetail(node)}
              sx={{ cursor: 'pointer', '&:hover': { backgroundColor: 'action.hover' } }}
            >
              <ListItemIcon>
                <Code color="primary" />
              </ListItemIcon>
              <ListItemText
                primary={node.name}
                secondary={
                  <Box>
                    <Typography variant="body2">
                      Dependencies: {node.dependenciesIn} in, {node.dependenciesOut} out
                    </Typography>
                    <Typography variant="body2">
                      Complexity: {node.complexity}/10 • Quality: {node.quality}%
                    </Typography>
                  </Box>
                }
              />
              <ArrowForward />
            </ListItem>
          ))}
        </List>
      </Box>
    );
  }

  // Desktop graph view
  return (
    <Box sx={{ height: '100vh', display: 'flex', flexDirection: 'column' }}>
      {/* Controls Bar - following L3 specification layout */}
      <Paper sx={{ p: 2, borderRadius: 0 }}>
        <Box display="flex" alignItems="center" gap={2} flexWrap="wrap">
          {/* Layout Selector */}
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Layout</InputLabel>
            <Select value={layout} label="Layout" onChange={(e) => setLayout(e.target.value as LayoutType)}>
              <MenuItem value="hierarchical">Hierarchical</MenuItem>
              <MenuItem value="force">Force-directed</MenuItem>
              <MenuItem value="circular">Circular</MenuItem>
            </Select>
          </FormControl>

          {/* Filter */}
          <FormControl size="small" sx={{ minWidth: 100 }}>
            <InputLabel>Filter</InputLabel>
            <Select value={fileTypeFilter} label="Filter" onChange={(e) => setFileTypeFilter(e.target.value)}>
              <MenuItem value="all">All files</MenuItem>
              <MenuItem value="ts">TypeScript</MenuItem>
              <MenuItem value="js">JavaScript</MenuItem>
              <MenuItem value="cs">C#</MenuItem>
              <MenuItem value="py">Python</MenuItem>
            </Select>
          </FormControl>

          {/* Search */}
          <TextField
            size="small"
            placeholder="Search nodes..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            InputProps={{
              startAdornment: <InputAdornment position="start"><SearchIcon /></InputAdornment>
            }}
            sx={{ minWidth: 200 }}
          />

          {/* Issue Detection Overlays */}
          <FormControlLabel
            control={
              <Switch 
                checked={showCircularDeps}
                onChange={(e) => setShowCircularDeps(e.target.checked)}
                size="small"
              />
            }
            label={`⊙ Circular deps${circularDepCount > 0 ? ` (${circularDepCount})` : ''}`}
          />

          <FormControlLabel
            control={
              <Switch 
                checked={showOrphans}
                onChange={(e) => setShowOrphans(e.target.checked)}
                size="small"
              />
            }
            label={`⊙ Orphans${orphanCount > 0 ? ` (${orphanCount})` : ''}`}
          />

          {/* Graph Controls */}
          <Box display="flex" gap={1}>
            <IconButton size="small" onClick={() => fgRef.current?.zoom(fgRef.current.zoom() * 1.2)}>
              <ZoomIn />
            </IconButton>
            <IconButton size="small" onClick={() => fgRef.current?.zoom(fgRef.current.zoom() / 1.2)}>
              <ZoomOut />
            </IconButton>
            <IconButton size="small" onClick={() => fgRef.current?.centerAt()}>
              <CenterFocusStrong />
            </IconButton>
            <IconButton size="small" onClick={handleExportPNG}>
              <GetApp />
            </IconButton>
          </Box>
        </Box>

        {/* Active Filters Chips */}
        {(searchQuery || fileTypeFilter !== 'all' || showCircularDeps || showOrphans) && (
          <Box display="flex" gap={1} mt={2} flexWrap="wrap">
            {searchQuery && (
              <Chip size="small" label={`Search: ${searchQuery}`} onDelete={() => setSearchQuery('')} />
            )}
            {fileTypeFilter !== 'all' && (
              <Chip size="small" label={`Type: .${fileTypeFilter}`} onDelete={() => setFileTypeFilter('all')} />
            )}
            {showCircularDeps && (
              <Chip size="small" label={`${circularDepCount} circular dependencies detected`} color="error" />
            )}
            {showOrphans && (
              <Chip size="small" label={`${orphanCount} orphaned nodes detected`} color="warning" />
            )}
          </Box>
        )}
      </Paper>

      {/* Main Graph Area */}
      <Box sx={{ flexGrow: 1, position: 'relative', overflow: 'hidden' }}>
        {loading ? (
          <Box display="flex" alignItems="center" justifyContent="center" height="100%">
            <Typography>Loading code graph...</Typography>
          </Box>
        ) : error ? (
          <Box 
            display="flex" 
            flexDirection="column" 
            alignItems="center" 
            justifyContent="center" 
            height="100%"
            sx={{ p: 4, textAlign: 'center' }}
          >
            <Alert severity="error" sx={{ mb: 3, maxWidth: 600 }}>
              {error}
            </Alert>
            <Button 
              variant="outlined" 
              onClick={loadCodeGraph}
              sx={{ mt: 2 }}
            >
              Retry
            </Button>
          </Box>
        ) : nodes.length === 0 ? (
          <Box 
            display="flex" 
            flexDirection="column" 
            alignItems="center" 
            justifyContent="center" 
            height="100%"
            sx={{ p: 4, textAlign: 'center' }}
          >
            <Alert severity="info" sx={{ mb: 3, maxWidth: 600 }}>
              No code graph data available for this repository. 
              The repository may be empty or analysis may be pending.
            </Alert>
            <Button 
              variant="outlined" 
              onClick={loadCodeGraph}
              sx={{ mt: 2 }}
            >
              Check Again
            </Button>
          </Box>
        ) : (
          <ForceGraph2D
            ref={fgRef}
            width={window.innerWidth}
            height={window.innerHeight - 120}
            graphData={{
              nodes: filteredNodes,
              links: filteredEdges
            }}
            nodeLabel={(node: any) => `
              <div style="background: white; padding: 8px; border-radius: 4px; box-shadow: 0 2px 8px rgba(0,0,0,0.2); max-width: 250px;">
                <strong>${node.name}</strong><br/>
                <small>${node.filePath}</small><br/>
                Complexity: ${node.complexity}/10<br/>
                Quality: ${node.quality}%<br/>
                Deps in: ${node.dependenciesIn} | out: ${node.dependenciesOut}<br/>
                Last changed: ${node.lastChanged}
                ${node.issues.critical + node.issues.high > 0 ? '<br/><span style="color: red;">⚠ Issues detected</span>' : '<br/><span style="color: green;">✓ No issues</span>'}
              </div>
            `}
            nodeColor={getNodeColor}
            nodeRelSize={6}
            nodeVal={(node: any) => Math.max(4, node.complexity)}
            linkColor={getLinkColor}
            linkWidth={(link: any) => link.isCircular && showCircularDeps ? 3 : 1}
            linkDirectionalArrowLength={3}
            linkDirectionalArrowRelPos={1}
            onNodeClick={handleNodeClick}
            onBackgroundClick={() => {
              setDetailPanelOpen(false);
              setSelectedNode(null);
            }}
            enableZoomInteraction={true}
            enablePanInteraction={true}
            enableNodeDrag={true}
            d3AlphaDecay={0.02}
            d3VelocityDecay={0.3}
            cooldownTime={15000}
          />
        )}

        {/* Node Detail Panel */}
        <Drawer
          anchor="left"
          open={detailPanelOpen}
          onClose={() => setDetailPanelOpen(false)}
          variant="persistent"
          sx={{
            '& .MuiDrawer-paper': {
              width: 400,
              top: 64,
              height: 'calc(100vh - 64px)',
              boxSizing: 'border-box',
            },
          }}
        >
          {selectedNode && (
            <Box sx={{ p: 2 }}>
              <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                <Typography variant="h6">Node Details</Typography>
                <IconButton size="small" onClick={() => setDetailPanelOpen(false)}>
                  <Close />
                </IconButton>
              </Box>

              <Card sx={{ mb: 2 }}>
                <CardContent>
                  <Typography variant="subtitle1" gutterBottom>
                    {selectedNode.name}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    {selectedNode.filePath}
                  </Typography>
                  
                  <Grid container spacing={2} sx={{ mt: 1 }}>
                    <Grid item xs={6}>
                      <Typography variant="h4" color="warning.main">
                        {selectedNode.complexity}
                      </Typography>
                      <Typography variant="caption">Complexity (0–10)</Typography>
                    </Grid>
                    <Grid item xs={6}>
                      <Typography variant="h4" color="success.main">
                        {selectedNode.quality}%
                      </Typography>
                      <Typography variant="caption">Quality Score</Typography>
                    </Grid>
                    <Grid item xs={6}>
                      <Typography variant="h4" color="primary.main">
                        {selectedNode.dependenciesIn}
                      </Typography>
                      <Typography variant="caption">Deps in</Typography>
                    </Grid>
                    <Grid item xs={6}>
                      <Typography variant="h4" color="info.main">
                        {selectedNode.dependenciesOut}
                      </Typography>
                      <Typography variant="caption">Deps out</Typography>
                    </Grid>
                  </Grid>

                  <Typography variant="body2" sx={{ mt: 2 }}>
                    <strong>Last changed:</strong> {selectedNode.lastChanged}
                  </Typography>

                  {/* Issues */}
                  {selectedNode.issues.critical + selectedNode.issues.high + selectedNode.issues.medium + selectedNode.issues.low > 0 ? (
                    <Box sx={{ mt: 2 }}>
                      <Typography variant="subtitle2" gutterBottom>Active Issues:</Typography>
                      <Box display="flex" gap={1} flexWrap="wrap">
                        {selectedNode.issues.critical > 0 && (
                          <Chip size="small" label={`${selectedNode.issues.critical} Critical`} color="error" />
                        )}
                        {selectedNode.issues.high > 0 && (
                          <Chip size="small" label={`${selectedNode.issues.high} High`} color="warning" />
                        )}
                        {selectedNode.issues.medium > 0 && (
                          <Chip size="small" label={`${selectedNode.issues.medium} Medium`} color="info" />
                        )}
                        {selectedNode.issues.low > 0 && (
                          <Chip size="small" label={`${selectedNode.issues.low} Low`} color="default" />
                        )}
                      </Box>
                    </Box>
                  ) : (
                    <Alert severity="success" sx={{ mt: 2 }}>
                      No issues detected
                    </Alert>
                  )}

                  <Button
                    fullWidth
                    variant="contained"
                    startIcon={<ArrowForward />}
                    onClick={() => handleViewFileDetail(selectedNode)}
                    sx={{ mt: 2 }}
                  >
                    View File Detail
                  </Button>
                </CardContent>
              </Card>
            </Box>
          )}
        </Drawer>
      </Box>
    </Box>
  );
};

export default L3CodeGraph;
