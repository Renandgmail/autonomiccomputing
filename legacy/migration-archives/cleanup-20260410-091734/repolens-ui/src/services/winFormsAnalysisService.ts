import apiService from './apiService';

export interface AnalyzerStatus {
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

export interface DiscoveryRequest {
  sourcePath: string;
  formFilter?: string;
}

export interface DiscoveryResult {
  sourcePath: string;
  totalForms: number;
  forms: {
    formName: string;
    logicFile: string;
    designerFile: string;
    otherFiles: string[];
    hasDesigner: boolean;
    estimatedComplexity: 'low' | 'medium' | 'high';
  }[];
  recommendations: {
    startWithSimpleForms: string[];
    complexForms: string[];
  };
}

export interface AnalysisRequest {
  sourcePath: string;
  formFilter?: string;
  verbose?: boolean;
}

export interface AnalysisResult {
  success: boolean;
  summary: {
    projectPath: string;
    totalForms: number;
    analysisTime: string;
    costSavings: number;
    averageConfidence: number;
    aiUsageStats: string;
    overallSummary: string;
  };
  phases: {
    phaseNumber: number;
    phaseName: string;
    modelUsed: string;
    processingTime: string;
    costSaving: string;
    quality: string;
    confidence: number;
    warning?: string;
    processedWithAI: boolean;
    content: string;
    structuredData?: any;
  }[];
  extractionStats: {
    filesScanned: number;
    formsFound: number;
    controlsFound: number;
    eventHandlersFound: number;
    dbMethodsFound: number;
    validationMethodsFound: number;
    dbPatternsFound: number;
    totalMethodsScanned: number;
    totalLinesScanned: number;
    manualReviewFlags: string[];
  };
  recommendations: {
    immediate: string[];
    nextSteps: string[];
    costOptimization: string[];
  };
  error?: string;
}

export interface PhaseAnalysisRequest {
  sourcePath: string;
  phase: 'controls' | 'events' | 'database' | 'validation' | 'blueprint';
}

export interface CostAnalysisRequest {
  sourcePath: string;
}

export interface CostAnalysisResult {
  currentSetup: {
    aiModel: string;
    costPerAnalysis: number;
    privacyLevel: string;
    internetRequired: boolean;
  };
  projectedSavings: {
    perProject: {
      small: { traditional: number; enhanced: number; savings: number };
      medium: { traditional: number; enhanced: number; savings: number };
      large: { traditional: number; enhanced: number; savings: number };
      enterprise: { traditional: number; enhanced: number; savings: number };
    };
    annual: {
      lowUsage: { projects: number; savings: number };
      mediumUsage: { projects: number; savings: number };
      highUsage: { projects: number; savings: number };
    };
  };
  qualityComparison: {
    codeLlama: {
      accuracy: string;
      speedSeconds: string;
      privacyLevel: string;
      cost: string;
      internetRequired: boolean;
    };
    anthropic: {
      accuracy: string;
      speedSeconds: string;
      privacyLevel: string;
      cost: string;
      internetRequired: boolean;
    };
    fallback: {
      accuracy: string;
      speedSeconds: string;
      privacyLevel: string;
      cost: string;
      internetRequired: boolean;
    };
  };
  recommendations: string[];
}

export interface ExportRequest {
  sourcePath: string;
  format: 'json' | 'markdown' | 'csv';
}

export class WinFormsAnalysisService {
  private static readonly BASE_URL = '/api/winformsanalysis';

  /**
   * Get the current analyzer status and capabilities
   */
  static async getAnalyzerStatus(): Promise<AnalyzerStatus> {
    // Use the internal axios instance from apiService
    const response = await (apiService as any).api.get(`${this.BASE_URL}/status`);
    return response.data;
  }

  /**
   * Discover WinForms in a project directory
   */
  static async discoverWinForms(request: DiscoveryRequest): Promise<DiscoveryResult> {
    const response = await (apiService as any).api.post(`${this.BASE_URL}/discover`, request);
    return response.data;
  }

