import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Card,
  CardContent,
  Button,
  TextField,
  Chip,
  LinearProgress,
  Tabs,
  Tab,
  Alert,
  Grid,
  CircularProgress,
  IconButton,
  Divider
} from '@mui/material';
import {
  FolderOpen,
  Bolt,
  AttachMoney,
  Schedule,
  TrendingUp,
  GetApp,
  Settings,
  ErrorOutline,
  CheckCircle,
  SmartToy,
  Code
} from '@mui/icons-material';
import { WinFormsAnalysisService } from '../../services/winFormsAnalysisService';
import { WinFormsDiscovery } from './WinFormsDiscovery';

interface AnalyzerStatus {
  aiAnalyzer: string;
  capabilities: {
    controlsAnalysis: boolean;
    eventHandlerMapping: boolean;
    databaseAnalysis: boolean;
    validationLogic: boolean;
    blueprintGeneration: boolean;
    costTracking: boolean;
    qualityAssessment: boolean;
  };
  costSavingsEnabled: boolean;
  fallbackMode: boolean;
  recommendations: string[];
}

interface AnalysisProgress {
  phase: number;
  phaseName: string;
  isActive: boolean;
  isComplete: boolean;
  processingTime?: string;
  modelUsed?: string;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

const TabPanel: React.FC<TabPanelProps> = ({ children, value, index, ...other }) => (
  <div
    role="tabpanel"
    hidden={value !== index}
    id={`simple-tabpanel-${index}`}
    aria-labelledby={`simple-tab-${index}`}
    {...other}
  >
    {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
  </div>
);

export const WinFormsModernizationDashboard: React.FC = () => {
  const [analyzerStatus, setAnalyzerStatus] = useState<AnalyzerStatus | null>(null);
  const [sourcePath, setSourcePath] = useState('');
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [analysisProgress, setAnalysisProgress] = useState<AnalysisProgress[]>([]);
  const [analysisResults, setAnalysisResults] = useState<any>(null);
  const [selectedPhase, setSelectedPhase] = useState<number | null>(null);
  const [activeTab, setActiveTab] = useState(0);

  useEffect(() => {
    loadAnalyzerStatus();
  }, []);

  const loadAnalyzerStatus = async () => {
    try {
      const status = await WinFormsAnalysisService.getAnalyzerStatus();
      setAnalyzerStatus(status);
    } catch (error) {
      console.error('Failed to load analyzer status:', error);
    }
  };

  const runCompleteAnalysis = async () => {
    if (!sourcePath.trim()) return;

    setIsAnalyzing(true);
    setAnalysisResults(null);
    
    // Initialize progress tracking
    const phases = [
      'Controls Analysis',
      'Event Handlers Analysis', 
      'Database Analysis',
      'Validation Analysis',
      'Blueprint Generation'
    ];

    const progressSteps = phases.map((name, index) => ({
      phase: index + 1,
      phaseName: name,
      isActive: false,
      isComplete: false
    }));

    setAnalysisProgress(progressSteps);

    try {
      // Simulate real-time progress updates
      const updateProgress = (phaseIndex: number, status: 'active' | 'complete', data?: any) => {
        setAnalysisProgress(prev => prev.map((step, index) => ({
          ...step,
          isActive: index === phaseIndex && status === 'active',
          isComplete: index < phaseIndex || (index === phaseIndex && status === 'complete'),
          ...(data && { processingTime: data.processingTime, modelUsed: data.modelUsed })
        })));
      };

      // Start analysis
      phases.forEach((_, index) => {
        setTimeout(() => updateProgress(index, 'active'), index * 1000);
      });

      const result = await WinFormsAnalysisService.analyzeProject({
        sourcePath: sourcePath.trim(),
        verbose: true
      });

      // Mark all phases complete
      phases.forEach((_, index) => {
        updateProgress(index, 'complete', {
          processingTime: result.phases[index]?.processingTime || '0ms',
          modelUsed: result.phases[index]?.modelUsed || 'Unknown'
        });
      });

      setAnalysisResults(result);
      setActiveTab(2); // Switch to results tab
    } catch (error) {
      console.error('Analysis failed:', error);
    } finally {
      setIsAnalyzing(false);
    }
  };

  const exportResults = async (format: 'json' | 'markdown' | 'csv') => {
    try {
      await WinFormsAnalysisService.exportAnalysis({
        sourcePath: sourcePath.trim(),
        format
      });
    } catch (error) {
      console.error('Export failed:', error);
    }
  };

  if (!analyzerStatus) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={4}>
        <Box>
          <Typography variant="h3" component="h1" fontWeight="bold" gutterBottom>
            WinForms Modernization
          </Typography>
          <Typography variant="h6" color="text.secondary">
            AI-powered legacy application modernization
          </Typography>
        </Box>
        <Box display="flex" alignItems="center" gap={2}>
          <Chip
            icon={analyzerStatus.costSavingsEnabled ? <Bolt /> : <ErrorOutline />}
            label={analyzerStatus.costSavingsEnabled ? 'FREE AI' : 'Basic Mode'}
            color={analyzerStatus.costSavingsEnabled ? 'success' : 'default'}
          />
          <Button
            variant="outlined"
            startIcon={<Settings />}
            onClick={loadAnalyzerStatus}
          >
            Refresh Status
          </Button>
        </Box>
      </Box>

      {/* Status Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" gap={2}>
                <SmartToy sx={{ fontSize: 32, color: 'primary.main' }} />
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    AI Analyzer
                  </Typography>
                  <Typography variant="subtitle1" fontWeight="bold">
                    {analyzerStatus.aiAnalyzer}
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" gap={2}>
                <AttachMoney sx={{ fontSize: 32, color: 'success.main' }} />
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Cost Savings
                  </Typography>
                  <Typography variant="subtitle1" fontWeight="bold">
                    {analyzerStatus.costSavingsEnabled ? '$4.55-13.50 per project' : 'Install CodeLlama'}
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" gap={2}>
                <Code sx={{ fontSize: 32, color: 'secondary.main' }} />
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Privacy Level
                  </Typography>
                  <Typography variant="subtitle1" fontWeight="bold">
                    {analyzerStatus.costSavingsEnabled ? 'Complete Local' : 'Basic'}
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" gap={2}>
                <TrendingUp sx={{ fontSize: 32, color: 'warning.main' }} />
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Quality Level
                  </Typography>
                  <Typography variant="subtitle1" fontWeight="bold">
                    {analyzerStatus.fallbackMode ? 'Basic Rules' : 'AI Enhanced'}
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Recommendations */}
      {analyzerStatus.recommendations.length > 0 && (
        <Alert 
          severity={analyzerStatus.costSavingsEnabled ? "success" : "warning"}
          sx={{ mb: 4 }}
        >
          <Box>
            {analyzerStatus.recommendations.map((rec, index) => (
              <Typography key={index} variant="body2">
                {rec}
              </Typography>
            ))}
          </Box>
        </Alert>
      )}

      {/* Analysis Progress */}
      {isAnalyzing && (
        <Card sx={{ mb: 4 }}>
          <CardContent>
            <Box display="flex" alignItems="center" gap={1} mb={2}>
              <Schedule />
              <Typography variant="h6">Analysis Progress</Typography>
            </Box>
            
            <Box>
              {analysisProgress.map((step) => (
                <Box key={step.phase} display="flex" alignItems="center" gap={2} mb={2}>
                  <Box
                    sx={{
                      width: 32,
                      height: 32,
                      borderRadius: '50%',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '0.875rem',
                      fontWeight: 'bold',
                      bgcolor: step.isComplete 
                        ? 'success.light' 
                        : step.isActive 
                          ? 'primary.light' 
                          : 'grey.200',
                      color: step.isComplete || step.isActive ? 'white' : 'grey.600'
                    }}
                  >
                    {step.isComplete ? <CheckCircle sx={{ fontSize: 16 }} /> : step.phase}
                  </Box>
                  
                  <Box flex={1}>
                    <Box display="flex" justifyContent="space-between" alignItems="center">
                      <Typography 
                        variant="body1" 
                        fontWeight={step.isActive ? 'bold' : 'normal'}
                        color={step.isActive ? 'primary' : step.isComplete ? 'success.main' : 'text.secondary'}
                      >
                        {step.phaseName}
                      </Typography>
                      {step.processingTime && (
                        <Typography variant="caption" color="text.secondary">
                          {step.processingTime}
                        </Typography>
                      )}
                    </Box>
                    {step.modelUsed && (
                      <Typography variant="caption" color="text.secondary">
                        Using: {step.modelUsed}
                      </Typography>
                    )}
                  </Box>
                </Box>
              ))}
            </Box>
          </CardContent>
        </Card>
      )}

      {/* Main Interface */}
      <Paper sx={{ width: '100%' }}>
        <Tabs 
          value={activeTab} 
          onChange={(_, newValue) => setActiveTab(newValue)}
          variant="fullWidth"
        >
          <Tab label="Discover" />
          <Tab label="Analyze" />
          <Tab label="Results" disabled={!analysisResults} />
          <Tab label="Cost Analysis" />
        </Tabs>

        <TabPanel value={activeTab} index={0}>
          <WinFormsDiscovery 
            onProjectSelected={(path) => {
              setSourcePath(path);
              setActiveTab(1);
            }}
          />
        </TabPanel>

        <TabPanel value={activeTab} index={1}>
          <Box>
            <Typography variant="h6" gutterBottom>
              Run Analysis
            </Typography>
            
            <Box mb={3}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                WinForms Project Path
              </Typography>
              <TextField
                fullWidth
                value={sourcePath}
                onChange={(e) => setSourcePath(e.target.value)}
                placeholder="C:\Projects\LegacyWinFormsApp\src"
              />
            </Box>
            
            <Box display="flex" gap={2}>
              <Button
                variant="contained"
                startIcon={isAnalyzing ? <CircularProgress size={16} /> : <Bolt />}
                onClick={runCompleteAnalysis}
                disabled={!sourcePath.trim() || isAnalyzing}
              >
                {isAnalyzing ? 'Analyzing...' : 'Run Complete Analysis'}
              </Button>
              
              <Button
                variant="outlined"
                startIcon={<FolderOpen />}
                onClick={() => setActiveTab(0)}
              >
                Browse Projects
              </Button>
            </Box>
          </Box>
        </TabPanel>

        <TabPanel value={activeTab} index={2}>
          {analysisResults && (
            <Box>
              <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
                <Typography variant="h5">Analysis Results</Typography>
                <Box display="flex" gap={1}>
                  <Button
                    variant="outlined"
                    size="small"
                    startIcon={<GetApp />}
                    onClick={() => exportResults('json')}
                  >
                    Export JSON
                  </Button>
                  <Button
                    variant="outlined"
                    size="small"
                    startIcon={<GetApp />}
                    onClick={() => exportResults('markdown')}
                  >
                    Export Report
                  </Button>
                </Box>
              </Box>
              
              {/* Analysis Results Summary */}
              <Grid container spacing={3}>
                <Grid item xs={12} md={4}>
                  <Card>
                    <CardContent>
                      <Typography variant="h4" color="primary" gutterBottom>
                        {analysisResults.summary.totalForms}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Forms Analyzed
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
                
                <Grid item xs={12} md={4}>
                  <Card>
                    <CardContent>
                      <Typography variant="h4" color="success.main" gutterBottom>
                        {(analysisResults.summary.averageConfidence * 100).toFixed(0)}%
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Average Confidence
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
                
                <Grid item xs={12} md={4}>
                  <Card>
                    <CardContent>
                      <Typography variant="h4" color="warning.main" gutterBottom>
                        ${analysisResults.summary.costSavings.toFixed(2)}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Cost Savings
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>

              <Box mt={3}>
                <Typography variant="h6" gutterBottom>
                  Analysis Phases
                </Typography>
                {analysisResults.phases.map((phase: any, index: number) => (
                  <Card key={index} sx={{ mb: 2 }}>
                    <CardContent>
                      <Box display="flex" justifyContent="space-between" alignItems="center">
                        <Box>
                          <Typography variant="subtitle1" fontWeight="bold">
                            Phase {phase.phaseNumber}: {phase.phaseName}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            {phase.modelUsed} • {WinFormsAnalysisService.formatDuration(phase.processingTime)}
                          </Typography>
                        </Box>
                        <Box textAlign="right">
                          <Typography variant="h6" color="primary">
                            {(phase.confidence * 100).toFixed(0)}%
                          </Typography>
                          <Typography variant="caption">Confidence</Typography>
                        </Box>
                      </Box>
                    </CardContent>
                  </Card>
                ))}
              </Box>
            </Box>
          )}
        </TabPanel>

        <TabPanel value={activeTab} index={3}>
          <Box>
            <Typography variant="h6" gutterBottom>
              Cost Analysis
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Cost analysis functionality will be implemented here.
            </Typography>
          </Box>
        </TabPanel>
      </Paper>
    </Box>
  );
};
