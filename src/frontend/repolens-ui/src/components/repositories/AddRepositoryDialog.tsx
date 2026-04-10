import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Box,
  Typography,
  Alert,
  CircularProgress,
  Chip,
  IconButton,
  Stepper,
  Step,
  StepLabel,
  StepContent,
  FormHelperText,
  InputAdornment
} from '@mui/material';
import {
  Close as CloseIcon,
  GitHub as GitHubIcon,
  Language as GitLabIcon,
  Folder as LocalIcon,
  Code as CodeIcon,
  CloudDownload as AzureIcon,
  FolderOpen as BrowseIcon,
  Visibility,
  VisibilityOff
} from '@mui/icons-material';
import { addRepository } from '../../services/apiService';

interface AddRepositoryDialogProps {
  open: boolean;
  onClose: () => void;
  onRepositoryAdded?: (repository: any) => void;
}

type ProviderType = 'GitHub' | 'GitLab' | 'Bitbucket' | 'AzureDevOps' | 'Local';

interface ProviderConfig {
  type: ProviderType;
  name: string;
  icon: React.ReactNode;
  color: string;
  urlPattern: RegExp;
  requiresAuth: boolean;
  authFields?: string[];
}

const PROVIDERS: ProviderConfig[] = [
  {
    type: 'GitHub',
    name: 'GitHub',
    icon: <GitHubIcon />,
    color: '#24292e',
    urlPattern: /^https?:\/\/(www\.)?github\.com\//i,
    requiresAuth: true,
    authFields: ['Personal Access Token']
  },
  {
    type: 'GitLab',
    name: 'GitLab',
    icon: <GitLabIcon />,
    color: '#fc6d26',
    urlPattern: /^https?:\/\/(www\.)?gitlab\.com\//i,
    requiresAuth: true,
    authFields: ['Personal Access Token']
  },
  {
    type: 'Bitbucket',
    name: 'Bitbucket',
    icon: <CodeIcon />,
    color: '#0052cc',
    urlPattern: /^https?:\/\/(www\.)?bitbucket\.org\//i,
    requiresAuth: true,
    authFields: ['App Password']
  },
  {
    type: 'AzureDevOps',
    name: 'Azure DevOps',
    icon: <AzureIcon />,
    color: '#0078d4',
    urlPattern: /^https?:\/\/dev\.azure\.com\//i,
    requiresAuth: true,
    authFields: ['Personal Access Token']
  },
  {
    type: 'Local',
    name: 'Local Repository',
    icon: <LocalIcon />,
    color: '#757575',
    urlPattern: /^(file:\/\/|[a-zA-Z]:\\|\/)/i,
    requiresAuth: false
  }
];