  /**
   * Run complete WinForms modernization analysis
   */
  static async analyzeProject(request: AnalysisRequest): Promise<AnalysisResult> {
    const response = await (apiService as any).api.post(`${this.BASE_URL}/analyze`, request);
    return response.data;
  }

  /**
   * Analyze a specific phase in detail
   */
  static async analyzePhase(request: PhaseAnalysisRequest): Promise<any> {
    const response = await (apiService as any).api.post(`${this.BASE_URL}/analyze-phase`, request);
    return response.data;
  }

  /**
   * Get detailed cost analysis and savings projection
   */
  static async getCostAnalysis(request: CostAnalysisRequest): Promise<CostAnalysisResult> {
    const response = await (apiService as any).api.post(`${this.BASE_URL}/cost-analysis`, request);
    return response.data;
  }

  /**
   * Export analysis results in various formats
   */
  static async exportAnalysis(request: ExportRequest): Promise<void> {
    const response = await (apiService as any).api.post(`${this.BASE_URL}/export`, request, {
      responseType: 'blob'
    });
    
    // Create download link
    const contentDisposition = response.headers['content-disposition'];
    const filename = contentDisposition 
      ? contentDisposition.split('filename=')[1]?.replace(/"/g, '') 
      : `winforms-analysis.${request.format}`;
    
    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  /**
   * Utility method to check if a path is valid for analysis
   */
  static async validateProjectPath(sourcePath: string): Promise<boolean> {
    try {
      const discovery = await this.discoverWinForms({ sourcePath });
      return discovery.totalForms > 0;
    } catch (error) {
      console.error('Path validation failed:', error);
      return false;
    }
  }

  /**
   * Get suggested project paths based on common locations
   */
  static getSuggestedProjectPaths(): string[] {
    return [
      'C:\\Projects',
      'C:\\Source',
      'C:\\Development',
      'D:\\Projects',
      'C:\\Users\\Public\\Documents',
      'C:\\Program Files',
      'C:\\inetpub\\wwwroot'
    ];
  }

  /**
   * Format analysis duration for display
   */
  static formatDuration(processingTime: string): string {
    // Convert various time formats to readable format
    const match = processingTime.match(/(\d+):(\d+):(\d+)/);
    if (match) {
      const [, hours, minutes, seconds] = match;
      if (parseInt(hours) > 0) return `${hours}h ${minutes}m ${seconds}s`;
      if (parseInt(minutes) > 0) return `${minutes}m ${seconds}s`;
      return `${seconds}s`;
    }
    
    // Handle milliseconds format
    const msMatch = processingTime.match(/(\d+)ms/);
    if (msMatch) {
      const ms = parseInt(msMatch[1]);
      if (ms < 1000) return `${ms}ms`;
      return `${(ms / 1000).toFixed(1)}s`;
    }
    
    return processingTime;
  }

  /**
   * Calculate modernization complexity score
   */
  static calculateComplexityScore(extractionStats: AnalysisResult['extractionStats']): {
    score: number;
    level: 'Low' | 'Medium' | 'High' | 'Very High';
    factors: string[];
  } {
    let score = 0;
    const factors: string[] = [];

    // Forms complexity
    if (extractionStats.formsFound > 20) {
      score += 3;
      factors.push('Many forms (>20)');
    } else if (extractionStats.formsFound > 10) {
      score += 2;
      factors.push('Multiple forms (>10)');
    } else if (extractionStats.formsFound > 5) {
      score += 1;
      factors.push('Several forms (>5)');
    }

    // Controls complexity
    const avgControlsPerForm = extractionStats.controlsFound / extractionStats.formsFound;
    if (avgControlsPerForm > 30) {
      score += 3;
      factors.push('Complex forms (>30 controls avg)');
    } else if (avgControlsPerForm > 15) {
      score += 2;
      factors.push('Moderate complexity forms');
    }

    // Database complexity
    if (extractionStats.dbMethodsFound > 50) {
      score += 3;
      factors.push('Heavy database usage');
    } else if (extractionStats.dbMethodsFound > 20) {
      score += 2;
      factors.push('Moderate database usage');
    } else if (extractionStats.dbMethodsFound > 0) {
      score += 1;
      factors.push('Some database usage');
    }

    // Event handler complexity
    const avgEventHandlersPerForm = extractionStats.eventHandlersFound / extractionStats.formsFound;
    if (avgEventHandlersPerForm > 20) {
      score += 2;
      factors.push('Many event handlers');
    } else if (avgEventHandlersPerForm > 10) {
      score += 1;
      factors.push('Moderate event handling');
    }

    // Manual review flags
    if (extractionStats.manualReviewFlags.length > 10) {
      score += 3;
      factors.push('Many manual review items');
    } else if (extractionStats.manualReviewFlags.length > 5) {
      score += 2;
      factors.push('Some manual review needed');
    } else if (extractionStats.manualReviewFlags.length > 0) {
      score += 1;
      factors.push('Minor manual review needed');
    }

    // Code size
    if (extractionStats.totalLinesScanned > 100000) {
      score += 3;
      factors.push('Large codebase (>100k lines)');
    } else if (extractionStats.totalLinesScanned > 50000) {
      score += 2;
      factors.push('Medium codebase (>50k lines)');
    } else if (extractionStats.totalLinesScanned > 20000) {
      score += 1;
      factors.push('Moderate codebase (>20k lines)');
    }

    const level = score <= 3 ? 'Low' : score <= 6 ? 'Medium' : score <= 10 ? 'High' : 'Very High';

    return { score, level, factors };
  }

  /**
   * Get modernization recommendations based on analysis results
   */
  static getModernizationRecommendations(result: AnalysisResult): {
    priority: 'High' | 'Medium' | 'Low';
    timeline: string;
    approach: string;
    riskLevel: 'Low' | 'Medium' | 'High';
    recommendations: string[];
  } {
    const complexity = this.calculateComplexityScore(result.extractionStats);
    const avgConfidence = result.summary.averageConfidence;
    
    let priority: 'High' | 'Medium' | 'Low' = 'Medium';
    let timeline = '3-6 months';
    let approach = 'Incremental migration';
    let riskLevel: 'Low' | 'Medium' | 'High' = 'Medium';
    const recommendations: string[] = [];

    // Determine priority based on complexity and confidence
    if (complexity.level === 'Low' && avgConfidence > 0.8) {
      priority = 'High';
      timeline = '1-2 months';
      riskLevel = 'Low';
      approach = 'Direct conversion';
      recommendations.push('Excellent candidate for automated migration');
      recommendations.push('Start immediately for quick wins');
    } else if (complexity.level === 'Very High' || avgConfidence < 0.5) {
      priority = 'Low';
      timeline = '12+ months';
      riskLevel = 'High';
      approach = 'Gradual rewrite';
      recommendations.push('Consider gradual rewrite instead of direct migration');
      recommendations.push('Focus on extracting business logic first');
    }

    // Add specific recommendations based on analysis
    if (result.summary.costSavings > 0) {
      recommendations.push(`Continue using CodeLlama to save $${result.summary.costSavings.toFixed(2)} per analysis`);
    }

    if (result.extractionStats.manualReviewFlags.length > 0) {
      recommendations.push(`Address ${result.extractionStats.manualReviewFlags.length} manual review items before migration`);
    }

    if (result.extractionStats.dbMethodsFound > result.extractionStats.formsFound * 3) {
      recommendations.push('Consider separating database logic into a dedicated API layer');
    }

    if (avgConfidence > 0.7) {
      recommendations.push('High confidence analysis - suitable for automation tools');
    }

    return { priority, timeline, approach, riskLevel, recommendations };
  }
}
