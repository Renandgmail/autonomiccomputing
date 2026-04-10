import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Grid,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
  Alert,
  Tabs,
  Tab,
  LinearProgress,
  Tooltip,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Badge
} from '@mui/material';
import {
  ExpandMore as ExpandMoreIcon,
  Psychology as PsychologyIcon,
  Business as BusinessIcon,
  Code as CodeIcon,
  Search as SearchIcon,
  TrendingUp as TrendingUpIcon,
  AccountTree as AccountTreeIcon,
  Language as LanguageIcon,
  Category as CategoryIcon
} from '@mui/icons-material';
import apiService from '../../services/apiService';

interface VocabularyTerm {
  id: number;
  term: string;
  termType: string;
  source: string;
  domain: string;
  frequency: number;
  relevanceScore: number;
  businessRelevance: number;
  technicalRelevance: number;
  context: string;
  usageExamples: string[];
  locations: VocabularyLocation[];
}

interface VocabularyLocation {
  filePath: string;
  startLine: number;
  contextType: string;
  contextDescription: string;
}

interface VocabularyExtractionResult {
  repositoryId: number;
  totalTermsExtracted: number;
  businessTermsIdentified: number;
  technicalTermsIdentified: number;
  conceptRelationshipsFound: number;
  processingTime: string;
  relevanceScore: number;
  dominantDomains: string[];
  highValueTerms: VocabularyTerm[];
}

interface VocabularyStats {
  totalTerms: number;
  businessTerms: number;
  technicalTerms: number;
  domainSpecificTerms: number;
  termsByLanguage: Record<string, number>;
  termsByDomain: Record<string, number>;
  averageRelevanceScore: number;
}

interface Props {
  repositoryId: number;
}

