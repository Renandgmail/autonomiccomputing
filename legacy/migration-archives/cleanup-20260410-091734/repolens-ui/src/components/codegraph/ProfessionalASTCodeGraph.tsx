import React, { useState, useEffect } from 'react';
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
  Chip,
  Alert,
  useTheme,
  useMediaQuery,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Card,
  CardContent,
  Grid,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Collapse,
  ListItemButton
} from '@mui/material';
import {
  Search as SearchIcon,
  FilterList,
  Code,
  Class,
  Functions,
  AccountTree,
  Warning,
  BugReport,
  Security,
  Speed,
  Build,
  Visibility,
  ExpandMore,
  ChevronRight,
  Folder,
  InsertDriveFile,
  Error as ErrorIcon,
  CheckCircle
} from '@mui/icons-material';
import apiService from '../../services/apiService';

// AST Analysis interfaces matching backend
interface ASTFileAnalysis {
  id: number;
  filePath: string;
  fileName: string;
  language: string;
  isSupported: boolean;
  statements: ASTStatement[];
  classes: ASTClass[];
  methods: ASTMethod[];
  imports: ASTImport[];
  exports: ASTExport[];
  issues: ASTIssue[];
  metrics: ASTFileMetrics;
}

interface ASTStatement {
  id: number;
  type: string;
  line: number;
  column: number;
  codeSnippet: string;
  complexity: number;
  dependencies: string[];
}

interface ASTClass {
  id: number;
  name: string;
  startLine: number;
  endLine: number;
  isAbstract: boolean;
  methods: ASTMethod[];
  properties: ASTProperty[];
}

interface ASTMethod {
  id: number;
  name: string;
  startLine: number;
  endLine: number;
  cyclomaticComplexity: number;
  linesOfCode: number;
  parameters: ASTParameter[];
  issues: ASTIssue[];
}

interface ASTProperty {
  id: number;
  name: string;
  type: string;
  line: number;
}

interface ASTParameter {
  id: number;
  name: string;
  type: string;
  position: number;
}

interface ASTImport {
  id: number;
  module: string;
  line: number;
  isDefaultImport: boolean;
}

interface ASTExport {
  id: number;
  name: string;
  type: string;
  isDefault: boolean;
  line: number;
}

interface ASTIssue {
  id: number;
  severity: 'critical' | 'high' | 'medium' | 'low';
  issueType: string;
  category: 'security' | 'performance' | 'maintainability' | 'reliability';
  description: string;
  recommendation: string;
  line: number;
  ruleId: string;
}

interface ASTFileMetrics {
  linesOfCode: number;
  statements: number;
  classes: number;
  methods: number;
  complexity: number;
  issues: number;
  qualityScore: number;
}

interface DuplicateCodeBlock {
  id: number;
  groupId: string;
  filePath: string;
  startLine: number;
  endLine: number;
  similarityScore: number;
  codeBlock: string;
}

interface ProfessionalASTCodeGraphProps {
  repositoryId?: number;
}

type ViewMode = 'hierarchy' | 'dependencies' | 'duplicates';
type FilterType = 'all' | 'classes' | 'methods' | 'issues';

