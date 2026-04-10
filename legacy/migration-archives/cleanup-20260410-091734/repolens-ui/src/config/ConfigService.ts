import appConfig from './app-config.json';

interface FrontendConfigData {
  api: {
    baseUrl: string;
    timeout: number;
    retryAttempts: number;
  };
  auth: {
    tokenStorageKey: string;
    refreshThreshold: number;
  };
  ui: {
    theme: string;
    pageSize: number;
    refreshInterval: number;
  };
}

class ConfigService {
  private config: FrontendConfigData;
  
  constructor() {
    const env = process.env.NODE_ENV || 'development';
    this.config = (appConfig as any)[env] as FrontendConfigData;
    
    if (!this.config) {
      console.warn(`[ConfigService] No configuration found for environment: ${env}, falling back to development`);
      this.config = (appConfig as any)['development'] as FrontendConfigData;
    }

    console.log('[ConfigService] Frontend initialized with config:', {
      env,
      apiBaseUrl: this.apiBaseUrl,
      theme: this.config.ui.theme
    });
  }
  
  get apiBaseUrl(): string {
    return this.config.api.baseUrl;
  }
  
  get apiUrl(): string {
    return this.config.api.baseUrl;
  }
  
  get apiConfig() {
    return this.config.api;
  }

  get authConfig() {
    return this.config.auth;
  }

  get uiConfig() {
    return this.config.ui;
  }

  // Legacy compatibility methods (can be removed later)
  get frontendUrl(): string {
    return window.location.origin;
  }
}

const configService = new ConfigService();
export default configService;