export const VocabularyDashboard: React.FC<Props> = ({ repositoryId }) => {
  const [activeTab, setActiveTab] = useState(0);
  const [extractionResult, setExtractionResult] = useState<VocabularyExtractionResult | null>(null);
  const [vocabularyTerms, setVocabularyTerms] = useState<VocabularyTerm[]>([]);
  const [isExtracting, setIsExtracting] = useState(false);
  const [isLoadingTerms, setIsLoadingTerms] = useState(false);
  const [error, setError] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState('');
  const [filterTermType, setFilterTermType] = useState('');
  const [filterDomain, setFilterDomain] = useState('');
  const [page, setPage] = useState(1);
  const [stats, setStats] = useState<VocabularyStats | null>(null);

  // Extract vocabulary from repository
  const handleExtractVocabulary = async () => {
    try {
      setIsExtracting(true);
      setError('');
      
      const response = await apiService['api'].post(`/api/vocabulary/extract/${repositoryId}`, {});
      const result = response.data;
      setExtractionResult(result);
      
      // Load terms after extraction
      await loadVocabularyTerms();
      
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to extract vocabulary');
    } finally {
      setIsExtracting(false);
    }
  };

  // Load vocabulary terms with filtering
  const loadVocabularyTerms = async () => {
    try {
      setIsLoadingTerms(true);
      
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: '50'
      });
      
      if (filterTermType) params.append('termType', filterTermType);
      if (filterDomain) params.append('domain', filterDomain);
      if (searchTerm) params.append('search', searchTerm);

      const response = await apiService['api'].get(`/api/vocabulary/${repositoryId}/terms?${params}`);
      const responseData = response.data;
      
      setVocabularyTerms(responseData.terms || []);
      setStats(responseData.statistics);
      
    } catch (err: any) {
      if (err.response?.status === 501) {
        setError('Vocabulary query functionality will be available in the next release');
      } else {
        setError(err.response?.data?.error || 'Failed to load vocabulary terms');
      }
    } finally {
      setIsLoadingTerms(false);
    }
  };

  useEffect(() => {
    loadVocabularyTerms();
  }, [repositoryId, filterTermType, filterDomain, page]);

  const renderOverviewTab = () => (
    <Grid container spacing={3}>
      {/* Extraction Controls */}
      <Grid item xs={12}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              <PsychologyIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
              Vocabulary Extraction
            </Typography>
            <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
              Extract domain-specific vocabulary and business concepts from your codebase
            </Typography>
            <Button
              variant="contained"
              onClick={handleExtractVocabulary}
              disabled={isExtracting}
              startIcon={isExtracting ? <CircularProgress size={20} /> : <PsychologyIcon />}
            >
              {isExtracting ? 'Extracting Vocabulary...' : 'Extract Vocabulary'}
            </Button>
            {isExtracting && (
              <Box sx={{ mt: 2 }}>
                <LinearProgress />
                <Typography variant="body2" sx={{ mt: 1 }}>
                  Analyzing code elements and extracting domain terms...
                </Typography>
              </Box>
            )}
          </CardContent>
        </Card>
      </Grid>

      {/* Extraction Results */}
      {extractionResult && (
        <>
          <Grid item xs={12} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Total Terms Extracted
                </Typography>
                <Typography variant="h4">
                  {extractionResult.totalTermsExtracted.toLocaleString()}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Business Terms
                </Typography>
                <Typography variant="h4" color="primary">
                  {extractionResult.businessTermsIdentified.toLocaleString()}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Technical Terms
                </Typography>
                <Typography variant="h4" color="secondary">
                  {extractionResult.technicalTermsIdentified.toLocaleString()}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Processing Time
                </Typography>
                <Typography variant="h4">
                  {Math.round(parseFloat(extractionResult.processingTime))}ms
                </Typography>
              </CardContent>
            </Card>
          </Grid>

          {/* Dominant Domains */}
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  <CategoryIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                  Dominant Domains
                </Typography>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                  {extractionResult.dominantDomains.map((domain, index) => (
                    <Chip
                      key={index}
                      label={domain}
                      color="primary"
                      variant="outlined"
                    />
                  ))}
                </Box>
              </CardContent>
            </Card>
          </Grid>

          {/* Relevance Score */}
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  <TrendingUpIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                  Average Relevance Score
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <LinearProgress
                    variant="determinate"
                    value={extractionResult.relevanceScore * 100}
                    sx={{ flexGrow: 1, height: 8, borderRadius: 4 }}
                  />
                  <Typography variant="h6">
                    {Math.round(extractionResult.relevanceScore * 100)}%
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        </>
      )}

      {/* High Value Terms */}
      {extractionResult?.highValueTerms && extractionResult.highValueTerms.length > 0 && (
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <TrendingUpIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                High-Value Terms (80%+ Relevance)
              </Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {extractionResult.highValueTerms.slice(0, 20).map((term, index) => (
                  <Tooltip
                    key={index}
                    title={`${term.termType} • Relevance: ${Math.round(term.relevanceScore * 100)}% • Frequency: ${term.frequency}`}
                  >
                    <Chip
                      label={term.term}
                      color={term.businessRelevance > 0.5 ? 'primary' : 'secondary'}
                      sx={{ 
                        cursor: 'pointer',
                        '&:hover': { opacity: 0.8 }
                      }}
                    />
                  </Tooltip>
                ))}
              </Box>
            </CardContent>
          </Card>
        </Grid>
      )}
    </Grid>
  );

  const renderTermsTab = () => (
    <Box>
      {/* Filters */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Search Terms"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && loadVocabularyTerms()}
                InputProps={{
                  endAdornment: <SearchIcon />
                }}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <FormControl fullWidth>
                <InputLabel>Term Type</InputLabel>
                <Select
                  value={filterTermType}
                  label="Term Type"
                  onChange={(e) => setFilterTermType(e.target.value)}
                >
                  <MenuItem value="">All Types</MenuItem>
                  <MenuItem value="BusinessTerm">Business Term</MenuItem>
                  <MenuItem value="TechnicalTerm">Technical Term</MenuItem>
                  <MenuItem value="DomainSpecific">Domain Specific</MenuItem>
                  <MenuItem value="MethodName">Method Name</MenuItem>
                  <MenuItem value="ClassName">Class Name</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={3}>
              <FormControl fullWidth>
                <InputLabel>Domain</InputLabel>
                <Select
                  value={filterDomain}
                  label="Domain"
                  onChange={(e) => setFilterDomain(e.target.value)}
                >
                  <MenuItem value="">All Domains</MenuItem>
                  <MenuItem value="Financial">Financial</MenuItem>
                  <MenuItem value="Healthcare">Healthcare</MenuItem>
                  <MenuItem value="Ecommerce">E-commerce</MenuItem>
                  <MenuItem value="Security">Security</MenuItem>
                  <MenuItem value="Analytics">Analytics</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={2}>
              <Button
                fullWidth
                variant="contained"
                onClick={loadVocabularyTerms}
                disabled={isLoadingTerms}
              >
                Filter
              </Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Terms Table */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Vocabulary Terms
            {vocabularyTerms.length > 0 && (
              <Chip
                label={`${vocabularyTerms.length} terms`}
                size="small"
                sx={{ ml: 1 }}
              />
            )}
          </Typography>
          
          {isLoadingTerms ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Box>
          ) : vocabularyTerms.length === 0 ? (
            <Alert severity="info">
              No vocabulary terms found. Try extracting vocabulary first or adjust your filters.
            </Alert>
          ) : (
            <TableContainer component={Paper} variant="outlined">
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell><strong>Term</strong></TableCell>
                    <TableCell><strong>Type</strong></TableCell>
                    <TableCell><strong>Domain</strong></TableCell>
                    <TableCell><strong>Relevance</strong></TableCell>
                    <TableCell><strong>Frequency</strong></TableCell>
                    <TableCell><strong>Source</strong></TableCell>
                    <TableCell><strong>Context</strong></TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {vocabularyTerms.map((term) => (
                    <TableRow
                      key={term.id}
                      hover
                      sx={{ cursor: 'pointer' }}
                      onClick={() => {
                        // TODO: Show term details
                      }}
                    >
                      <TableCell>
                        <Typography variant="body2" fontWeight="bold">
                          {term.term}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={term.termType}
                          size="small"
                          color={term.businessRelevance > 0.5 ? 'primary' : 'secondary'}
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>
                        {term.domain && (
                          <Chip label={term.domain} size="small" />
                        )}
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <LinearProgress
                            variant="determinate"
                            value={term.relevanceScore * 100}
                            sx={{ width: 60, height: 4 }}
                          />
                          <Typography variant="body2">
                            {Math.round(term.relevanceScore * 100)}%
                          </Typography>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Badge badgeContent={term.frequency} color="primary">
                          <Box sx={{ width: 20, height: 20 }} />
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" color="textSecondary">
                          {term.source}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Tooltip title={term.context}>
                          <Typography 
                            variant="body2" 
                            sx={{ 
                              maxWidth: 200, 
                              overflow: 'hidden', 
                              textOverflow: 'ellipsis',
                              whiteSpace: 'nowrap'
                            }}
                          >
                            {term.context}
                          </Typography>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>
    </Box>
  );

  const renderAnalyticsTab = () => (
    <Grid container spacing={3}>
      {stats && (
        <>
          {/* Statistics Overview */}
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  <LanguageIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                  Terms by Language
                </Typography>
                {Object.entries(stats.termsByLanguage).map(([language, count]) => (
                  <Box key={language} sx={{ mb: 1 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                      <Typography variant="body2">{language}</Typography>
                      <Typography variant="body2">{count}</Typography>
                    </Box>
                    <LinearProgress
                      variant="determinate"
                      value={(count / stats.totalTerms) * 100}
                      sx={{ height: 6, borderRadius: 3 }}
                    />
                  </Box>
                ))}
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  <CategoryIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
                  Terms by Domain
                </Typography>
                {Object.entries(stats.termsByDomain).map(([domain, count]) => (
                  <Box key={domain} sx={{ mb: 1 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                      <Typography variant="body2">{domain}</Typography>
                      <Typography variant="body2">{count}</Typography>
                    </Box>
                    <LinearProgress
                      variant="determinate"
                      value={(count / stats.totalTerms) * 100}
                      sx={{ height: 6, borderRadius: 3 }}
                    />
                  </Box>
                ))}
              </CardContent>
            </Card>
          </Grid>

          {/* Business vs Technical Split */}
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Business vs Technical Terms Distribution
                </Typography>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={4}>
                    <Box textAlign="center">
                      <BusinessIcon sx={{ fontSize: 40, color: 'primary.main', mb: 1 }} />
                      <Typography variant="h4" color="primary">
                        {stats.businessTerms}
                      </Typography>
                      <Typography variant="body2" color="textSecondary">
                        Business Terms
                      </Typography>
                    </Box>
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <Box textAlign="center">
                      <CodeIcon sx={{ fontSize: 40, color: 'secondary.main', mb: 1 }} />
                      <Typography variant="h4" color="secondary">
                        {stats.technicalTerms}
                      </Typography>
                      <Typography variant="body2" color="textSecondary">
                        Technical Terms
                      </Typography>
                    </Box>
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <Box textAlign="center">
                      <AccountTreeIcon sx={{ fontSize: 40, color: 'success.main', mb: 1 }} />
                      <Typography variant="h4" color="success.main">
                        {stats.domainSpecificTerms}
                      </Typography>
                      <Typography variant="body2" color="textSecondary">
                        Domain Specific
                      </Typography>
                    </Box>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>
        </>
      )}
    </Grid>
  );

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        <PsychologyIcon sx={{ mr: 2, verticalAlign: 'middle' }} />
        Vocabulary Intelligence Dashboard
      </Typography>
      <Typography variant="body1" color="textSecondary" sx={{ mb: 3 }}>
        Extract and analyze domain-specific vocabulary from your codebase to understand business concepts and technical patterns.
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 3 }}>
        <Tabs value={activeTab} onChange={(_, newValue) => setActiveTab(newValue)}>
          <Tab label="Overview" />
          <Tab label="Vocabulary Terms" />
          <Tab label="Analytics" />
        </Tabs>
      </Box>

      {activeTab === 0 && renderOverviewTab()}
      {activeTab === 1 && renderTermsTab()}
      {activeTab === 2 && renderAnalyticsTab()}
    </Box>
  );
};

export default VocabularyDashboard;
