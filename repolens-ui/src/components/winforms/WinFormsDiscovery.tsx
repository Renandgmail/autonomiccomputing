import React, { useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Card,
  CardContent,
  Button,
  TextField,
  Chip,
  Alert,
  Grid,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  CircularProgress,
  InputAdornment,
  Divider
} from '@mui/material';
import {
  FolderOpen,
  Search,
  Description,
  ErrorOutline,
  CheckCircle,
  Warning
} from '@mui/icons-material';
import { WinFormsAnalysisService, DiscoveryResult } from '../../services/winFormsAnalysisService';

interface WinFormsDiscoveryProps {
  onProjectSelected: (path: string) => void;
}

export const WinFormsDiscovery: React.FC<WinFormsDiscoveryProps> = ({ onProjectSelected }) => {
  const [searchPath, setSearchPath] = useState('');
  const [isDiscovering, setIsDiscovering] = useState(false);
  const [discoveryResult, setDiscoveryResult] = useState<DiscoveryResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  const suggestedPaths = WinFormsAnalysisService.getSuggestedProjectPaths();

  const discoverProjects = async (path: string) => {
    if (!path.trim()) return;

    setIsDiscovering(true);
    setError(null);
    setDiscoveryResult(null);

    try {
      const result = await WinFormsAnalysisService.discoverWinForms({ 
        sourcePath: path.trim() 
      });
      setDiscoveryResult(result);
      
      if (result.totalForms === 0) {
        setError('No WinForms projects found in the specified directory');
      }
    } catch (err: any) {
      setError(err.message || 'Discovery failed');
    } finally {
      setIsDiscovering(false);
    }
  };

  const handlePathClick = (path: string) => {
    setSearchPath(path);
    discoverProjects(path);
  };

  const getComplexityColor = (complexity: 'low' | 'medium' | 'high') => {
    switch (complexity) {
      case 'low': return 'success';
      case 'medium': return 'warning';
      case 'high': return 'error';
      default: return 'default';
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      discoverProjects(searchPath);
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Search Section */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" gutterBottom>
          Project Directory Path
        </Typography>
        
        <Box display="flex" gap={2} mb={3}>
          <TextField
            fullWidth
            value={searchPath}
            onChange={(e) => setSearchPath(e.target.value)}
            placeholder="C:\Projects\LegacyWinFormsApp"
            onKeyPress={handleKeyPress}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <FolderOpen />
                </InputAdornment>
              )
            }}
          />
          <Button 
            variant="contained"
            onClick={() => discoverProjects(searchPath)}
            disabled={!searchPath.trim() || isDiscovering}
            startIcon={isDiscovering ? <CircularProgress size={16} /> : <Search />}
          >
            {isDiscovering ? 'Scanning...' : 'Discover'}
          </Button>
        </Box>

        {/* Suggested Paths */}
        <Typography variant="body2" color="text.secondary" gutterBottom>
          Common project locations:
        </Typography>
        <Box display="flex" flexWrap="wrap" gap={1}>
          {suggestedPaths.map((path) => (
            <Button
              key={path}
              variant="outlined"
              size="small"
              onClick={() => handlePathClick(path)}
              startIcon={<FolderOpen />}
              sx={{ fontSize: '0.7rem' }}
            >
              {path}
            </Button>
          ))}
        </Box>
      </Paper>

      {/* Error Display */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          <Typography variant="subtitle1" fontWeight="bold">
            Discovery Failed
          </Typography>
          <Typography variant="body2">{error}</Typography>
        </Alert>
      )}

      {/* Discovery Results */}
      {discoveryResult && (
        <Box>
          {/* Summary */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Box display="flex" alignItems="center" gap={1} mb={2}>
                <CheckCircle color="success" />
                <Typography variant="h6">Discovery Complete</Typography>
              </Box>
              
              <Grid container spacing={4}>
                <Grid item xs={12} sm={4}>
                  <Box textAlign="center">
                    <Typography variant="h3" color="primary" fontWeight="bold">
                      {discoveryResult.totalForms}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Forms Found
                    </Typography>
                  </Box>
                </Grid>
                <Grid item xs={12} sm={4}>
                  <Box textAlign="center">
                    <Typography variant="h3" color="success.main" fontWeight="bold">
                      {discoveryResult.recommendations.startWithSimpleForms.length}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Simple Forms
                    </Typography>
                  </Box>
                </Grid>
                <Grid item xs={12} sm={4}>
                  <Box textAlign="center">
                    <Typography variant="h3" color="warning.main" fontWeight="bold">
                      {discoveryResult.recommendations.complexForms.length}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Complex Forms
                    </Typography>
                  </Box>
                </Grid>
              </Grid>

              <Divider sx={{ my: 2 }} />
              
              <Button 
                fullWidth
                variant="contained"
                size="large"
                onClick={() => onProjectSelected(discoveryResult.sourcePath)}
              >
                Analyze This Project
              </Button>
            </CardContent>
          </Card>

          {/* Forms List */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Discovered Forms ({discoveryResult.forms.length})
              </Typography>
              
              <Box sx={{ maxHeight: 400, overflow: 'auto' }}>
                {discoveryResult.forms.map((form, index) => (
                  <Card key={index} variant="outlined" sx={{ mb: 2, cursor: 'pointer' }}>
                    <CardContent sx={{ py: 2 }}>
                      <Box display="flex" justifyContent="space-between" alignItems="center">
                        <Box display="flex" alignItems="center" gap={2}>
                          <Description color="primary" />
                          <Box>
                            <Typography variant="subtitle1" fontWeight="bold">
                              {form.formName}
                            </Typography>
                            <Typography variant="body2" color="text.secondary">
                              {form.logicFile ? 'Logic + ' : ''}{form.hasDesigner ? 'Designer' : 'Code Only'}
                            </Typography>
                          </Box>
                        </Box>
                        
                        <Box display="flex" alignItems="center" gap={1}>
                          <Chip 
                            label={form.estimatedComplexity}
                            color={getComplexityColor(form.estimatedComplexity)}
                            size="small"
                          />
                          <Button
                            variant="outlined"
                            size="small"
                            onClick={() => onProjectSelected(discoveryResult.sourcePath)}
                          >
                            Analyze
                          </Button>
                        </Box>
                      </Box>
                    </CardContent>
                  </Card>
                ))}
              </Box>
            </CardContent>
          </Card>

          {/* Recommendations */}
          {(discoveryResult.recommendations.startWithSimpleForms.length > 0 || 
            discoveryResult.recommendations.complexForms.length > 0) && (
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Modernization Recommendations
                </Typography>
                
                <Grid container spacing={3}>
                  {discoveryResult.recommendations.startWithSimpleForms.length > 0 && (
                    <Grid item xs={12} md={6}>
                      <Typography variant="subtitle1" color="success.main" fontWeight="bold" gutterBottom>
                        Start with these simple forms:
                      </Typography>
                      <List dense>
                        {discoveryResult.recommendations.startWithSimpleForms.map((form, index) => (
                          <ListItem key={index}>
                            <ListItemIcon>
                              <CheckCircle color="success" fontSize="small" />
                            </ListItemIcon>
                            <ListItemText 
                              primary={form}
                              primaryTypographyProps={{ variant: 'body2' }}
                            />
                          </ListItem>
                        ))}
                      </List>
                    </Grid>
                  )}

                  {discoveryResult.recommendations.complexForms.length > 0 && (
                    <Grid item xs={12} md={6}>
                      <Typography variant="subtitle1" color="warning.main" fontWeight="bold" gutterBottom>
                        Complex forms requiring manual review:
                      </Typography>
                      <List dense>
                        {discoveryResult.recommendations.complexForms.map((form, index) => (
                          <ListItem key={index}>
                            <ListItemIcon>
                              <Warning color="warning" fontSize="small" />
                            </ListItemIcon>
                            <ListItemText 
                              primary={form}
                              primaryTypographyProps={{ variant: 'body2' }}
                            />
                          </ListItem>
                        ))}
                      </List>
                    </Grid>
                  )}
                </Grid>
              </CardContent>
            </Card>
          )}
        </Box>
      )}
    </Box>
  );
};
