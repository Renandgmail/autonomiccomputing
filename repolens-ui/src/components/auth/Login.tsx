import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  Alert,
  Container,
  Paper,
  Checkbox,
  FormControlLabel,
  Divider,
  CircularProgress
} from '@mui/material';
import { Lock, Person } from '@mui/icons-material';

import { LoginRequest } from '../../types/api';
import apiService from '../../services/apiService';

interface LoginFormData {
  email: string;
  password: string;
  rememberMe: boolean;
}

const Login: React.FC = () => {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors }
  } = useForm<LoginFormData>({
    defaultValues: {
      email: '',
      password: '',
      rememberMe: false
    }
  });

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true);
    setError(null);

    try {
      const loginRequest: LoginRequest = {
        email: data.email,
        password: data.password,
        rememberMe: data.rememberMe
      };

      console.log('[Login] Attempting login for:', data.email);
      const response = await apiService.login(loginRequest);
      
      console.log('[Login] Response received:', {
        success: response.success,
        hasToken: !!response.token,
        hasUser: !!response.user,
        debugInfo: response.debugInfo
      });

      if (response.success && response.token) {
        console.log('[Login] Login successful, redirecting to dashboard');
        
        // Verify token was stored properly
        const storedToken = localStorage.getItem('repolens_token');
        const authCheck = apiService.isAuthenticated();
        
        console.log('[Login] Post-login verification:', {
          tokenStored: !!storedToken,
          tokenLength: storedToken?.length,
          isAuthenticated: authCheck,
          user: response.user
        });
        
        if (authCheck) {
          console.log('[Login] Authentication verified, navigating to dashboard');
          // Navigate immediately since token is verified
          navigate('/dashboard', { replace: true });
        } else {
          console.error('[Login] Authentication verification failed after successful login');
          setError('Authentication verification failed. Please try again.');
        }
      } else {
        const errorMsg = response.errorMessage || 'Login failed';
        console.error('[Login] Login failed:', errorMsg);
        if (response.debugInfo) {
          console.error('[Login] Debug info:', response.debugInfo);
        }
        setError(errorMsg);
      }
    } catch (err: any) {
      console.error('[Login] Exception during login:', err);
      setError('An unexpected error occurred. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Container component="main" maxWidth="sm">
      <Box
        sx={{
          marginTop: 8,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        {/* Logo and Title */}
        <Paper
          elevation={0}
          sx={{
            display: 'flex',
            alignItems: 'center',
            mb: 4,
            p: 2,
            backgroundColor: 'transparent'
          }}
        >
          <Lock sx={{ fontSize: 40, color: 'primary.main', mr: 2 }} />
          <Typography component="h1" variant="h4" color="primary">
            RepoLens
          </Typography>
        </Paper>

        {/* Login Card */}
        <Card sx={{ width: '100%', maxWidth: 400 }}>
          <CardContent sx={{ p: 4 }}>
            <Box sx={{ textAlign: 'center', mb: 3 }}>
              <Typography component="h2" variant="h5" gutterBottom>
                Sign In
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Access your repository analytics dashboard
              </Typography>
            </Box>

            {error && (
              <Alert severity="error" sx={{ mb: 3 }}>
                {error}
              </Alert>
            )}

            <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ mt: 1 }}>
              <TextField
                {...register('email', {
                  required: 'Email is required',
                  pattern: {
                    value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                    message: 'Invalid email address'
                  }
                })}
                margin="normal"
                required
                fullWidth
                id="email"
                label="Email Address"
                name="email"
                autoComplete="email"
                autoFocus
                error={!!errors.email}
                helperText={errors.email?.message}
                InputProps={{
                  startAdornment: <Person sx={{ color: 'action.active', mr: 1 }} />
                }}
              />

              <TextField
                {...register('password', {
                  required: 'Password is required',
                  minLength: {
                    value: 6,
                    message: 'Password must be at least 6 characters'
                  }
                })}
                margin="normal"
                required
                fullWidth
                name="password"
                label="Password"
                type="password"
                id="password"
                autoComplete="current-password"
                error={!!errors.password}
                helperText={errors.password?.message}
                InputProps={{
                  startAdornment: <Lock sx={{ color: 'action.active', mr: 1 }} />
                }}
              />

              <FormControlLabel
                control={
                  <Checkbox
                    {...register('rememberMe')}
                    color="primary"
                  />
                }
                label="Remember me"
                sx={{ mt: 1 }}
              />

              <Button
                type="submit"
                fullWidth
                variant="contained"
                sx={{ mt: 3, mb: 2, py: 1.5 }}
                disabled={isLoading}
              >
                {isLoading ? (
                  <CircularProgress size={24} color="inherit" />
                ) : (
                  'Sign In'
                )}
              </Button>

              <Divider sx={{ my: 2 }} />

              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="body2" color="text.secondary">
                  Don't have an account?{' '}
                  <Link
                    to="/register"
                    style={{
                      color: '#1976d2',
                      textDecoration: 'none',
                      fontWeight: 600
                    }}
                  >
                    Sign up here
                  </Link>
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>

        {/* Footer */}
        <Typography variant="body2" color="text.secondary" align="center" sx={{ mt: 4 }}>
          © 2026 RepoLens. Autonomous Repository Analytics.
        </Typography>
      </Box>
    </Container>
  );
};

export default Login;