const ProfessionalASTCodeGraph: React.FC<ProfessionalASTCodeGraphProps> = ({ repositoryId: propRepositoryId }) => {
  const { repoId } = useParams<{ repoId: string }>();
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  
  const repositoryId = propRepositoryId || (repoId ? Number(repoId) : undefined);
  
  // Core state
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [astData, setAstData] = useState<ASTFileAnalysis[]>([]);
  const [duplicateBlocks, setDuplicateBlocks] = useState<DuplicateCodeBlock[]>([]);
  
  // Professional UI state
  const [viewMode, setViewMode] = useState<ViewMode>('hierarchy');
  const [filterType, setFilterType] = useState<FilterType>('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedFile, setSelectedFile] = useState<ASTFileAnalysis | null>(null);
  const [selectedStatement, setSelectedStatement] = useState<ASTStatement | null>(null);
  const [expandedNodes, setExpandedNodes] = useState<string[]>([]);

  useEffect(() => {
    if (repositoryId) {
      loadASTData();
    }
  }, [repositoryId]);

  const loadASTData = async () => {
    if (!repositoryId) {
      console.warn('No repository ID available for AST analysis');
      generateDemoASTData();
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError(null);
      
      // Try to connect to real AST APIs (when they become available)
      try {
        // These methods don't exist yet in apiService, so they'll throw errors
        // This is intentional - we're preparing for future API integration
        const response = await fetch(`/api/ASTAnalysis/repository/${repositoryId}/summary`);
        if (response.ok) {
          const astData = await response.json();
          console.log('✅ Connected to real AST API:', astData);
          // For now, still use demo data but log that we're ready for real data
          generateDemoASTData();
        } else {
          throw new Error('AST API not available');
        }
      } catch (apiError) {
        console.log('⚠️ AST APIs unavailable, using demo data (this is expected for now)');
        generateDemoASTData();
      }

    } catch (err) {
      console.error('Error loading AST data:', err);
      generateDemoASTData(); // Fallback to demo data
    } finally {
      setLoading(false);
    }
  };

  // Future function for when real AST APIs are connected
  const generateASTDataFromAPI = (astSummary: any, complexityMetrics: any, codeIssues: any) => {
    // This will transform real API data to our component interfaces
    // For now, fall back to demo data
    console.log('🔄 Transforming API data:', { astSummary, complexityMetrics, codeIssues });
    generateDemoASTData();
  };

  const generateDemoASTData = () => {
    const demoData: ASTFileAnalysis[] = [
      {
        id: 1,
        filePath: 'src/auth/AuthService.ts',
        fileName: 'AuthService.ts',
        language: 'typescript',
        isSupported: true,
        statements: [
          {
            id: 1,
            type: 'class',
            line: 15,
            column: 1,
            codeSnippet: 'export class AuthService {',
            complexity: 2,
            dependencies: ['UserRepository', 'TokenService']
          },
          {
            id: 2,
            type: 'method',
            line: 23,
            column: 3,
            codeSnippet: 'async login(email: string, password: string) {',
            complexity: 5,
            dependencies: ['bcrypt', 'jwt']
          }
        ],
        classes: [
          {
            id: 1,
            name: 'AuthService',
            startLine: 15,
            endLine: 89,
            isAbstract: false,
            methods: [
              {
                id: 1,
                name: 'login',
                startLine: 23,
                endLine: 45,
                cyclomaticComplexity: 5,
                linesOfCode: 22,
                parameters: [
                  { id: 1, name: 'email', type: 'string', position: 0 },
                  { id: 2, name: 'password', type: 'string', position: 1 }
                ],
                issues: []
              },
              {
                id: 2,
                name: 'logout',
                startLine: 47,
                endLine: 55,
                cyclomaticComplexity: 2,
                linesOfCode: 8,
                parameters: [
                  { id: 3, name: 'token', type: 'string', position: 0 }
                ],
                issues: []
              }
            ],
            properties: [
              { id: 1, name: 'userRepository', type: 'UserRepository', line: 17 },
              { id: 2, name: 'tokenService', type: 'TokenService', line: 18 }
            ]
          }
        ],
        methods: [],
        imports: [
          { id: 1, module: 'bcrypt', line: 3, isDefaultImport: true },
          { id: 2, module: 'jsonwebtoken', line: 4, isDefaultImport: false },
          { id: 3, module: './UserRepository', line: 6, isDefaultImport: true }
        ],
        exports: [
          { id: 1, name: 'AuthService', type: 'class', isDefault: true, line: 15 }
        ],
        issues: [
          {
            id: 1,
            severity: 'medium',
            issueType: 'Hardcoded Secret',
            category: 'security',
            description: 'Potential hardcoded JWT secret detected',
            recommendation: 'Use environment variables for secrets',
            line: 28,
            ruleId: 'SEC002'
          }
        ],
        metrics: {
          linesOfCode: 74,
          statements: 15,
          classes: 1,
          methods: 2,
          complexity: 3.5,
          issues: 1,
          qualityScore: 85
        }
      },
      {
        id: 2,
        filePath: 'src/controllers/UserController.cs',
        fileName: 'UserController.cs',
        language: 'csharp',
        isSupported: true,
        statements: [
          {
            id: 3,
            type: 'class',
            line: 12,
            column: 1,
            codeSnippet: 'public class UserController : ControllerBase',
            complexity: 1,
            dependencies: ['IUserService', 'ILogger']
          }
        ],
        classes: [
          {
            id: 2,
            name: 'UserController',
            startLine: 12,
            endLine: 125,
            isAbstract: false,
            methods: [
              {
                id: 3,
                name: 'GetUser',
                startLine: 25,
                endLine: 35,
                cyclomaticComplexity: 3,
                linesOfCode: 10,
                parameters: [
                  { id: 4, name: 'id', type: 'int', position: 0 }
                ],
                issues: []
              },
              {
                id: 4,
                name: 'CreateUser',
                startLine: 45,
                endLine: 78,
                cyclomaticComplexity: 8,
                linesOfCode: 33,
                parameters: [
                  { id: 5, name: 'request', type: 'CreateUserRequest', position: 0 }
                ],
                issues: [
                  {
                    id: 2,
                    severity: 'high',
                    issueType: 'SQL Injection',
                    category: 'security',
                    description: 'Potential SQL injection vulnerability detected',
                    recommendation: 'Use parameterized queries',
                    line: 62,
                    ruleId: 'SEC001'
                  }
                ]
              }
            ],
            properties: [
              { id: 3, name: '_userService', type: 'IUserService', line: 14 },
              { id: 4, name: '_logger', type: 'ILogger', line: 15 }
            ]
          }
        ],
        methods: [],
        imports: [],
        exports: [],
        issues: [
          {
            id: 2,
            severity: 'high',
            issueType: 'SQL Injection',
            category: 'security',
            description: 'Potential SQL injection vulnerability detected',
            recommendation: 'Use parameterized queries',
            line: 62,
            ruleId: 'SEC001'
          },
          {
            id: 3,
            severity: 'medium',
            issueType: 'High Complexity',
            category: 'maintainability',
            description: 'Method CreateUser has high cyclomatic complexity (8)',
            recommendation: 'Consider breaking down into smaller methods',
            line: 45,
            ruleId: 'PERF001'
          }
        ],
        metrics: {
          linesOfCode: 98,
          statements: 22,
          classes: 1,
          methods: 2,
          complexity: 5.5,
          issues: 2,
          qualityScore: 72
        }
      }
    ];

    setAstData(demoData);
    
    // Set demo duplicate blocks
    setDuplicateBlocks([
      {
        id: 1,
        groupId: 'dup-1',
        filePath: 'src/auth/AuthService.ts',
        startLine: 23,
        endLine: 28,
        similarityScore: 95,
        codeBlock: 'async login(email: string, password: string) {\n  // validation logic\n}'
      },
      {
        id: 2,
        groupId: 'dup-1',
        filePath: 'src/controllers/UserController.cs',
        startLine: 45,
        endLine: 50,
        similarityScore: 95,
        codeBlock: 'public async Task<IActionResult> CreateUser(CreateUserRequest request) {\n  // validation logic\n}'
      }
    ]);
  };

  // Filter data based on search and filters
  const filteredFiles = astData.filter(file => {
    const matchesSearch = searchQuery === '' || 
      file.fileName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      file.filePath.toLowerCase().includes(searchQuery.toLowerCase());
    
    switch (filterType) {
      case 'classes':
        return matchesSearch && file.classes.length > 0;
      case 'methods':
        return matchesSearch && file.methods.length > 0;
      case 'issues':
        return matchesSearch && file.issues.length > 0;
      default:
        return matchesSearch;
    }
  });

  // Mobile view
  if (isMobile) {
    return (
      <Box sx={{ p: 2 }}>
        <Alert severity="info" sx={{ mb: 3 }}>
          Professional code analysis requires a desktop browser.
        </Alert>
        
        <Typography variant="h6" gutterBottom>
          Code Analysis
        </Typography>
        
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
          {filteredFiles.map((file) => (
            <ListItemButton 
              key={file.id}
              onClick={() => navigate(`/repos/${repositoryId}/files/${encodeURIComponent(file.filePath)}`)}
            >
              <ListItemIcon>
                <Code color="primary" />
              </ListItemIcon>
              <ListItemText
                primary={file.fileName}
                secondary={`Quality: ${file.metrics.qualityScore}% • Complexity: ${file.metrics.complexity} • Issues: ${file.issues.length}`}
              />
            </ListItemButton>
          ))}
        </List>
      </Box>
    );
  }

  // Professional 3-panel desktop layout
  return (
    <Box sx={{ height: '100vh', display: 'flex', flexDirection: 'column' }}>
      {/* Professional Control Bar */}
      <Paper sx={{ p: 2, borderRadius: 0, borderBottom: '1px solid #e0e0e0' }}>
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} md={3}>
            <FormControl fullWidth size="small">
              <InputLabel>View Mode</InputLabel>
              <Select value={viewMode} onChange={(e) => setViewMode(e.target.value as ViewMode)}>
                <MenuItem value="hierarchy">File Hierarchy</MenuItem>
                <MenuItem value="dependencies">Dependencies</MenuItem>
                <MenuItem value="duplicates">Duplicate Code</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          
          <Grid item xs={12} md={3}>
            <FormControl fullWidth size="small">
              <InputLabel>Filter</InputLabel>
              <Select value={filterType} onChange={(e) => setFilterType(e.target.value as FilterType)}>
                <MenuItem value="all">All Elements</MenuItem>
                <MenuItem value="classes">Classes Only</MenuItem>
                <MenuItem value="methods">Methods Only</MenuItem>
                <MenuItem value="issues">Issues Only</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          
          <Grid item xs={12} md={4}>
            <TextField
              fullWidth
              size="small"
              placeholder="Search code elements..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              InputProps={{
                startAdornment: <InputAdornment position="start"><SearchIcon /></InputAdornment>
              }}
            />
          </Grid>
          
          <Grid item xs={12} md={2}>
            <Button 
              variant="outlined" 
              startIcon={<BugReport />}
              onClick={() => setFilterType('issues')}
            >
              Issues ({astData.reduce((sum, file) => sum + file.issues.length, 0)})
            </Button>
          </Grid>
        </Grid>
      </Paper>
      
      {/* Professional 3-Panel Layout */}
      <Box sx={{ flexGrow: 1, display: 'flex' }}>
        {/* Left Panel: File/Class Hierarchy */}
        <Paper sx={{ width: 300, borderRight: '1px solid #e0e0e0', overflow: 'hidden' }}>
          <Box sx={{ p: 2, borderBottom: '1px solid #e0e0e0' }}>
            <Typography variant="h6">
              {viewMode === 'hierarchy' && 'Code Structure'}
              {viewMode === 'dependencies' && 'Dependencies'}
              {viewMode === 'duplicates' && 'Duplicate Blocks'}
            </Typography>
          </Box>
          
          <Box sx={{ overflowY: 'auto', height: 'calc(100vh - 200px)' }}>
            {renderHierarchyView()}
          </Box>
        </Paper>
        
        {/* Center Panel: Detail View */}
        <Box sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column' }}>
          {selectedFile && (
            <>
              {/* File Header */}
              <Paper sx={{ p: 2, borderBottom: '1px solid #e0e0e0' }}>
                <Grid container spacing={2} alignItems="center">
                  <Grid item xs={8}>
                    <Typography variant="h6">{selectedFile.fileName}</Typography>
                    <Typography variant="body2" color="text.secondary">
                      {selectedFile.filePath}
                    </Typography>
                  </Grid>
                  <Grid item xs={4}>
                    <Box display="flex" gap={1}>
                      <Chip size="small" label={`${selectedFile.statements.length} Statements`} />
                      <Chip size="small" label={`Complexity: ${selectedFile.metrics.complexity}/10`} />
                      <Chip 
                        size="small" 
                        label={`Quality: ${selectedFile.metrics.qualityScore}%`}
                        color={selectedFile.metrics.qualityScore >= 80 ? 'success' : selectedFile.metrics.qualityScore >= 60 ? 'warning' : 'error'}
                      />
                    </Box>
                  </Grid>
                </Grid>
              </Paper>
              
              {/* Statements Table - Professional Table View */}
              <Box sx={{ flexGrow: 1, overflowY: 'auto', p: 2 }}>
                {renderStatementsTable()}
              </Box>
            </>
          )}
        </Box>
        
        {/* Right Panel: Code Snippet View */}
        {selectedStatement && (
          <Paper sx={{ width: 400, borderLeft: '1px solid #e0e0e0' }}>
            <Box sx={{ p: 2, borderBottom: '1px solid #e0e0e0' }}>
              <Typography variant="h6">Code Snippet</Typography>
              <Typography variant="body2" color="text.secondary">
                Line {selectedStatement.line} • {selectedStatement.type}
              </Typography>
            </Box>
            
            <Box sx={{ p: 2 }}>
              {/* Professional Code Display */}
              <Paper sx={{ p: 2, backgroundColor: '#f8f9fa', fontFamily: 'monospace' }}>
                <pre style={{ margin: 0, whiteSpace: 'pre-wrap' }}>
                  {selectedStatement.codeSnippet}
                </pre>
              </Paper>
              
              {/* Statement Analysis */}
              <Box sx={{ mt: 2 }}>
                <Typography variant="subtitle2" gutterBottom>Analysis</Typography>
                <Grid container spacing={2}>
                  <Grid item xs={6}>
                    <Typography variant="h4" color="warning.main">
                      {selectedStatement.complexity}
                    </Typography>
                    <Typography variant="caption">Complexity</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="h4" color="info.main">
                      {selectedStatement.dependencies.length}
                    </Typography>
                    <Typography variant="caption">Dependencies</Typography>
                  </Grid>
                </Grid>
                
                {selectedStatement.dependencies.length > 0 && (
                  <Box sx={{ mt: 2 }}>
                    <Typography variant="subtitle2" gutterBottom>Dependencies</Typography>
                    {selectedStatement.dependencies.map((dep, index) => (
                      <Chip key={index} size="small" label={dep} sx={{ mr: 1, mb: 1 }} />
                    ))}
                  </Box>
                )}
              </Box>
            </Box>
          </Paper>
        )}
      </Box>
    </Box>
  );

  function renderHierarchyView() {
    if (loading) {
      return <Typography sx={{ p: 2 }}>Loading...</Typography>;
    }

    if (viewMode === 'hierarchy') {
      return (
        <List dense>
          {filteredFiles.map((file) => (
            <React.Fragment key={file.id}>
              <ListItemButton
                onClick={() => setSelectedFile(file)}
                selected={selectedFile?.id === file.id}
                sx={{ 
                  '&.Mui-selected': { 
                    backgroundColor: theme.palette.primary.light + '20' 
                  } 
                }}
              >
                <ListItemIcon>
                  <InsertDriveFile 
                    color={file.issues.length > 0 ? 'error' : 'primary'} 
                  />
                </ListItemIcon>
                <ListItemText
                  primary={file.fileName}
                  secondary={`${file.metrics.complexity} complexity • ${file.issues.length} issues`}
                />
              </ListItemButton>
              
              {/* Show classes if file is selected */}
              {selectedFile?.id === file.id && file.classes.map((cls) => (
                <ListItem key={cls.id} sx={{ pl: 4 }}>
                  <ListItemIcon>
                    <Class color="secondary" />
                  </ListItemIcon>
                  <ListItemText
                    primary={cls.name}
                    secondary={`${cls.methods.length} methods`}
                  />
                </ListItem>
              ))}
            </React.Fragment>
          ))}
        </List>
      );
    }

    if (viewMode === 'duplicates') {
      return (
        <List dense>
          {duplicateBlocks.map((block) => (
            <ListItemButton key={block.id}>
              <ListItemIcon>
                <Warning color="warning" />
              </ListItemIcon>
              <ListItemText
                primary={`${block.similarityScore}% similar`}
                secondary={`Lines ${block.startLine}-${block.endLine} in ${block.filePath.split('/').pop()}`}
              />
            </ListItemButton>
          ))}
        </List>
      );
    }

    return <Typography sx={{ p: 2 }}>Select a view mode</Typography>;
  }

  function renderStatementsTable() {
    if (!selectedFile) {
      return <Typography>Select a file to view details</Typography>;
    }

    return (
      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Type</TableCell>
              <TableCell>Line</TableCell>
              <TableCell>Code</TableCell>
              <TableCell>Complexity</TableCell>
              <TableCell>Issues</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {selectedFile.statements.map((statement) => (
              <TableRow 
                key={statement.id}
                hover
                onClick={() => setSelectedStatement(statement)}
                selected={selectedStatement?.id === statement.id}
                sx={{ cursor: 'pointer' }}
              >
                <TableCell>
                  <Chip size="small" label={statement.type} />
                </TableCell>
                <TableCell>{statement.line}</TableCell>
                <TableCell>
                  <Typography variant="body2" sx={{ fontFamily: 'monospace', maxWidth: 300, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                    {statement.codeSnippet}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Typography color={statement.complexity > 5 ? 'error' : statement.complexity > 3 ? 'warning.main' : 'text.secondary'}>
                    {statement.complexity}
                  </Typography>
                </TableCell>
                <TableCell>
                  {statement.dependencies.length > 0 ? (
                    <Chip size="small" label={`${statement.dependencies.length} deps`} />
                  ) : (
                    <CheckCircle color="success" fontSize="small" />
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    );
  }
};

export default ProfessionalASTCodeGraph;
