import React, { useState, useEffect } from 'react';
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
  Avatar,
  Tooltip,
  IconButton,
  Collapse,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Badge,
  Skeleton
} from '@mui/material';
import {
  People,
  Person,
  TrendingUp,
  TrendingDown,
  Assessment,
  Security,
  Speed,
  Code,
  BugReport,
  Star,
  Warning,
  ExpandMore,
  ExpandLess,
  Timeline,
  Group,
  School,
  EmojiEvents
} from '@mui/icons-material';
import apiService from '../../services/apiService';

interface ContributorMetric {
  email: string;
  name: string;
  commitCount: number;
  linesAdded: number;
  linesDeleted: number;
  filesModified: number;
  contributionPercentage: number;
  averageCommitSize: number;
  commitFrequency: number;
  isCoreContributor: boolean;
  isNewContributor: boolean;
  retentionScore: number;
  codeOwnershipPercentage: number;
  bugFixCommits: number;
  featureCommits: number;
  refactoringCommits: number;
  documentationCommits: number;
}

interface TeamCollaboration {
  totalContributors: number;
  collaborationIndex: number;
  teamCohesion: number;
  isolatedContributors: string[];
  communicationEffectiveness: number;
}

interface TeamRisks {
  busFactor: number;
  criticalContributors: string[];
  knowledgeGaps: string[];
  teamResilience: number;
  riskLevel: 'Low' | 'Medium' | 'High';
}

interface ProductivityData {
  teamProductivity: number;
  highPerformers: string[];
  improvementCandidates: string[];
}

interface ContributorAnalyticsProps {
  repositoryId: number;
  timeRange: number;
}

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8', '#82ca9d'];

