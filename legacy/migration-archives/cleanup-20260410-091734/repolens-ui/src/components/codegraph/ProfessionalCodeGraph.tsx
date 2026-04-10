import React, { useState, useEffect, useMemo } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  IconButton,
  Tooltip,
  Tabs,
  Tab,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Collapse,
  Alert,
  Button,
  TextField,
  InputAdornment,
} from '@mui/material';
import {
  ExpandLess,
  ExpandMore,
  Code,
  Security,
  Speed,
  BugReport,
  Info,
  Search,
  FilePresent,
  AccountTree,
  ContentCopy,
  Error,
  Warning,
  CheckCircle,
} from '@mui/icons-material';
import { useTheme } from '@mui/material/styles';

interface ASTNode {
  id: string;
  name: string;
  type: 'file' | 'class' | 'method' | 'statement';
  filePath: string;
  line?: number;
  column?: number;
  complexity?: number;
  issues: ASTIssue[];
  children?: ASTNode[];
  codeSnippet?: string;
  dependencies: string[];
}

interface ASTIssue {
  id: string;
  severity: 'critical' | 'high' | 'medium' | 'low';
  category: 'security' | 'performance' | 'maintainability' | 'reliability';
  description: string;
  recommendation: string;
  line: number;
  column?: number;
  ruleId: string;
}

interface DuplicateCodeBlock {
  id: string;
  groupId: string;
  filePath: string;
  startLine: number;
  endLine: number;
  similarityScore: number;
  codeBlock: string;
  duplicateType: 'exact' | 'similar' | 'semantic';
}

interface CodeGraphData {
  files: ASTNode[];
  dependencies: Array<{
    source: string;
    target: string;
    type: string;
    weight: number;
  }>;
  duplicates: DuplicateCodeBlock[];
  metrics: {
    totalFiles: number;
    totalClasses: number;
    totalMethods: number;
    averageComplexity: number;
    totalIssues: number;
    criticalIssues: number;
    duplicatedLines: number;
  };
}

interface ProfessionalCodeGraphProps {
  repositoryId: number;
  data?: CodeGraphData;
  loading?: boolean;
  onNodeClick?: (node: ASTNode) => void;
  onIssueClick?: (issue: ASTIssue) => void;
}

