import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Card,
  CardContent,
  CardActions,
  Button,
  Grid,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Alert,
  CircularProgress,
  Fab,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Tooltip
} from '@mui/material';
import {
  Add,
  Sync,
  Edit,
  Delete,
  MoreVert,
  Folder,
  Language,
  Schedule,
  OpenInNew,
  Refresh,
  Analytics,
  GitHub as GitHubIcon,
  Code as CodeIcon,
  CloudDownload as AzureIcon
} from '@mui/icons-material';
// import { useForm } from 'react-hook-form';
import apiService from '../../services/apiService';
import { Repository, ProcessingStatus, RepositoryStatus, ProviderType } from '../../types/api';

// interface AddRepositoryForm {
//   url: string;
//   name?: string;
//   description?: string;
// }

const Repositories: React.FC = () => {
  const navigate = useNavigate();
  const [repositories, setRepositories] = useState<Repository[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [addDialogOpen, setAddDialogOpen] = useState(false);
  const [menuAnchor, setMenuAnchor] = useState<null | HTMLElement>(null);
  const [selectedRepo, setSelectedRepo] = useState<Repository | null>(null);
  const [syncing, setSyncing] = useState<Set<number>>(new Set());

  const [formData, setFormData] = useState({
    url: '',
    name: '',
    description: ''
  });
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const loadRepositories = async () => {
    try {
      setLoading(true);
      setError(null);
      const repos = await apiService.getRepositories();
      setRepositories(repos);
    } catch (err: any) {
      setError(err.message || 'Failed to load repositories');
    } finally {
      setLoading(false);
    }
  };

  const validateForm = () => {
    const errors: Record<string, string> = {};
    
    if (!formData.url.trim()) {
      errors.url = 'Repository URL is required';
    } else if (!/^https?:\/\/.+/.test(formData.url)) {
      errors.url = 'Please enter a valid HTTP/HTTPS URL';
    }
    
    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleAddRepository = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) return;
    
    try {
      setIsSubmitting(true);
      await apiService.addRepository(formData.url, formData.name || undefined);
      setAddDialogOpen(false);
      setFormData({ url: '', name: '', description: '' });
      setFormErrors({});
      await loadRepositories();
    } catch (err: any) {
      setError(err.message || 'Failed to add repository');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleInputChange = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData(prev => ({ ...prev, [field]: e.target.value }));
    if (formErrors[field]) {
      setFormErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  const handleSyncRepository = async (repo: Repository) => {
    try {
      setSyncing(prev => new Set(prev.add(repo.id)));
      await apiService.syncRepository(repo.id);
      await loadRepositories(); // Refresh to show updated status
    } catch (err: any) {
      setError(err.message || 'Failed to sync repository');
    } finally {
      setSyncing(prev => {
        const newSet = new Set(prev);
        newSet.delete(repo.id);
        return newSet;
      });
    }
  };

  const handleDeleteRepository = async (repo: Repository) => {
    if (window.confirm(`Are you sure you want to delete "${repo.name}"? This action cannot be undone.`)) {
      try {
        await apiService.deleteRepository(repo.id);
        await loadRepositories();
      } catch (err: any) {
        setError(err.message || 'Failed to delete repository');
      }
    }
    handleMenuClose();
  };

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>, repo: Repository) => {
    setMenuAnchor(event.currentTarget);
    setSelectedRepo(repo);
  };

  const handleMenuClose = () => {
    setMenuAnchor(null);
    setSelectedRepo(null);
  };

  const getStatusColor = (status: RepositoryStatus): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    switch (status) {
      case RepositoryStatus.Active:
        return 'success';
      case RepositoryStatus.Syncing:
        return 'warning';
      case RepositoryStatus.Error:
        return 'error';
      case RepositoryStatus.Archived:
        return 'secondary';
      default:
        return 'default';
    }
  };

  const getProcessingStatusColor = (status: ProcessingStatus): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    switch (status) {
      case ProcessingStatus.Completed:
        return 'success';
      case ProcessingStatus.InProgress:
      case ProcessingStatus.Processing:
        return 'warning';
      case ProcessingStatus.Failed:
        return 'error';
      case ProcessingStatus.Pending:
        return 'info';
      default:
        return 'default';
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Never';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  const getRepositoryDisplayName = (repo: Repository) => {
    return repo.name || repo.url.split('/').pop()?.replace('.git', '') || 'Unknown';
  };

  const getProviderDisplayName = (providerType: ProviderType): string => {
    switch (providerType) {
      case ProviderType.GitHub:
        return 'GitHub';
      case ProviderType.GitLab:
        return 'GitLab';
      case ProviderType.Bitbucket:
        return 'Bitbucket';
      case ProviderType.AzureDevOps:
        return 'Azure DevOps';
      case ProviderType.Local:
        return 'Local';
      case ProviderType.Unknown:
      default:
        return 'Git';
    }
  };

  const getProviderIcon = (providerType: ProviderType) => {
    switch (providerType) {
      case ProviderType.GitHub:
        return <GitHubIcon fontSize="small" />;
      case ProviderType.GitLab:
        return <Language fontSize="small" />;
      case ProviderType.Bitbucket:
        return <CodeIcon fontSize="small" />;
      case ProviderType.AzureDevOps:
        return <AzureIcon fontSize="small" />;
      case ProviderType.Local:
        return <Folder fontSize="small" />;
      case ProviderType.Unknown:
      default:
        return <CodeIcon fontSize="small" />;
    }
  };

  const getProviderColor = (providerType: ProviderType): string => {
    switch (providerType) {
      case ProviderType.GitHub:
        return '#24292e';
      case ProviderType.GitLab:
        return '#fc6d26';
      case ProviderType.Bitbucket:
        return '#0052cc';
      case ProviderType.AzureDevOps:
        return '#0078d4';
      case ProviderType.Local:
        return '#757575';
      case ProviderType.Unknown:
      default:
        return '#1976d2';
    }
  };

  useEffect(() => {
    loadRepositories();
  }, []);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
        <CircularProgress size={60} />
      </Box>
    );
  }

  return (
    <Box>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h4" component="h1" gutterBottom>
            Repositories
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Manage your tracked repositories and their synchronization
          </Typography>
        </Box>
        <Button
          variant="outlined"
          startIcon={<Refresh />}
          onClick={loadRepositories}
          disabled={loading}
        >
          Refresh
        </Button>
      </Box>

      {/* Error Alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Repository Grid */}
      {repositories.length === 0 ? (
        <Card sx={{ textAlign: 'center', py: 6 }}>
          <CardContent>
            <Folder sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
            <Typography variant="h6" gutterBottom>
              No Repositories Found
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Add your first repository to start tracking code metrics and analytics.
            </Typography>
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => setAddDialogOpen(true)}
            >
              Add Repository
            </Button>
          </CardContent>
        </Card>
      ) : (
        <>
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: {
                xs: '1fr',
                sm: 'repeat(2, 1fr)',
                md: 'repeat(3, 1fr)'
              },
              gap: 3
            }}
          >
            {repositories.map((repo) => (
              <Card key={repo.id} sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
                  <CardContent sx={{ flexGrow: 1 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                      <Typography variant="h6" component="h3" noWrap sx={{ flexGrow: 1, mr: 1 }}>
                        {getRepositoryDisplayName(repo)}
                      </Typography>
                      <IconButton
                        size="small"
                        onClick={(e) => handleMenuOpen(e, repo)}
                      >
                        <MoreVert />
                      </IconButton>
                    </Box>

                    {repo.description && (
                      <Typography variant="body2" color="text.secondary" paragraph>
                        {repo.description}
                      </Typography>
                    )}

                    <Box sx={{ display: 'flex', gap: 1, mb: 2, flexWrap: 'wrap' }}>
                      <Chip
                        icon={getProviderIcon(repo.providerType)}
                        label={getProviderDisplayName(repo.providerType)}
                        size="small"
                        sx={{
                          backgroundColor: getProviderColor(repo.providerType),
                          color: 'white',
                          '& .MuiChip-icon': {
                            color: 'white'
                          }
                        }}
                      />
                      <Chip
                        label={RepositoryStatus[repo.status]}
                        color={getStatusColor(repo.status)}
                        size="small"
                      />
                      <Chip
                        label={ProcessingStatus[repo.processingStatus]}
                        color={getProcessingStatusColor(repo.processingStatus)}
                        size="small"
                        variant="outlined"
                      />
                    </Box>

                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mt: 2 }}>
                      <Tooltip title="Files">
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                          <Language sx={{ fontSize: 16, color: 'text.secondary' }} />
                          <Typography variant="caption" color="text.secondary">
                            {(repo as any).processedFiles?.toLocaleString() || '0'}
                          </Typography>
                        </Box>
                      </Tooltip>

                      <Tooltip title="Last Sync">
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                          <Schedule sx={{ fontSize: 16, color: 'text.secondary' }} />
                          <Typography variant="caption" color="text.secondary">
                            {formatDate(repo.lastSyncAt)}
                          </Typography>
                        </Box>
                      </Tooltip>
                    </Box>

                    {repo.syncErrorMessage && (
                      <Alert severity="error" sx={{ mt: 2 }} variant="outlined">
                        <Typography variant="caption">
                          {repo.syncErrorMessage}
                        </Typography>
                      </Alert>
                    )}
                  </CardContent>

                  <CardActions>
                    <Button
                      size="small"
                      startIcon={<Sync />}
                      onClick={() => handleSyncRepository(repo)}
                      disabled={syncing.has(repo.id) || repo.status === RepositoryStatus.Syncing}
                    >
                      {syncing.has(repo.id) ? 'Syncing...' : 'Sync'}
                    </Button>
                    <Button
                      size="small"
                      startIcon={<Analytics />}
                      onClick={() => navigate(`/repositories/${repo.id}`)}
                    >
                      Metrics
                    </Button>
                    <Button
                      size="small"
                      startIcon={<OpenInNew />}
                      href={repo.url}
                      target="_blank"
                      rel="noopener noreferrer"
                    >
                      Source
                    </Button>
                  </CardActions>
                </Card>
            ))}
          </Box>

          {/* Floating Action Button */}
          <Fab
            color="primary"
            aria-label="add repository"
            sx={{ position: 'fixed', bottom: 16, right: 16 }}
            onClick={() => setAddDialogOpen(true)}
          >
            <Add />
          </Fab>
        </>
      )}

      {/* Context Menu */}
      <Menu
        anchorEl={menuAnchor}
        open={Boolean(menuAnchor)}
        onClose={handleMenuClose}
      >
        <MenuItem onClick={() => selectedRepo && handleSyncRepository(selectedRepo)}>
          <ListItemIcon>
            <Sync fontSize="small" />
          </ListItemIcon>
          <ListItemText>Sync Repository</ListItemText>
        </MenuItem>
        <MenuItem onClick={handleMenuClose}>
          <ListItemIcon>
            <Edit fontSize="small" />
          </ListItemIcon>
          <ListItemText>Edit Repository</ListItemText>
        </MenuItem>
        <MenuItem onClick={() => selectedRepo && handleDeleteRepository(selectedRepo)}>
          <ListItemIcon>
            <Delete fontSize="small" />
          </ListItemIcon>
          <ListItemText>Delete Repository</ListItemText>
        </MenuItem>
      </Menu>

      {/* Add Repository Dialog */}
      <Dialog open={addDialogOpen} onClose={() => setAddDialogOpen(false)} maxWidth="sm" fullWidth>
        <form onSubmit={handleAddRepository}>
          <DialogTitle>Add Repository</DialogTitle>
          <DialogContent>
            <Typography variant="body2" color="text.secondary" paragraph>
              Add a Git repository to start tracking its metrics and analytics.
            </Typography>

            <TextField
              autoFocus
              margin="normal"
              label="Repository URL"
              fullWidth
              variant="outlined"
              value={formData.url}
              onChange={handleInputChange('url')}
              error={!!formErrors.url}
              helperText={formErrors.url || 'e.g., https://github.com/username/repository.git'}
              placeholder="https://github.com/username/repository.git"
            />

            <TextField
              margin="normal"
              label="Repository Name (Optional)"
              fullWidth
              variant="outlined"
              value={formData.name}
              onChange={handleInputChange('name')}
              error={!!formErrors.name}
              helperText={formErrors.name || 'Leave blank to auto-detect from URL'}
            />

            <TextField
              margin="normal"
              label="Description (Optional)"
              fullWidth
              variant="outlined"
              multiline
              rows={3}
              value={formData.description}
              onChange={handleInputChange('description')}
              error={!!formErrors.description}
              helperText={formErrors.description}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setAddDialogOpen(false)}>Cancel</Button>
            <Button
              type="submit"
              variant="contained"
              disabled={isSubmitting}
              startIcon={isSubmitting ? <CircularProgress size={20} /> : <Add />}
            >
              {isSubmitting ? 'Adding...' : 'Add Repository'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  );
};

export default Repositories;
