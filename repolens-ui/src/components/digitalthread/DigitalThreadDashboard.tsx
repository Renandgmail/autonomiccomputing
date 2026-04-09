import React, { useState, useEffect } from 'react';
import {
  Box,
  Paper,
  Typography,
  Grid,
  Card,
  CardContent,
  CardHeader,
  Button,
  Chip,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  LinearProgress,
  Divider,
  IconButton,
  Tooltip
} from '@mui/material';
import {
  Timeline,
  CallSplit,
  Science,
  Link,
  BugReport,
  Code,
  Security,
  Speed,
  Assessment,
  Refresh,
  Launch,
  Warning,
  CheckCircle,
  Error,
  Info
} from '@mui/icons-material';
import apiService from '../../services/apiService';

interface DigitalThreadDashboardProps {
  repositoryId: number;
  repositoryName: string;
}

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
      id={`digital-thread-tabpanel-${index}`}
      aria-labelledby={`digital-thread-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

const DigitalThreadDashboard: React.FC<DigitalThreadDashboardProps> = ({
  repositoryId,
  repositoryName
}) => {
  const [activeTab, setActiveTab] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Digital Thread Data States
  const [branchData, setBranchData] = useState<any>(null);
  const [uiElements, setUIElements] = useState<any>(null);
  const [testCoverage, setTestCoverage] = useState<any>(null);
  const [traceabilityMatrix, setTraceabilityMatrix] = useState<any>(null);
  const [digitalThread, setDigitalThread] = useState<any>(null);
  const [automationGaps, setAutomationGaps] = useState<any>(null);

  useEffect(() => {
    loadDigitalThreadData();
  }, [repositoryId]);

  const loadDigitalThreadData = async () => {
    setLoading(true);
    setError(null);
    
    try {
      console.log('🧵 Loading Digital Thread data for repository:', repositoryId);
      
      // Get complete digital thread analysis
      const analysis = await apiService.getCompleteDigitalThreadAnalysis(repositoryId);
      
      setDigitalThread(analysis.digitalThread);
      setTraceabilityMatrix(analysis.traceabilityMatrix);
      setTestCoverage(analysis.testCoverage);
      setAutomationGaps(analysis.automationGaps);
      
      console.log('✅ Digital Thread data loaded successfully', analysis);
      
    } catch (err: any) {
      console.error('❌ Failed to load Digital Thread data:', err);
      setError(err.message || 'Failed to load Digital Thread data');
    } finally {
      setLoading(false);
    }
  };

  const loadBranchData = async () => {
    try {
      const branches = await apiService.getAvailableBranches(repositoryId);
      setBranchData(branches);
    } catch (err: any) {
      console.warn('Failed to load branch data:', err.message);
    }
  };

  const loadUIElementData = async () => {
    try {
      const elements = await apiService.getUIElements(repositoryId);
      setUIElements(elements);
    } catch (err: any) {
      console.warn('Failed to load UI element data:', err.message);
    }
  };

  const handleRefresh = () => {
    loadDigitalThreadData();
    if (activeTab === 1) loadBranchData();
    if (activeTab === 2) loadUIElementData();
  };

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
    
    // Load tab-specific data on demand
    if (newValue === 1 && !branchData) {
      loadBranchData();
    }
    if (newValue === 2 && !uiElements) {
      loadUIElementData();
    }
  };

  const renderOverviewTab = () => (
    <Grid container spacing={3}>
      {/* Digital Thread Summary */}
      <Grid item xs={12}>
        <Card>
          <CardHeader 
            title="🧵 Digital Thread Overview"
            subheader={`Complete SDLC traceability for ${repositoryName}`}
            action={
              <Tooltip title="Refresh Data">
                <IconButton onClick={handleRefresh} disabled={loading}>
                  <Refresh />
                </IconButton>
              </Tooltip>
            }
          />
          <CardContent>
            {traceabilityMatrix?.summary && (
              <Grid container spacing={2}>
                <Grid item xs={12} md={3}>
                  <Box textAlign="center">
                    <Typography variant="h4" color="primary">
                      {traceabilityMatrix.summary.totalRequirements || 0}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Requirements
                    </Typography>
                  </Box>
                </Grid>
                <Grid item xs={12} md={3}>
                  <Box textAlign="center">
                    <Typography variant="h4" color="secondary">
                      {traceabilityMatrix.summary.totalTestCases || 0}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Test Cases
                    </Typography>
                  </Box>
                </Grid>
                <Grid item xs={12} md={3}>
                  <Box textAlign="center">
                    <Typography variant="h4" color="info.main">
                      {traceabilityMatrix.summary.totalCodeFiles || 0}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Code Files
                    </Typography>
                  </Box>
                </Grid>
                <Grid item xs={12} md={3}>
                  <Box textAlign="center">
                    <Typography variant="h4" color="success.main">
                      {Math.round((traceabilityMatrix.summary.overallTraceability || 0) * 100)}%
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Traceability
                    </Typography>
                  </Box>
                </Grid>
              </Grid>
            )}
            
            {/* Progress Indicators */}
            {testCoverage?.statistics && (
              <Box mt={3}>
                <Typography variant="body2" gutterBottom>
                  Test Coverage: {Math.round(testCoverage.statistics.overallCoverage || 0)}%
                </Typography>
                <LinearProgress 
                  variant="determinate" 
                  value={testCoverage.statistics.overallCoverage || 0}
                  sx={{ height: 8, borderRadius: 4 }}
                />
              </Box>
            )}
            
            {automationGaps?.summary && (
              <Box mt={2}>
                <Typography variant="body2" gutterBottom>
                  Automation Readiness: {Math.round(automationGaps.summary.automationReadinessPercentage || 0)}%
                </Typography>
                <LinearProgress 
                  variant="determinate" 
                  value={automationGaps.summary.automationReadinessPercentage || 0}
                  color="secondary"
                  sx={{ height: 8, borderRadius: 4 }}
                />
              </Box>
            )}
          </CardContent>
        </Card>
      </Grid>

      {/* Quick Actions */}
      <Grid item xs={12} md={6}>
        <Card>
          <CardHeader title="🚀 Quick Actions" />
          <CardContent>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <Button
                  fullWidth
                  variant="outlined"
                  startIcon={<CallSplit />}
                  onClick={() => setActiveTab(1)}
                >
                  Compare Branches
                </Button>
              </Grid>
              <Grid item xs={12}>
                <Button
                  fullWidth
                  variant="outlined"
                  startIcon={<Code />}
                  onClick={() => setActiveTab(2)}
                >
                  Scan UI Elements
                </Button>
              </Grid>
              <Grid item xs={12}>
                <Button
                  fullWidth
                  variant="outlined"
                  startIcon={<Science />}
                  onClick={() => setActiveTab(3)}
                >
                  Manage Test Cases
                </Button>
              </Grid>
              <Grid item xs={12}>
                <Button
                  fullWidth
                  variant="outlined"
                  startIcon={<Timeline />}
                  onClick={() => setActiveTab(4)}
                >
                  View Traceability
                </Button>
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      </Grid>

      {/* Current Gaps */}
      <Grid item xs={12} md={6}>
        <Card>
          <CardHeader title="⚠️ Action Items" />
          <CardContent>
            {automationGaps?.gaps && automationGaps.gaps.length > 0 ? (
              <List dense>
                {automationGaps.gaps.slice(0, 5).map((gap: any, index: number) => (
                  <ListItem key={index}>
                    <ListItemIcon>
                      <Warning color={gap.priority === 'High' ? 'error' : 'warning'} />
                    </ListItemIcon>
                    <ListItemText
                      primary={gap.description}
                      secondary={`Priority: ${gap.priority}`}
                    />
                  </ListItem>
                ))}
              </List>
            ) : (
              <Typography color="text.secondary">
                No automation gaps identified
              </Typography>
            )}
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );

  const renderBranchAnalysisTab = () => (
    <Grid container spacing={3}>
      <Grid item xs={12}>
        <Card>
          <CardHeader title="🔀 Branch Analysis & Comparison" />
          <CardContent>
            <Alert severity="info" sx={{ mb: 2 }}>
              Branch analysis feature helps you compare feature branches with main/develop 
              and assess merge risks before deployment.
            </Alert>
            
            {branchData && branchData.length > 0 ? (
              <List>
                {branchData.slice(0, 10).map((branch: string, index: number) => (
                  <ListItem key={index}>
                    <ListItemIcon>
                      <CallSplit />
                    </ListItemIcon>
                    <ListItemText primary={branch} />
                    <Button
                      size="small"
                      variant="outlined"
                      onClick={() => {
                        // TODO: Implement branch analysis
                        console.log('Analyzing branch:', branch);
                      }}
                    >
                      Analyze
                    </Button>
                  </ListItem>
                ))}
              </List>
            ) : (
              <Box textAlign="center" py={4}>
                <Button
                  variant="contained"
                  startIcon={<Refresh />}
                  onClick={loadBranchData}
                  disabled={loading}
                >
                  Load Branches
                </Button>
              </Box>
            )}
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );

  const renderUIElementsTab = () => (
    <Grid container spacing={3}>
      <Grid item xs={12}>
        <Card>
          <CardHeader title="🎨 UI Element Discovery" />
          <CardContent>
            <Alert severity="info" sx={{ mb: 2 }}>
              UI element discovery automatically finds components and generates 
              XPath/CSS selectors for test automation.
            </Alert>
            
            <Grid container spacing={2} sx={{ mb: 2 }}>
              <Grid item>
                <Button
                  variant="contained"
                  startIcon={<Launch />}
                  onClick={async () => {
                    try {
                      setLoading(true);
                      const scanResult = await apiService.scanUIElements(repositoryId);
                      setUIElements(scanResult);
                      setLoading(false);
                    } catch (err: any) {
                      setError(err.message);
                      setLoading(false);
                    }
                  }}
                  disabled={loading}
                >
                  {loading ? <CircularProgress size={20} /> : 'Scan UI Elements'}
                </Button>
              </Grid>
              
              <Grid item>
                <Button
                  variant="outlined"
                  startIcon={<Assessment />}
                  onClick={async () => {
                    try {
                      const score = await apiService.getTestabilityScore(repositoryId);
                      console.log('Testability score:', score);
                    } catch (err: any) {
                      console.error('Failed to get testability score:', err);
                    }
                  }}
                >
                  Get Testability Score
                </Button>
              </Grid>
            </Grid>

            {uiElements && (
              <Box>
                <Typography variant="h6" gutterBottom>
                  Discovered Elements: {uiElements.elements?.length || 0}
                </Typography>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Testability Score: {Math.round((uiElements.testabilityScore || 0) * 100)}%
                </Typography>
                
                {uiElements.elements?.slice(0, 5).map((element: any, index: number) => (
                  <Box key={index} sx={{ mb: 2, p: 2, border: 1, borderColor: 'divider', borderRadius: 1 }}>
                    <Typography variant="subtitle2">{element.component}</Typography>
                    <Typography variant="body2" color="text.secondary">
                      {element.filePath}
                    </Typography>
                    <Box sx={{ mt: 1 }}>
                      <Chip
                        size="small"
                        label={element.automationReady ? 'Automation Ready' : 'Needs Setup'}
                        color={element.automationReady ? 'success' : 'warning'}
                        icon={element.automationReady ? <CheckCircle /> : <Warning />}
                      />
                    </Box>
                  </Box>
                ))}
              </Box>
            )}
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );

  const renderTestCasesTab = () => (
    <Grid container spacing={3}>
      <Grid item xs={12}>
        <Card>
          <CardHeader title="🧪 Test Case Management" />
          <CardContent>
            <Alert severity="info" sx={{ mb: 2 }}>
              Manage test cases with full traceability to requirements, code files, and UI elements.
            </Alert>
            
            {testCoverage ? (
              <Grid container spacing={2}>
                <Grid item xs={12} md={6}>
                  <Typography variant="h6" gutterBottom>Coverage Statistics</Typography>
                  <List dense>
                    <ListItem>
                      <ListItemText 
                        primary="Overall Coverage" 
                        secondary={`${Math.round(testCoverage.statistics?.overallCoverage || 0)}%`}
                      />
                    </ListItem>
                    <ListItem>
                      <ListItemText 
                        primary="Total Test Cases" 
                        secondary={testCoverage.statistics?.totalTestCases || 0}
                      />
                    </ListItem>
                    <ListItem>
                      <ListItemText 
                        primary="Passing Tests" 
                        secondary={testCoverage.statistics?.passingTestCases || 0}
                      />
                    </ListItem>
                    <ListItem>
                      <ListItemText 
                        primary="Automated Tests" 
                        secondary={testCoverage.statistics?.automatedTestCases || 0}
                      />
                    </ListItem>
                  </List>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <Typography variant="h6" gutterBottom>Quick Actions</Typography>
                  <Grid container spacing={2}>
                    <Grid item xs={12}>
                      <Button
                        fullWidth
                        variant="outlined"
                        startIcon={<Science />}
                        onClick={() => {
                          // TODO: Implement create test case
                          console.log('Create new test case');
                        }}
                      >
                        Create Test Case
                      </Button>
                    </Grid>
                    <Grid item xs={12}>
                      <Button
                        fullWidth
                        variant="outlined"
                        startIcon={<Link />}
                        onClick={() => {
                          // TODO: Implement link to requirements
                          console.log('Link to requirements');
                        }}
                      >
                        Link to Requirements
                      </Button>
                    </Grid>
                    <Grid item xs={12}>
                      <Button
                        fullWidth
                        variant="outlined"
                        startIcon={<BugReport />}
                        onClick={async () => {
                          try {
                            const testCases = await apiService.generateTestCases(
                              ['REQ-001', 'REQ-002'], 
                              { includePositiveTests: true, includeNegativeTests: true }
                            );
                            console.log('Generated test cases:', testCases);
                          } catch (err) {
                            console.error('Failed to generate test cases:', err);
                          }
                        }}
                      >
                        Generate AI Test Cases
                      </Button>
                    </Grid>
                  </Grid>
                </Grid>
              </Grid>
            ) : (
              <Box textAlign="center" py={4}>
                <CircularProgress />
                <Typography variant="body2" sx={{ mt: 2 }}>
                  Loading test coverage data...
                </Typography>
              </Box>
            )}
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );

  const renderTraceabilityTab = () => (
    <Grid container spacing={3}>
      <Grid item xs={12}>
        <Card>
          <CardHeader title="📊 Traceability Matrix" />
          <CardContent>
            <Alert severity="info" sx={{ mb: 2 }}>
              Complete traceability from requirements through code to tests and deployment.
            </Alert>
            
            {traceabilityMatrix ? (
              <Grid container spacing={3}>
                <Grid item xs={12} md={4}>
                  <Paper sx={{ p: 2 }}>
                    <Typography variant="h6" gutterBottom>
                      <Timeline /> Requirements
                    </Typography>
                    <Typography variant="h4" color="primary">
                      {Object.keys(traceabilityMatrix.requirements || {}).length}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Traced: {traceabilityMatrix.statistics?.tracedRequirements || 0}
                    </Typography>
                  </Paper>
                </Grid>
                
                <Grid item xs={12} md={4}>
                  <Paper sx={{ p: 2 }}>
                    <Typography variant="h6" gutterBottom>
                      <Code /> Code Files
                    </Typography>
                    <Typography variant="h4" color="secondary">
                      {Object.keys(traceabilityMatrix.codeFiles || {}).length}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Traced: {traceabilityMatrix.statistics?.tracedCodeFiles || 0}
                    </Typography>
                  </Paper>
                </Grid>
                
                <Grid item xs={12} md={4}>
                  <Paper sx={{ p: 2 }}>
                    <Typography variant="h6" gutterBottom>
                      <Science /> Test Cases
                    </Typography>
                    <Typography variant="h4" color="info.main">
                      {Object.keys(traceabilityMatrix.testCases || {}).length}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Traced: {traceabilityMatrix.statistics?.tracedTestCases || 0}
                    </Typography>
                  </Paper>
                </Grid>

                <Grid item xs={12}>
                  <Divider sx={{ my: 2 }} />
                  <Typography variant="h6" gutterBottom>Traceability Gaps</Typography>
                  {traceabilityMatrix.gaps && traceabilityMatrix.gaps.length > 0 ? (
                    <List>
                      {traceabilityMatrix.gaps.slice(0, 5).map((gap: any, index: number) => (
                        <ListItem key={index}>
                          <ListItemIcon>
                            <Error color="error" />
                          </ListItemIcon>
                          <ListItemText
                            primary={gap.description}
                            secondary={`Type: ${gap.type} | Severity: ${gap.severity}`}
                          />
                        </ListItem>
                      ))}
                    </List>
                  ) : (
                    <Alert severity="success">
                      No traceability gaps identified! 🎉
                    </Alert>
                  )}
                </Grid>
              </Grid>
            ) : (
              <Box textAlign="center" py={4}>
                <CircularProgress />
                <Typography variant="body2" sx={{ mt: 2 }}>
                  Generating traceability matrix...
                </Typography>
              </Box>
            )}
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );

  if (error) {
    return (
      <Box p={3}>
        <Alert severity="error" action={
          <Button color="inherit" size="small" onClick={() => {
            setError(null);
            loadDigitalThreadData();
          }}>
            Retry
          </Button>
        }>
          {error}
        </Alert>
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Tabs 
          value={activeTab} 
          onChange={handleTabChange}
          variant="scrollable"
          scrollButtons="auto"
        >
          <Tab label="Overview" icon={<Timeline />} />
          <Tab label="Branch Analysis" icon={<CallSplit />} />
          <Tab label="UI Elements" icon={<Code />} />
          <Tab label="Test Cases" icon={<Science />} />
          <Tab label="Traceability" icon={<Link />} />
        </Tabs>
      </Box>

      <TabPanel value={activeTab} index={0}>
        {renderOverviewTab()}
      </TabPanel>
      
      <TabPanel value={activeTab} index={1}>
        {renderBranchAnalysisTab()}
      </TabPanel>
      
      <TabPanel value={activeTab} index={2}>
        {renderUIElementsTab()}
      </TabPanel>
      
      <TabPanel value={activeTab} index={3}>
        {renderTestCasesTab()}
      </TabPanel>
      
      <TabPanel value={activeTab} index={4}>
        {renderTraceabilityTab()}
      </TabPanel>
    </Box>
  );
};

export default DigitalThreadDashboard;