const ProfessionalCodeGraph: React.FC<ProfessionalCodeGraphProps> = ({
  repositoryId,
  data,
  loading = false,
  onNodeClick,
  onIssueClick,
}) => {
  const theme = useTheme();
  const [selectedTab, setSelectedTab] = useState(0);
  const [selectedFile, setSelectedFile] = useState<ASTNode | null>(null);
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(new Set());
  const [searchTerm, setSearchTerm] = useState('');
  const [severityFilter, setSeverityFilter] = useState<string>('all');

  // Filter and sort data based on search and filters
  const filteredData = useMemo(() => {
    if (!data) return { files: [], issues: [], duplicates: [] };

    let files = data.files;
    
    // Apply search filter
    if (searchTerm) {
      files = files.filter(file => 
        file.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        file.filePath.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Collect all issues from files
    const allIssues: (ASTIssue & { filePath: string })[] = [];
    files.forEach(file => {
      file.issues.forEach(issue => {
        allIssues.push({ ...issue, filePath: file.filePath });
      });
    });

    // Apply severity filter to issues
    const filteredIssues = severityFilter === 'all' 
      ? allIssues 
      : allIssues.filter(issue => issue.severity === severityFilter);

    // Sort issues by severity priority
    const severityOrder = { critical: 4, high: 3, medium: 2, low: 1 };
    filteredIssues.sort((a, b) => 
      (severityOrder[b.severity] || 0) - (severityOrder[a.severity] || 0)
    );

    return {
      files: files.sort((a, b) => b.issues.length - a.issues.length),
      issues: filteredIssues,
      duplicates: data.duplicates.sort((a, b) => b.similarityScore - a.similarityScore),
    };
  }, [data, searchTerm, severityFilter]);

  const handleNodeToggle = (nodeId: string) => {
    setExpandedNodes(prev => {
      const newSet = new Set(prev);
      if (newSet.has(nodeId)) {
        newSet.delete(nodeId);
      } else {
        newSet.add(nodeId);
      }
      return newSet;
    });
  };

  const handleFileSelect = (file: ASTNode) => {
    setSelectedFile(file);
    onNodeClick?.(file);
  };

  const getSeverityColor = (severity: string) => {
    const colors = {
      critical: theme.palette.error.main,
      high: theme.palette.warning.main,
      medium: theme.palette.info.main,
      low: theme.palette.success.main,
    };
    return colors[severity as keyof typeof colors] || theme.palette.grey[500];
  };

  const getSeverityIcon = (severity: string) => {
    const icons = {
      critical: <Error />,
      high: <Warning />,
      medium: <Info />,
      low: <CheckCircle />,
    };
    return icons[severity as keyof typeof icons] || <Info />;
  };

  const renderFileHierarchy = () => (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
          <AccountTree sx={{ mr: 1 }} />
          <Typography variant="h6">
            File Hierarchy ({filteredData.files.length} files)
          </Typography>
        </Box>
        
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
            ),
          }}
          sx={{ mb: 2 }}
        />

        <Box sx={{ maxHeight: 400, overflow: 'auto' }}>
          <List dense>
            {filteredData.files.map((file) => (
              <Box key={file.id}>
                <ListItem
                  onClick={() => handleFileSelect(file)}
                  sx={{
                    borderLeft: file.issues.length > 0 
                      ? `4px solid ${getSeverityColor(file.issues[0]?.severity || 'low')}` 
                      : 'none',
                    cursor: 'pointer',
                    backgroundColor: selectedFile?.id === file.id ? 'action.selected' : 'transparent',
                    '&:hover': {
                      backgroundColor: 'action.hover',
                    },
                  }}
                >
                  <ListItemIcon>
                    <FilePresent color={file.issues.length > 0 ? 'error' : 'inherit'} />
                  </ListItemIcon>
                  <ListItemText
                    primary={file.name}
                    secondary={
                      <Box>
                        <Typography variant="caption" display="block">
                          {file.filePath}
                        </Typography>
                        {file.issues.length > 0 && (
                          <Box sx={{ display: 'flex', gap: 0.5, mt: 0.5 }}>
                            <Chip
                              size="small"
                              label={`${file.issues.length} issues`}
                              color="error"
                              variant="outlined"
                            />
                            {file.complexity && (
                              <Chip
                                size="small"
                                label={`Complexity: ${file.complexity}`}
                                color={file.complexity > 10 ? 'error' : 'default'}
                                variant="outlined"
                              />
                            )}
                          </Box>
                        )}
                      </Box>
                    }
                  />
                  <IconButton
                    size="small"
                    onClick={(e) => {
                      e.stopPropagation();
                      handleNodeToggle(file.id);
                    }}
                  >
                    {expandedNodes.has(file.id) ? <ExpandLess /> : <ExpandMore />}
                  </IconButton>
                </ListItem>
                
                <Collapse in={expandedNodes.has(file.id)}>
                  <List component="div" disablePadding>
                    {file.children?.map((child) => (
                      <ListItem 
                        key={child.id} 
                        onClick={() => onNodeClick?.(child)}
                        sx={{ 
                          pl: 4,
                          cursor: 'pointer',
                          '&:hover': {
                            backgroundColor: 'action.hover',
                          },
                        }}
                      >
                        <ListItemIcon>
                          <Code fontSize="small" />
                        </ListItemIcon>
                        <ListItemText
                          primary={`${child.type}: ${child.name}`}
                          secondary={child.line ? `Line ${child.line}` : undefined}
                        />
                        {child.issues.length > 0 && (
                          <Chip
                            size="small"
                            label={child.issues.length}
                            color="error"
                            sx={{ minWidth: 'auto' }}
                          />
                        )}
                      </ListItem>
                    ))}
                  </List>
                </Collapse>
              </Box>
            ))}
          </List>
        </Box>
      </CardContent>
    </Card>
  );

  const renderIssuesPanel = () => (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <BugReport sx={{ mr: 1 }} />
            <Typography variant="h6">
              Issues ({filteredData.issues.length})
            </Typography>
          </Box>
          
          <Box sx={{ display: 'flex', gap: 1 }}>
            {['all', 'critical', 'high', 'medium', 'low'].map((severity) => (
              <Button
                key={severity}
                size="small"
                variant={severityFilter === severity ? 'contained' : 'outlined'}
                onClick={() => setSeverityFilter(severity)}
                sx={{
                  minWidth: 'auto',
                  ...(severity !== 'all' && {
                    borderColor: getSeverityColor(severity),
                    color: severityFilter === severity ? 'white' : getSeverityColor(severity),
                    backgroundColor: severityFilter === severity ? getSeverityColor(severity) : 'transparent',
                  }),
                }}
              >
                {severity}
              </Button>
            ))}
          </Box>
        </Box>

        <TableContainer component={Paper} sx={{ maxHeight: 400 }}>
          <Table size="small" stickyHeader>
            <TableHead>
              <TableRow>
                <TableCell>Severity</TableCell>
                <TableCell>Category</TableCell>
                <TableCell>Description</TableCell>
                <TableCell>File</TableCell>
                <TableCell>Line</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredData.issues.map((issue) => (
                <TableRow key={issue.id} hover>
                  <TableCell>
                    <Chip
                      icon={getSeverityIcon(issue.severity)}
                      label={issue.severity}
                      size="small"
                      sx={{
                        backgroundColor: getSeverityColor(issue.severity),
                        color: 'white',
                      }}
                    />
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={issue.category}
                      size="small"
                      variant="outlined"
                      color={issue.category === 'security' ? 'error' : 'default'}
                    />
                  </TableCell>
                  <TableCell>
                    <Tooltip title={issue.recommendation}>
                      <Typography variant="body2" noWrap sx={{ maxWidth: 200 }}>
                        {issue.description}
                      </Typography>
                    </Tooltip>
                  </TableCell>
                  <TableCell>
                    <Typography variant="caption">
                      {issue.filePath.split('/').pop()}
                    </Typography>
                  </TableCell>
                  <TableCell>{issue.line}</TableCell>
                  <TableCell>
                    <Tooltip title="View Details">
                      <IconButton size="small" onClick={() => onIssueClick?.(issue)}>
                        <Info />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </CardContent>
    </Card>
  );

  const renderDuplicatesPanel = () => (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
          <ContentCopy sx={{ mr: 1 }} />
          <Typography variant="h6">
            Duplicate Code ({filteredData.duplicates.length} blocks)
          </Typography>
        </Box>

        <TableContainer component={Paper} sx={{ maxHeight: 400 }}>
          <Table size="small" stickyHeader>
            <TableHead>
              <TableRow>
                <TableCell>Similarity</TableCell>
                <TableCell>Type</TableCell>
                <TableCell>File</TableCell>
                <TableCell>Lines</TableCell>
                <TableCell>LOC</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredData.duplicates.map((duplicate) => (
                <TableRow key={duplicate.id} hover>
                  <TableCell>
                    <Chip
                      label={`${duplicate.similarityScore}%`}
                      size="small"
                      color={duplicate.similarityScore > 90 ? 'error' : duplicate.similarityScore > 70 ? 'warning' : 'default'}
                    />
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={duplicate.duplicateType}
                      size="small"
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell>
                    <Typography variant="caption">
                      {duplicate.filePath.split('/').pop()}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    {duplicate.startLine}-{duplicate.endLine}
                  </TableCell>
                  <TableCell>
                    {duplicate.endLine - duplicate.startLine + 1}
                  </TableCell>
                  <TableCell>
                    <Tooltip title="View Code">
                      <IconButton size="small">
                        <Code />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </CardContent>
    </Card>
  );

  const renderCodeSnippet = () => (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'between', mb: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <Code sx={{ mr: 1 }} />
            <Typography variant="h6">
              {selectedFile ? selectedFile.name : 'Code Snippet'}
            </Typography>
          </Box>
          {selectedFile && (
            <Typography variant="caption" color="textSecondary">
              {selectedFile.filePath}
            </Typography>
          )}
        </Box>

        {selectedFile && selectedFile.codeSnippet ? (
          <Paper
            sx={{
              p: 2,
              backgroundColor: theme.palette.mode === 'dark' ? '#1e1e1e' : '#f5f5f5',
              fontFamily: 'monospace',
              fontSize: '0.875rem',
              overflow: 'auto',
              maxHeight: 350,
            }}
          >
            <pre style={{ margin: 0, whiteSpace: 'pre-wrap' }}>
              {selectedFile.codeSnippet}
            </pre>
          </Paper>
        ) : (
          <Alert severity="info">
            Select a file from the hierarchy to view its code snippet
          </Alert>
        )}

        {selectedFile && selectedFile.issues.length > 0 && (
          <Box sx={{ mt: 2 }}>
            <Typography variant="subtitle2" sx={{ mb: 1 }}>
              Issues in this file:
            </Typography>
            {selectedFile.issues.slice(0, 3).map((issue) => (
              <Alert
                key={issue.id}
                severity={issue.severity === 'critical' || issue.severity === 'high' ? 'error' : 'warning'}
                sx={{ mb: 1 }}
                action={
                  <Button size="small" onClick={() => onIssueClick?.(issue)}>
                    Fix
                  </Button>
                }
              >
                <strong>Line {issue.line}:</strong> {issue.description}
              </Alert>
            ))}
            {selectedFile.issues.length > 3 && (
              <Typography variant="caption" color="textSecondary">
                +{selectedFile.issues.length - 3} more issues
              </Typography>
            )}
          </Box>
        )}
      </CardContent>
    </Card>
  );

  if (loading) {
    return (
      <Box sx={{ p: 3, textAlign: 'center' }}>
        <Typography>Loading code analysis...</Typography>
      </Box>
    );
  }

  if (!data) {
    return (
      <Alert severity="info">
        No AST analysis data available for this repository. Run code analysis to generate insights.
      </Alert>
    );
  }

  return (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Metrics Overview */}
      <Grid container spacing={2} sx={{ mb: 2 }}>
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h4" color="primary">
                {data.metrics.totalFiles}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Files Analyzed
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h4" color="error">
                {data.metrics.criticalIssues}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Critical Issues
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h4" color="warning.main">
                {data.metrics.averageComplexity.toFixed(1)}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Avg Complexity
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h4" color="info.main">
                {data.metrics.duplicatedLines}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Duplicate Lines
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* 3-Panel Layout */}
      <Grid container spacing={2} sx={{ flexGrow: 1 }}>
        {/* Left Panel - File Hierarchy */}
        <Grid item xs={12} md={4}>
          {renderFileHierarchy()}
        </Grid>

        {/* Center Panel - Issues/Duplicates */}
        <Grid item xs={12} md={4}>
          <Box sx={{ height: '100%' }}>
            <Tabs
              value={selectedTab}
              onChange={(_, newValue) => setSelectedTab(newValue)}
              sx={{ mb: 1 }}
            >
              <Tab label="Issues" />
              <Tab label="Duplicates" />
            </Tabs>
            {selectedTab === 0 ? renderIssuesPanel() : renderDuplicatesPanel()}
          </Box>
        </Grid>

        {/* Right Panel - Code Snippet */}
        <Grid item xs={12} md={4}>
          {renderCodeSnippet()}
        </Grid>
      </Grid>
    </Box>
  );
};

export default ProfessionalCodeGraph;