const AddRepositoryDialog: React.FC<AddRepositoryDialogProps> = ({
  open,
  onClose,
  onRepositoryAdded
}) => {
  const [activeStep, setActiveStep] = useState(0);
  const [url, setUrl] = useState('');
  const [name, setName] = useState('');
  const [selectedProvider, setSelectedProvider] = useState<ProviderType | ''>('');
  const [authToken, setAuthToken] = useState('');
  const [showToken, setShowToken] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<{ [key: string]: string }>({});

  const detectProviderFromUrl = (inputUrl: string): ProviderType | null => {
    for (const provider of PROVIDERS) {
      if (provider.urlPattern.test(inputUrl)) {
        return provider.type;
      }
    }
    return null;
  };

  const validateUrl = (inputUrl: string): boolean => {
    if (!inputUrl) return false;
    
    const detectedProvider = detectProviderFromUrl(inputUrl);
    if (!detectedProvider) return false;

    // Additional validation based on provider
    if (detectedProvider === 'Local') {
      return /^(file:\/\/|[a-zA-Z]:\\|\/)/i.test(inputUrl);
    } else {
      try {
        new URL(inputUrl);
        return true;
      } catch {
        return false;
      }
    }
  };

  const handleUrlChange = (value: string) => {
    setUrl(value);
    setError(null);
    setValidationErrors({});

    // Auto-detect provider
    const detected = detectProviderFromUrl(value);
    if (detected && detected !== selectedProvider) {
      setSelectedProvider(detected);
    }

    // Auto-generate name from URL
    if (value && !name) {
      const repoName = extractRepositoryName(value);
      if (repoName) {
        setName(repoName);
      }
    }
  };

  const extractRepositoryName = (repoUrl: string): string => {
    try {
      if (repoUrl.startsWith('file://') || /^[a-zA-Z]:\\/.test(repoUrl) || repoUrl.startsWith('/')) {
        // Local path - extract directory name
        const path = repoUrl.replace('file://', '');
        const segments = path.split(/[/\\]/);
        return segments[segments.length - 1] || segments[segments.length - 2] || '';
      }

      // Remote URL
      const url = new URL(repoUrl);
      const pathSegments = url.pathname.split('/').filter(Boolean);
      if (pathSegments.length >= 2) {
        return pathSegments[pathSegments.length - 1].replace('.git', '');
      }
    } catch {
      // Ignore URL parsing errors
    }
    return '';
  };

  const getSelectedProviderConfig = (): ProviderConfig | null => {
    return PROVIDERS.find(p => p.type === selectedProvider) || null;
  };

  const handleNext = () => {
    const errors: { [key: string]: string } = {};

    if (activeStep === 0) {
      // Validate URL and provider
      if (!url) {
        errors.url = 'Repository URL is required';
      } else if (!validateUrl(url)) {
        errors.url = 'Please enter a valid repository URL';
      }

      if (!selectedProvider) {
        errors.provider = 'Please select a provider';
      }

      if (!name.trim()) {
        errors.name = 'Repository name is required';
      }
    } else if (activeStep === 1) {
      const providerConfig = getSelectedProviderConfig();
      if (providerConfig?.requiresAuth && !authToken.trim()) {
        errors.authToken = `${providerConfig.authFields?.[0] || 'Authentication token'} is required`;
      }
    }

    setValidationErrors(errors);

    if (Object.keys(errors).length === 0) {
      if (activeStep === steps.length - 1) {
        handleSubmit();
      } else {
        setActiveStep(prev => prev + 1);
      }
    }
  };

  const handleBack = () => {
    setActiveStep(prev => prev - 1);
    setError(null);
    setValidationErrors({});
  };

  const handleSubmit = async () => {
    setLoading(true);
    setError(null);

    try {
      const repository = await addRepository(url, name || undefined);
      
      if (onRepositoryAdded) {
        onRepositoryAdded(repository);
      }
      
      handleClose();
    } catch (err: any) {
      setError(err.message || 'Failed to add repository');
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setActiveStep(0);
    setUrl('');
    setName('');
    setSelectedProvider('');
    setAuthToken('');
    setShowToken(false);
    setError(null);
    setValidationErrors({});
    onClose();
  };

  const handleBrowseLocal = async () => {
    try {
      // In a real implementation, this would open a directory picker
      // For now, show a help message
      setValidationErrors({
        url: 'Enter the full path to your local Git repository (e.g., C:\\Projects\\MyRepo or /home/user/repos/myrepo)'
      });
    } catch (err) {
      console.warn('Directory picker not available');
    }
  };

  const steps = ['Repository Details', 'Authentication', 'Review'];

  const renderProviderSelection = () => (
    <Box>
      <Typography variant="subtitle2" gutterBottom>
        Select Provider Type
      </Typography>
      <Box display="flex" flexWrap="wrap" gap={1} mb={2}>
        {PROVIDERS.map((provider) => (
          <Chip
            key={provider.type}
            icon={provider.icon as React.ReactElement}
            label={provider.name}
            variant={selectedProvider === provider.type ? 'filled' : 'outlined'}
            color={selectedProvider === provider.type ? 'primary' : 'default'}
            onClick={() => setSelectedProvider(provider.type)}
            sx={{
              backgroundColor: selectedProvider === provider.type ? provider.color : undefined,
              '&:hover': {
                backgroundColor: selectedProvider === provider.type 
                  ? provider.color 
                  : `${provider.color}20`
              }
            }}
          />
        ))}
      </Box>
      {validationErrors.provider && (
        <FormHelperText error>{validationErrors.provider}</FormHelperText>
      )}
    </Box>
  );

  const renderStepContent = (step: number) => {
    switch (step) {
      case 0:
        return (
          <Box sx={{ mt: 2 }}>
            {renderProviderSelection()}
            
            <TextField
              fullWidth
              label="Repository URL"
              value={url}
              onChange={(e) => handleUrlChange(e.target.value)}
              error={!!validationErrors.url}
              helperText={validationErrors.url || 'Enter the URL or path to your repository'}
              margin="normal"
              InputProps={{
                endAdornment: selectedProvider === 'Local' && (
                  <InputAdornment position="end">
                    <IconButton onClick={handleBrowseLocal} edge="end">
                      <BrowseIcon />
                    </IconButton>
                  </InputAdornment>
                )
              }}
            />

            <TextField
              fullWidth
              label="Repository Name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              error={!!validationErrors.name}
              helperText={validationErrors.name || 'A friendly name for this repository'}
              margin="normal"
            />

            {selectedProvider && (
              <Box mt={2}>
                <Alert severity="info">
                  <Typography variant="body2">
                    <strong>Provider:</strong> {getSelectedProviderConfig()?.name}
                    {getSelectedProviderConfig()?.requiresAuth && 
                      ' (Authentication required in next step)'}
                  </Typography>
                </Alert>
              </Box>
            )}
          </Box>
        );

      case 1:
        const providerConfig = getSelectedProviderConfig();
        if (!providerConfig?.requiresAuth) {
          return (
            <Box sx={{ mt: 2 }}>
              <Alert severity="success">
                No authentication required for local repositories.
              </Alert>
            </Box>
          );
        }

        return (
          <Box sx={{ mt: 2 }}>
            <Alert severity="info" sx={{ mb: 2 }}>
              <Typography variant="body2">
                <strong>{providerConfig?.name}</strong> requires authentication to access private repositories and increase API limits.
              </Typography>
            </Alert>

            <TextField
              fullWidth
              label={providerConfig?.authFields?.[0] || 'Authentication Token'}
              type={showToken ? 'text' : 'password'}
              value={authToken}
              onChange={(e) => setAuthToken(e.target.value)}
              error={!!validationErrors.authToken}
              helperText={validationErrors.authToken || `Enter your ${providerConfig?.name} ${providerConfig?.authFields?.[0]?.toLowerCase()}`}
              margin="normal"
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton onClick={() => setShowToken(!showToken)} edge="end">
                      {showToken ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                )
              }}
            />

            <Typography variant="caption" color="text.secondary" display="block" mt={1}>
              Your token is stored securely and only used for repository access.
            </Typography>
          </Box>
        );

      case 2:
        return (
          <Box sx={{ mt: 2 }}>
            <Typography variant="subtitle2" gutterBottom>
              Repository Summary
            </Typography>
            
            <Box display="flex" flexDirection="column" gap={1} mb={2}>
              <Box display="flex" justifyContent="space-between">
                <Typography variant="body2" color="text.secondary">Name:</Typography>
                <Typography variant="body2">{name}</Typography>
              </Box>
              <Box display="flex" justifyContent="space-between">
                <Typography variant="body2" color="text.secondary">URL:</Typography>
                <Typography variant="body2" sx={{ wordBreak: 'break-all' }}>{url}</Typography>
              </Box>
              <Box display="flex" justifyContent="space-between">
                <Typography variant="body2" color="text.secondary">Provider:</Typography>
                <Chip 
                  size="small" 
                  icon={getSelectedProviderConfig()?.icon as React.ReactElement | undefined}
                  label={getSelectedProviderConfig()?.name}
                  sx={{ backgroundColor: getSelectedProviderConfig()?.color, color: 'white' }}
                />
              </Box>
              <Box display="flex" justifyContent="space-between">
                <Typography variant="body2" color="text.secondary">Authentication:</Typography>
                <Typography variant="body2">
                  {getSelectedProviderConfig()?.requiresAuth 
                    ? (authToken ? 'Configured' : 'Not configured') 
                    : 'Not required'}
                </Typography>
              </Box>
            </Box>

            <Alert severity="info">
              <Typography variant="body2">
                RepoLens will analyze this repository and collect metrics when you click "Add Repository".
              </Typography>
            </Alert>
          </Box>
        );

      default:
        return null;
    }
  };

  return (
    <Dialog 
      open={open} 
      onClose={handleClose} 
      maxWidth="md" 
      fullWidth
      PaperProps={{
        sx: { minHeight: 500 }
      }}
    >
      <DialogTitle>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          Add Repository
          <IconButton onClick={handleClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent>
        <Stepper activeStep={activeStep} orientation="vertical">
          {steps.map((label, index) => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
              <StepContent>
                {renderStepContent(index)}
                
                {error && (
                  <Alert severity="error" sx={{ mt: 2 }}>
                    {error}
                  </Alert>
                )}

                <Box sx={{ mb: 2, mt: 3 }}>
                  <Button
                    variant="contained"
                    onClick={handleNext}
                    disabled={loading}
                    startIcon={loading ? <CircularProgress size={20} /> : undefined}
                  >
                    {index === steps.length - 1 ? 'Add Repository' : 'Next'}
                  </Button>
                  {index > 0 && (
                    <Button onClick={handleBack} sx={{ ml: 1 }} disabled={loading}>
                      Back
                    </Button>
                  )}
                </Box>
              </StepContent>
            </Step>
          ))}
        </Stepper>
      </DialogContent>
    </Dialog>
  );
};

export default AddRepositoryDialog;
