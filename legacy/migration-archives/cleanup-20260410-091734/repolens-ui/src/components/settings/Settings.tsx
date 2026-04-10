import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  TextField,
  Switch,
  FormControlLabel,
  Button,
  Divider,
  Alert,
  Snackbar,
  Avatar,
  Stack,
  Chip,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Paper,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  IconButton
} from '@mui/material';
import {
  Person,
  Notifications,
  Security,
  GitHub,
  Palette,
  Language,
  Schedule,
  Save,
  Edit,
  Delete,
  Add
} from '@mui/icons-material';

interface UserSettings {
  profile: {
    firstName: string;
    lastName: string;
    email: string;
    avatar?: string;
  };
  preferences: {
    theme: 'light' | 'dark' | 'auto';
    language: string;
    timezone: string;
    emailNotifications: boolean;
    pushNotifications: boolean;
    dashboardRefreshInterval: number;
  };
  github: {
    token?: string;
    username?: string;
    connectedAt?: string;
  };
  security: {
    twoFactorEnabled: boolean;
    lastPasswordChange?: string;
    activeSessions: number;
  };
}

const Settings: React.FC = () => {
  const [settings, setSettings] = useState<UserSettings>({
    profile: {
      firstName: 'John',
      lastName: 'Developer',
      email: 'john.developer@example.com',
    },
    preferences: {
      theme: 'light',
      language: 'en',
      timezone: 'UTC',
      emailNotifications: true,
      pushNotifications: false,
      dashboardRefreshInterval: 30,
    },
    github: {},
    security: {
      twoFactorEnabled: false,
      activeSessions: 1,
    },
  });

  const [isEditing, setIsEditing] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);
  const [showError, setShowError] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const handleSave = async () => {
    try {
      // TODO: Call API to save settings
      // await apiService.updateUserSettings(settings);
      console.log('Saving settings:', settings);
      setShowSuccess(true);
      setIsEditing(false);
    } catch (error) {
      setErrorMessage('Failed to save settings. Please try again.');
      setShowError(true);
    }
  };

  const handleProfileChange = (field: string, value: string) => {
    setSettings(prev => ({
      ...prev,
      profile: { ...prev.profile, [field]: value }
    }));
  };

  const handlePreferenceChange = (field: string, value: any) => {
    setSettings(prev => ({
      ...prev,
      preferences: { ...prev.preferences, [field]: value }
    }));
  };

  const handleSecurityToggle = (field: string, value: boolean) => {
    setSettings(prev => ({
      ...prev,
      security: { ...prev.security, [field]: value }
    }));
  };

  return (
    <Box sx={{ maxWidth: 1200, mx: 'auto', p: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Settings
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
        Manage your account settings and preferences
      </Typography>

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
        {/* First Row - Profile and Preferences */}
        <Box sx={{ 
          display: 'flex', 
          gap: 3, 
          flexDirection: { xs: 'column', md: 'row' }
        }}>
          {/* Profile Settings */}
          <Box sx={{ flex: 1 }}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                  <Person sx={{ mr: 2, color: 'primary.main' }} />
                  <Typography variant="h6">Profile Settings</Typography>
                  <Box sx={{ flexGrow: 1 }} />
                  <IconButton onClick={() => setIsEditing(!isEditing)}>
                    <Edit />
                  </IconButton>
                </Box>

                <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                  <Avatar 
                    sx={{ width: 80, height: 80, mr: 3 }}
                    src={settings.profile.avatar}
                  >
                    {settings.profile.firstName[0]}{settings.profile.lastName[0]}
                  </Avatar>
                  <Box>
                    <Typography variant="h6">
                      {settings.profile.firstName} {settings.profile.lastName}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {settings.profile.email}
                    </Typography>
                    <Chip 
                      label="Verified" 
                      color="success" 
                      size="small" 
                      sx={{ mt: 1 }} 
                    />
                  </Box>
                </Box>

                <Stack spacing={2}>
                  <TextField
                    label="First Name"
                    value={settings.profile.firstName}
                    onChange={(e) => handleProfileChange('firstName', e.target.value)}
                    disabled={!isEditing}
                    fullWidth
                  />
                  <TextField
                    label="Last Name"
                    value={settings.profile.lastName}
                    onChange={(e) => handleProfileChange('lastName', e.target.value)}
                    disabled={!isEditing}
                    fullWidth
                  />
                  <TextField
                    label="Email"
                    value={settings.profile.email}
                    onChange={(e) => handleProfileChange('email', e.target.value)}
                    disabled={!isEditing}
                    fullWidth
                    type="email"
                  />
                </Stack>
              </CardContent>
            </Card>
          </Box>

          {/* Preferences */}
          <Box sx={{ flex: 1 }}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                  <Palette sx={{ mr: 2, color: 'primary.main' }} />
                  <Typography variant="h6">Preferences</Typography>
                </Box>

                <Stack spacing={3}>
                  <FormControl fullWidth>
                    <InputLabel>Theme</InputLabel>
                    <Select
                      value={settings.preferences.theme}
                      onChange={(e) => handlePreferenceChange('theme', e.target.value)}
                      label="Theme"
                    >
                      <MenuItem value="light">Light</MenuItem>
                      <MenuItem value="dark">Dark</MenuItem>
                      <MenuItem value="auto">Auto (System)</MenuItem>
                    </Select>
                  </FormControl>

                  <FormControl fullWidth>
                    <InputLabel>Language</InputLabel>
                    <Select
                      value={settings.preferences.language}
                      onChange={(e) => handlePreferenceChange('language', e.target.value)}
                      label="Language"
                    >
                      <MenuItem value="en">English</MenuItem>
                      <MenuItem value="es">Español</MenuItem>
                      <MenuItem value="fr">Français</MenuItem>
                      <MenuItem value="de">Deutsch</MenuItem>
                    </Select>
                  </FormControl>

                  <TextField
                    label="Dashboard Refresh Interval (seconds)"
                    type="number"
                    value={settings.preferences.dashboardRefreshInterval}
                    onChange={(e) => handlePreferenceChange('dashboardRefreshInterval', parseInt(e.target.value))}
                    fullWidth
                    inputProps={{ min: 10, max: 300 }}
                  />

                  <FormControlLabel
                    control={
                      <Switch
                        checked={settings.preferences.emailNotifications}
                        onChange={(e) => handlePreferenceChange('emailNotifications', e.target.checked)}
                      />
                    }
                    label="Email Notifications"
                  />

                  <FormControlLabel
                    control={
                      <Switch
                        checked={settings.preferences.pushNotifications}
                        onChange={(e) => handlePreferenceChange('pushNotifications', e.target.checked)}
                      />
                    }
                    label="Push Notifications"
                  />
                </Stack>
              </CardContent>
            </Card>
          </Box>
        </Box>

        {/* Second Row - GitHub and Security */}
        <Box sx={{ 
          display: 'flex', 
          gap: 3, 
          flexDirection: { xs: 'column', md: 'row' }
        }}>
          {/* GitHub Integration */}
          <Box sx={{ flex: 1 }}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                  <GitHub sx={{ mr: 2, color: 'primary.main' }} />
                  <Typography variant="h6">GitHub Integration</Typography>
                </Box>

                {settings.github.token ? (
                  <Box>
                    <Alert severity="success" sx={{ mb: 2 }}>
                      GitHub account connected successfully!
                    </Alert>
                    <Typography variant="body2" color="text.secondary">
                      Connected as: <strong>@{settings.github.username}</strong>
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Connected: {settings.github.connectedAt}
                    </Typography>
                    <Button 
                      variant="outlined" 
                      color="error" 
                      sx={{ mt: 2 }}
                      startIcon={<Delete />}
                    >
                      Disconnect GitHub
                    </Button>
                  </Box>
                ) : (
                  <Box>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                      Connect your GitHub account to access private repositories and increase API rate limits.
                    </Typography>
                    <Button 
                      variant="contained" 
                      startIcon={<Add />}
                      onClick={() => {
                        // TODO: Implement GitHub OAuth flow
                        console.log('Connect GitHub clicked');
                      }}
                    >
                      Connect GitHub
                    </Button>
                  </Box>
                )}
              </CardContent>
            </Card>
          </Box>

          {/* Security Settings */}
          <Box sx={{ flex: 1 }}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                  <Security sx={{ mr: 2, color: 'primary.main' }} />
                  <Typography variant="h6">Security</Typography>
                </Box>

                <Stack spacing={3}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={settings.security.twoFactorEnabled}
                        onChange={(e) => handleSecurityToggle('twoFactorEnabled', e.target.checked)}
                      />
                    }
                    label="Two-Factor Authentication"
                  />

                  <Box>
                    <Typography variant="body2" color="text.secondary">
                      Active Sessions: {settings.security.activeSessions}
                    </Typography>
                    <Button variant="outlined" size="small" sx={{ mt: 1 }}>
                      View All Sessions
                    </Button>
                  </Box>

                  <Box>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                      Password last changed: {settings.security.lastPasswordChange || 'Never'}
                    </Typography>
                    <Button variant="outlined" size="small">
                      Change Password
                    </Button>
                  </Box>
                </Stack>
              </CardContent>
            </Card>
          </Box>
        </Box>

        {/* Third Row - Activity & Data */}
        <Box>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Account Activity & Data
              </Typography>
              <Box sx={{ 
                display: 'grid',
                gridTemplateColumns: { 
                  xs: '1fr', 
                  sm: '1fr 1fr', 
                  md: '1fr 1fr 1fr 1fr' 
                },
                gap: 2,
                mb: 3
              }}>
                <Paper sx={{ p: 2, textAlign: 'center' }}>
                  <Typography variant="h4" color="primary.main">
                    12
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Repositories Analyzed
                  </Typography>
                </Paper>
                <Paper sx={{ p: 2, textAlign: 'center' }}>
                  <Typography variant="h4" color="primary.main">
                    1,247
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    API Requests
                  </Typography>
                </Paper>
                <Paper sx={{ p: 2, textAlign: 'center' }}>
                  <Typography variant="h4" color="primary.main">
                    42
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Reports Generated
                  </Typography>
                </Paper>
                <Paper sx={{ p: 2, textAlign: 'center' }}>
                  <Typography variant="h4" color="primary.main">
                    7d
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Account Age
                  </Typography>
                </Paper>
              </Box>

              <Box sx={{ display: 'flex', gap: 2 }}>
                <Button variant="outlined">
                  Export Data
                </Button>
                <Button variant="outlined" color="error">
                  Delete Account
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Box>
      </Box>

      {/* Save Button */}
      {isEditing && (
        <Box sx={{ position: 'fixed', bottom: 24, right: 24, zIndex: 1000 }}>
          <Button
            variant="contained"
            size="large"
            startIcon={<Save />}
            onClick={handleSave}
            sx={{ 
              borderRadius: 8,
              px: 3,
              py: 1.5,
              fontSize: '1.1rem',
              boxShadow: '0 4px 20px rgba(0,0,0,0.15)'
            }}
          >
            Save Changes
          </Button>
        </Box>
      )}

      {/* Success Notification */}
      <Snackbar
        open={showSuccess}
        autoHideDuration={3000}
        onClose={() => setShowSuccess(false)}
      >
        <Alert severity="success" onClose={() => setShowSuccess(false)}>
          Settings saved successfully!
        </Alert>
      </Snackbar>

      {/* Error Notification */}
      <Snackbar
        open={showError}
        autoHideDuration={6000}
        onClose={() => setShowError(false)}
      >
        <Alert severity="error" onClose={() => setShowError(false)}>
          {errorMessage}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default Settings;