const ContributorAnalytics: React.FC<ContributorAnalyticsProps> = ({ repositoryId, timeRange }) => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [contributorMetrics, setContributorMetrics] = useState<ContributorMetric[]>([]);
  const [teamCollaboration, setTeamCollaboration] = useState<TeamCollaboration | null>(null);
  const [teamRisks, setTeamRisks] = useState<TeamRisks | null>(null);
  const [productivity, setProductivity] = useState<ProductivityData | null>(null);
  const [selectedTab, setSelectedTab] = useState('overview');
  const [expandedSections, setExpandedSections] = useState<{ [key: string]: boolean }>({
    metrics: true,
    collaboration: false,
    risks: false,
    productivity: false
  });

  useEffect(() => {
    loadContributorAnalytics();
  }, [repositoryId, timeRange]);

  const loadContributorAnalytics = async () => {
    if (!repositoryId) return;

    try {
      setLoading(true);
      setError(null);

      // Load all contributor analytics data in parallel using proper API service methods
      const [metricsData, collaborationData, risksData, productivityData] = await Promise.allSettled([
        fetch(`${apiService['baseURL']}/api/contributoranalytics/repositories/${repositoryId}/contributors/metrics`, {
          headers: { 'Authorization': `Bearer ${localStorage.getItem('repolens_token')}` }
        }).then(res => res.json()),
        fetch(`${apiService['baseURL']}/api/contributoranalytics/repositories/${repositoryId}/team/collaboration`, {
          headers: { 'Authorization': `Bearer ${localStorage.getItem('repolens_token')}` }
        }).then(res => res.json()),
        fetch(`${apiService['baseURL']}/api/contributoranalytics/repositories/${repositoryId}/team/risks`, {
          headers: { 'Authorization': `Bearer ${localStorage.getItem('repolens_token')}` }
        }).then(res => res.json()),
        fetch(`${apiService['baseURL']}/api/contributoranalytics/repositories/${repositoryId}/team/productivity?timeFrameDays=${timeRange}`, {
          headers: { 'Authorization': `Bearer ${localStorage.getItem('repolens_token')}` }
        }).then(res => res.json())
      ]);

      if (metricsData.status === 'fulfilled' && metricsData.value.success) {
        setContributorMetrics(metricsData.value.data.contributors || []);
      }

      if (collaborationData.status === 'fulfilled' && collaborationData.value.success) {
        setTeamCollaboration(collaborationData.value.data);
      }

      if (risksData.status === 'fulfilled' && risksData.value.success) {
        setTeamRisks(risksData.value.data);
      }

      if (productivityData.status === 'fulfilled' && productivityData.value.success) {
        setProductivity(productivityData.value.data);
      }

      // Log any failed requests
      [metricsData, collaborationData, risksData, productivityData].forEach((result, index) => {
        if (result.status === 'rejected') {
          console.warn(`Contributor analytics data load failed for endpoint ${index}:`, result.reason);
        }
      });

    } catch (err: any) {
      setError('Failed to load contributor analytics data. Please try again.');
      console.error('Error loading contributor analytics data:', err);
    } finally {
      setLoading(false);
    }
  };

  const toggleSection = (section: string) => {
    setExpandedSections(prev => ({
      ...prev,
      [section]: !prev[section]
    }));
  };

  const getContributorTypeColor = (contributor: ContributorMetric): string => {
    if (contributor.isCoreContributor) return '#4caf50';
    if (contributor.isNewContributor) return '#2196f3';
    return '#9e9e9e';
  };

  const getRetentionColor = (score: number): string => {
    if (score >= 0.8) return '#4caf50';
    if (score >= 0.6) return '#ff9800';
    return '#f44336';
  };

  const getRiskColor = (level: string): string => {
    switch (level) {
      case 'Low': return '#4caf50';
      case 'Medium': return '#ff9800';
      case 'High': return '#f44336';
      default: return '#9e9e9e';
    }
  };

  const formatPercentage = (value: number): string => {
    return `${value.toFixed(1)}%`;
  };

  const renderOverviewMetrics = () => {
    const totalContributors = contributorMetrics.length;
    const coreContributors = contributorMetrics.filter(c => c.isCoreContributor).length;
    const newContributors = contributorMetrics.filter(c => c.isNewContributor).length;
    const averageRetention = contributorMetrics.length > 0 
      ? contributorMetrics.reduce((sum, c) => sum + c.retentionScore, 0) / contributorMetrics.length 
      : 0;

    return (
      <Grid container spacing={3}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography variant="h4" fontWeight="bold" color="primary">
                    {totalContributors}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Total Contributors
                  </Typography>
                </Box>
                <People fontSize="large" color="primary" />
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
                    {coreContributors}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Core Contributors
                  </Typography>
                </Box>
                <Star fontSize="large" color="success" />
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
                    {newContributors}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    New Contributors
                  </Typography>
                </Box>
                <School fontSize="large" color="info" />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography variant="h4" fontWeight="bold" sx={{ color: getRetentionColor(averageRetention) }}>
                    {formatPercentage(averageRetention * 100)}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Avg Retention Score
                  </Typography>
                </Box>
                <Assessment fontSize="large" sx={{ color: getRetentionColor(averageRetention) }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    );
  };

  const renderContributorTable = () => {
    const sortedContributors = contributorMetrics
      .sort((a, b) => b.contributionPercentage - a.contributionPercentage)
      .slice(0, 10); // Top 10 contributors

    return (
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Contributor</TableCell>
              <TableCell align="right">Commits</TableCell>
              <TableCell align="right">Contribution %</TableCell>
              <TableCell align="right">Code Ownership %</TableCell>
              <TableCell align="right">Retention Score</TableCell>
              <TableCell align="center">Type</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {sortedContributors.map((contributor) => (
              <TableRow key={contributor.email}>
                <TableCell>
                  <Box display="flex" alignItems="center" gap={1}>
                    <Avatar sx={{ width: 32, height: 32, bgcolor: getContributorTypeColor(contributor) }}>
                      <Person fontSize="small" />
                    </Avatar>
                    <Box>
                      <Typography variant="body2" fontWeight="medium">
                        {contributor.name || contributor.email}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {contributor.email}
                      </Typography>
                    </Box>
                  </Box>
                </TableCell>
                <TableCell align="right">
                  <Typography variant="body2" fontWeight="medium">
                    {contributor.commitCount.toLocaleString()}
                  </Typography>
                </TableCell>
                <TableCell align="right">
                  <Box display="flex" alignItems="center" justifyContent="flex-end" gap={1}>
                    <LinearProgress
                      variant="determinate"
                      value={contributor.contributionPercentage}
                      sx={{ width: 60, height: 6 }}
                    />
                    <Typography variant="body2">
                      {formatPercentage(contributor.contributionPercentage)}
                    </Typography>
                  </Box>
                </TableCell>
                <TableCell align="right">
                  <Typography variant="body2">
                    {formatPercentage(contributor.codeOwnershipPercentage)}
                  </Typography>
                </TableCell>
                <TableCell align="right">
                  <Chip
                    label={formatPercentage(contributor.retentionScore * 100)}
                    size="small"
                    sx={{
                      backgroundColor: getRetentionColor(contributor.retentionScore),
                      color: 'white'
                    }}
                  />
                </TableCell>
                <TableCell align="center">
                  <Box display="flex" gap={0.5} justifyContent="center">
                    {contributor.isCoreContributor && (
                      <Tooltip title="Core Contributor">
                        <Star fontSize="small" color="warning" />
                      </Tooltip>
                    )}
                    {contributor.isNewContributor && (
                      <Tooltip title="New Contributor">
                        <School fontSize="small" color="info" />
                      </Tooltip>
                    )}
                  </Box>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    );
  };

  const renderTeamCollaboration = () => {
    if (!teamCollaboration) return null;

    return (
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Team Collaboration Metrics
            </Typography>
            <Box display="flex" flexDirection="column" gap={2}>
              <Box display="flex" justifyContent="space-between" alignItems="center">
                <Typography variant="body2">Collaboration Index</Typography>
                <Chip 
                  label={teamCollaboration.collaborationIndex.toFixed(2)}
                  color={teamCollaboration.collaborationIndex > 0.7 ? 'success' : 'warning'}
                />
              </Box>
              <Box display="flex" justifyContent="space-between" alignItems="center">
                <Typography variant="body2">Team Cohesion</Typography>
                <Chip 
                  label={formatPercentage(teamCollaboration.teamCohesion * 100)}
                  color={teamCollaboration.teamCohesion > 0.6 ? 'success' : 'warning'}
                />
              </Box>
              <Box display="flex" justifyContent="space-between" alignItems="center">
                <Typography variant="body2">Communication Effectiveness</Typography>
                <Chip 
                  label={formatPercentage(teamCollaboration.communicationEffectiveness * 100)}
                  color={teamCollaboration.communicationEffectiveness > 0.7 ? 'success' : 'warning'}
                />
              </Box>
            </Box>
          </Paper>
        </Grid>

        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Isolated Contributors
            </Typography>
            {teamCollaboration.isolatedContributors.length > 0 ? (
              <List dense>
                {teamCollaboration.isolatedContributors.map((contributor) => (
                  <ListItem key={contributor}>
                    <ListItemIcon>
                      <Warning color="warning" />
                    </ListItemIcon>
                    <ListItemText primary={contributor} />
                  </ListItem>
                ))}
              </List>
            ) : (
              <Typography variant="body2" color="success.main">
                No isolated contributors detected
              </Typography>
            )}
          </Paper>
        </Grid>
      </Grid>
    );
  };

  const renderTeamRisks = () => {
    if (!teamRisks) return null;

    return (
      <Grid container spacing={3}>
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography variant="h4" fontWeight="bold" sx={{ color: getRiskColor(teamRisks.riskLevel) }}>
                    {teamRisks.busFactor}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Bus Factor
                  </Typography>
                </Box>
                <Warning fontSize="large" sx={{ color: getRiskColor(teamRisks.riskLevel) }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography variant="h4" fontWeight="bold" color={getRiskColor(teamRisks.riskLevel)}>
                    {teamRisks.riskLevel}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Risk Level
                  </Typography>
                </Box>
                <Security fontSize="large" sx={{ color: getRiskColor(teamRisks.riskLevel) }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography variant="h4" fontWeight="bold" color="primary">
                    {formatPercentage(teamRisks.teamResilience * 100)}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Team Resilience
                  </Typography>
                </Box>
                <Group fontSize="large" color="primary" />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Critical Contributors
            </Typography>
            {teamRisks.criticalContributors.length > 0 ? (
              <List dense>
                {teamRisks.criticalContributors.map((contributor) => (
                  <ListItem key={contributor}>
                    <ListItemIcon>
                      <Star color="warning" />
                    </ListItemIcon>
                    <ListItemText primary={contributor} />
                  </ListItem>
                ))}
              </List>
            ) : (
              <Typography variant="body2" color="text.secondary">
                No critical single points of failure
              </Typography>
            )}
          </Paper>
        </Grid>

        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Knowledge Gaps
            </Typography>
            {teamRisks.knowledgeGaps.length > 0 ? (
              <List dense>
                {teamRisks.knowledgeGaps.map((gap) => (
                  <ListItem key={gap}>
                    <ListItemIcon>
                      <BugReport color="error" />
                    </ListItemIcon>
                    <ListItemText primary={gap} />
                  </ListItem>
                ))}
              </List>
            ) : (
              <Typography variant="body2" color="success.main">
                No significant knowledge gaps detected
              </Typography>
            )}
          </Paper>
        </Grid>
      </Grid>
    );
  };

  const renderProductivity = () => {
    if (!productivity) return null;

    return (
      <Grid container spacing={3}>
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography variant="h4" fontWeight="bold" color="primary">
                    {formatPercentage(productivity.teamProductivity * 100)}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Team Productivity
                  </Typography>
                </Box>
                <Speed fontSize="large" color="primary" />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom color="success.main">
              High Performers
            </Typography>
            {productivity.highPerformers.length > 0 ? (
              <List dense>
                {productivity.highPerformers.map((performer) => (
                  <ListItem key={performer}>
                    <ListItemIcon>
                      <EmojiEvents color="success" />
                    </ListItemIcon>
                    <ListItemText primary={performer} />
                  </ListItem>
                ))}
              </List>
            ) : (
              <Typography variant="body2" color="text.secondary">
                No high performers identified
              </Typography>
            )}
          </Paper>
        </Grid>

        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom color="warning.main">
              Improvement Candidates
            </Typography>
            {productivity.improvementCandidates.length > 0 ? (
              <List dense>
                {productivity.improvementCandidates.map((candidate) => (
                  <ListItem key={candidate}>
                    <ListItemIcon>
                      <TrendingUp color="warning" />
                    </ListItemIcon>
                    <ListItemText primary={candidate} />
                  </ListItem>
                ))}
              </List>
            ) : (
              <Typography variant="body2" color="success.main">
                All team members performing well
              </Typography>
            )}
          </Paper>
        </Grid>
      </Grid>
    );
  };

  const renderExpandableSection = (
    key: string,
    title: string,
    icon: React.ReactNode,
    content: React.ReactNode,
    color: string = 'primary'
  ) => (
    <Paper sx={{ mb: 2 }}>
      <Box
        sx={{
          p: 2,
          cursor: 'pointer',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          borderBottom: expandedSections[key] ? '1px solid #eee' : 'none'
        }}
        onClick={() => toggleSection(key)}
      >
        <Box display="flex" alignItems="center" gap={1}>
          <Box sx={{ color: `${color}.main` }}>{icon}</Box>
          <Typography variant="h6" color={`${color}.main`}>
            {title}
          </Typography>
        </Box>
        <IconButton size="small">
          {expandedSections[key] ? <ExpandLess /> : <ExpandMore />}
        </IconButton>
      </Box>
      <Collapse in={expandedSections[key]}>
        <Box sx={{ p: 3 }}>
          {content}
        </Box>
      </Collapse>
    </Paper>
  );

  if (loading) {
    return (
      <Box>
        <Typography variant="h5" gutterBottom>
          Contributor Analytics
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
          <Skeleton variant="rectangular" width="100%" height={400} />
        </Box>
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 3 }}>
        {error}
      </Alert>
    );
  }

  return (
    <Box>
      <Typography variant="h5" gutterBottom>
        Contributor Analytics
      </Typography>
      
      {/* Overview Metrics */}
      {renderExpandableSection('metrics', 'Team Overview', <People />, renderOverviewMetrics())}
      
      {/* Contributor Details */}
      <Paper sx={{ mb: 2 }}>
        <Box sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            Top Contributors
          </Typography>
          {contributorMetrics.length > 0 ? renderContributorTable() : (
            <Typography variant="body2" color="text.secondary">
              No contributor data available
            </Typography>
          )}
        </Box>
      </Paper>

      {/* Team Collaboration */}
      {renderExpandableSection('collaboration', 'Team Collaboration', <Group />, renderTeamCollaboration(), 'success')}
      
      {/* Team Risks */}
      {renderExpandableSection('risks', 'Risk Assessment (Bus Factor)', <Warning />, renderTeamRisks(), 'warning')}
      
      {/* Productivity */}
      {renderExpandableSection('productivity', 'Team Productivity', <Speed />, renderProductivity(), 'info')}
    </Box>
  );
};

export default ContributorAnalytics;
