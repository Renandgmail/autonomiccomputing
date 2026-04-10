import React, { useState, useEffect, useRef } from 'react';
import { useLocation } from 'react-router-dom';
import {
  Box,
  Fab,
  Drawer,
  Paper,
  Typography,
  TextField,
  IconButton,
  Button,
  List,
  ListItem,
  ListItemText,
  Divider,
  CircularProgress,
  Alert,
  Collapse,
  useTheme,
  useMediaQuery,
  Chip,
  Avatar
} from '@mui/material';
import {
  Psychology as PsychologyIcon,
  Close as CloseIcon,
  Send as SendIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  AutoAwesome as AutoAwesomeIcon
} from '@mui/icons-material';

// AI Assistant interfaces
interface AIMessage {
  id: string;
  content: string;
  isUser: boolean;
  timestamp: Date;
  isExpanded?: boolean;
}

interface ScreenContext {
  screenType: 'L1' | 'L2' | 'L3' | 'L4';
  screenName: string;
  repositoryId?: number;
  repositoryName?: string;
  repositoryHealth?: number;
  fileContext?: {
    filePath: string;
    metrics: any;
    issues: any[];
  };
  hotspots?: any[];
  analyticsData?: any;
}

const AIAssistantOverlay: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const location = useLocation();
  
  const [isOpen, setIsOpen] = useState(false);
  const [isFirstSession, setIsFirstSession] = useState(false);
  const [messages, setMessages] = useState<AIMessage[]>([]);
  const [currentMessage, setCurrentMessage] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [screenContext, setScreenContext] = useState<ScreenContext | null>(null);
  
  const textFieldRef = useRef<HTMLInputElement>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Check if this is the first session
  useEffect(() => {
    const hasSeenAssistant = localStorage.getItem('repolens-ai-assistant-seen');
    if (!hasSeenAssistant) {
      setIsFirstSession(true);
      localStorage.setItem('repolens-ai-assistant-seen', 'true');
    }
  }, []);

  // Generate context based on current route
  useEffect(() => {
    const context = generateScreenContext();
    setScreenContext(context);
  }, [location.pathname]);

  // Auto-focus input when opened
  useEffect(() => {
    if (isOpen) {
      setTimeout(() => {
        textFieldRef.current?.focus();
      }, 100);
    }
  }, [isOpen]);

  // Scroll to bottom when new messages arrive
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  // Keyboard shortcuts
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen) {
        setIsOpen(false);
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen]);

  const generateScreenContext = (): ScreenContext => {
    const path = location.pathname;
    
    // L1 Portfolio Dashboard
    if (path === '/') {
      return {
        screenType: 'L1',
        screenName: 'Portfolio Dashboard'
      };
    }
    
    // L2 Repository Dashboard
    if (path.match(/^\/repos\/\d+$/)) {
      const repoId = parseInt(path.split('/')[2]);
      return {
        screenType: 'L2',
        screenName: 'Repository Dashboard',
        repositoryId: repoId,
        repositoryName: 'frontend-app', // Mock data
        repositoryHealth: 87
      };
    }
    
    // L3 Analytics
    if (path.match(/^\/repos\/\d+\/analytics/)) {
      const repoId = parseInt(path.split('/')[2]);
      return {
        screenType: 'L3',
        screenName: 'Analytics',
        repositoryId: repoId,
        repositoryName: 'frontend-app'
      };
    }
    
    // L3 Search
    if (path.match(/^\/repos\/\d+\/search/) || path === '/search') {
      return {
        screenType: 'L3',
        screenName: 'Universal Search',
        repositoryId: path.includes('/repos/') ? parseInt(path.split('/')[2]) : undefined
      };
    }
    
    // L3 Code Graph
    if (path.match(/^\/repos\/\d+\/graph/)) {
      const repoId = parseInt(path.split('/')[2]);
      return {
        screenType: 'L3',
        screenName: 'Code Graph',
        repositoryId: repoId,
        repositoryName: 'frontend-app'
      };
    }
    
    // L4 File Detail
    if (path.match(/^\/repos\/\d+\/files/)) {
      const repoId = parseInt(path.split('/')[2]);
      const filePath = decodeURIComponent(path.split('/files/')[1] || '');
      return {
        screenType: 'L4',
        screenName: 'File Detail',
        repositoryId: repoId,
        repositoryName: 'frontend-app',
        fileContext: {
          filePath,
          metrics: { qualityScore: 92, complexity: 7.2, technicalDebt: 2.4 },
          issues: [
            { severity: 'critical', type: 'SQL Injection', lineNumber: 47 },
            { severity: 'high', type: 'Weak Password Validation', lineNumber: 89 }
          ]
        }
      };
    }
    
    return {
      screenType: 'L1',
      screenName: 'Unknown Screen'
    };
  };

  const handleSendMessage = async () => {
    if (!currentMessage.trim() || isLoading) return;

    const userMessage: AIMessage = {
      id: `user-${Date.now()}`,
      content: currentMessage,
      isUser: true,
      timestamp: new Date()
    };

    setMessages(prev => [...prev, userMessage]);
    setCurrentMessage('');
    setIsLoading(true);

    try {
      // Generate AI response based on context
      const aiResponse = await generateAIResponse(currentMessage, screenContext);
      
      const assistantMessage: AIMessage = {
        id: `ai-${Date.now()}`,
        content: aiResponse,
        isUser: false,
        timestamp: new Date()
      };

      setMessages(prev => [...prev, assistantMessage]);
    } catch (error) {
      console.error('Error generating AI response:', error);
      
      const errorMessage: AIMessage = {
        id: `error-${Date.now()}`,
        content: 'I apologize, but I encountered an issue processing your request. Please try again.',
        isUser: false,
        timestamp: new Date()
      };
      
      setMessages(prev => [...prev, errorMessage]);
    } finally {
      setIsLoading(false);
    }
  };

  const generateAIResponse = async (message: string, context: ScreenContext | null): Promise<string> => {
    // Mock AI responses based on context and message content
    const lowerMessage = message.toLowerCase();
    
    if (!context) {
      return "I can see you're using RepoLens, but I need a moment to understand the current context. Could you please try again?";
    }

    // Context-aware responses
    if (context.screenType === 'L4' && context.fileContext) {
      if (lowerMessage.includes('complexity') || lowerMessage.includes('complex')) {
        return `This file (${context.fileContext.filePath}) has a complexity score of ${context.fileContext.metrics.complexity}/10, which is in the moderate range. This means the code has several decision points and could benefit from refactoring to improve maintainability.`;
      }
      
      if (lowerMessage.includes('issue') || lowerMessage.includes('problem')) {
        const criticalIssues = context.fileContext.issues.filter(i => i.severity === 'critical').length;
        const highIssues = context.fileContext.issues.filter(i => i.severity === 'high').length;
        
        return `I can see ${criticalIssues} critical and ${highIssues} high-severity issues in this file. The most urgent is the ${context.fileContext.issues[0]?.type} on line ${context.fileContext.issues[0]?.lineNumber}. I recommend addressing critical issues first as they pose security risks.`;
      }
      
      if (lowerMessage.includes('priorit') || lowerMessage.includes('what should i')) {
        return `For this file, I'd recommend prioritizing the critical security issues first, particularly the SQL injection vulnerability. After that, focus on reducing complexity through refactoring. The 2.4 hours of technical debt is manageable if addressed incrementally.`;
      }
    }

    if (context.screenType === 'L2') {
      if (lowerMessage.includes('health') || lowerMessage.includes('score')) {
        return `Your repository "${context.repositoryName}" has a health score of ${context.repositoryHealth}%, which is quite good. This indicates solid code quality with room for improvement. Focus on the quality hotspots shown in the main panel for the biggest impact.`;
      }
      
      if (lowerMessage.includes('priorit') || lowerMessage.includes('focus') || lowerMessage.includes('what should')) {
        return `Based on your repository dashboard, I'd suggest focusing on the top 3 quality hotspots first. These are files with the highest combination of complexity and change frequency, meaning they'll give you the most return on investment for quality improvements.`;
      }
    }

    if (context.screenType === 'L3') {
      if (context.screenName === 'Analytics') {
        return `The analytics view shows trends in your repository quality. Look for downward trends in key metrics and correlate them with recent changes. This helps identify which areas need attention and whether recent efforts are improving quality.`;
      }
      
      if (context.screenName === 'Code Graph') {
        return `The code graph visualization helps you understand architectural dependencies. Look for circular dependencies (highlighted in red) and orphaned modules. High complexity nodes with many connections are good candidates for refactoring.`;
      }
    }

    if (context.screenType === 'L1') {
      if (lowerMessage.includes('priorit') || lowerMessage.includes('focus')) {
        return `From your portfolio view, start with repositories showing red health indicators. These have the most critical issues that could impact delivery. Repositories with declining trends also need attention before issues compound.`;
      }
    }

    // General responses
    if (lowerMessage.includes('explain') || lowerMessage.includes('what is') || lowerMessage.includes('what does')) {
      if (lowerMessage.includes('quality')) {
        return 'Quality scores combine multiple factors: complexity, test coverage, security issues, and code patterns. Higher scores indicate more maintainable, secure code. Scores below 70% typically need attention.';
      }
      
      if (lowerMessage.includes('technical debt')) {
        return 'Technical debt represents the estimated time needed to bring code up to quality standards. It accumulates from shortcuts, outdated patterns, and deferred maintenance. Measured in hours, it helps prioritize refactoring efforts.';
      }
    }

    if (lowerMessage.includes('summary') || lowerMessage.includes('report')) {
      return `Based on your current ${context.screenName.toLowerCase()}, the main priority areas are quality improvements and security issue resolution. The metrics show generally healthy code with specific hotspots that need attention. Would you like me to elaborate on any particular area?`;
    }

    // Default helpful response
    return `I can help explain the metrics you're seeing, suggest priorities based on your current ${context.screenName.toLowerCase()}, or provide guidance on improving code quality. What specific aspect would you like to know more about?`;
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  const toggleMessageExpansion = (messageId: string) => {
    setMessages(prev => prev.map(msg => 
      msg.id === messageId ? { ...msg, isExpanded: !msg.isExpanded } : msg
    ));
  };

  // Don't render if we don't have context yet
  if (!screenContext) return null;

  return (
    <>
      {/* Floating AI Assistant Button */}
      <Fab
        color="primary"
        sx={{
          position: 'fixed',
          bottom: 24,
          right: 24,
          zIndex: 1000,
          animation: isFirstSession ? 'pulse 2s infinite' : 'none',
          '&.MuiButton-root': {
            animation: isFirstSession ? 'pulse 2s infinite' : 'none',
          }
        }}
        onClick={() => setIsOpen(true)}
      >
        {isFirstSession ? <AutoAwesomeIcon /> : <PsychologyIcon />}
      </Fab>

      {/* AI Assistant Panel */}
      <Drawer
        anchor={isMobile ? 'bottom' : 'right'}
        open={isOpen}
        onClose={() => setIsOpen(false)}
        variant="temporary"
        sx={{
          '& .MuiDrawer-paper': {
            width: isMobile ? '100%' : 380,
            height: isMobile ? '70vh' : '100vh',
            boxSizing: 'border-box',
          },
        }}
        ModalProps={{
          keepMounted: false,
        }}
      >
        <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
          {/* Header */}
          <Paper sx={{ p: 2, borderRadius: 0 }} elevation={1}>
            <Box display="flex" alignItems="center" justifyContent="space-between">
              <Box display="flex" alignItems="center" gap={2}>
                <Avatar sx={{ bgcolor: 'primary.main' }}>
                  <PsychologyIcon />
                </Avatar>
                <Box>
                  <Typography variant="h6">AI Assistant</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {screenContext.screenName} • {screenContext.repositoryName || 'RepoLens'}
                  </Typography>
                </Box>
              </Box>
              <IconButton onClick={() => setIsOpen(false)}>
                <CloseIcon />
              </IconButton>
            </Box>
          </Paper>

          {/* Context Indicator */}
          <Box sx={{ p: 2, borderBottom: '1px solid #eee' }}>
            <Typography variant="body2" color="text.secondary">
              I can see you're viewing: <strong>{screenContext.screenName}</strong>
              {screenContext.repositoryName && (
                <> in <strong>{screenContext.repositoryName}</strong></>
              )}
            </Typography>
          </Box>

          {/* Messages Area */}
          <Box sx={{ flexGrow: 1, overflow: 'auto', p: 2 }}>
            {messages.length === 0 && (
              <Alert severity="info" sx={{ mb: 2 }}>
                👋 Hi! I'm here to help explain metrics, suggest priorities, and answer questions about what you're seeing. Ask me anything about your code quality data!
              </Alert>
            )}

            <List disablePadding>
              {messages.map((message) => (
                <ListItem
                  key={message.id}
                  sx={{
                    flexDirection: 'column',
                    alignItems: message.isUser ? 'flex-end' : 'flex-start',
                    px: 0,
                    mb: 1
                  }}
                >
                  <Paper
                    elevation={1}
                    sx={{
                      p: 2,
                      maxWidth: '85%',
                      bgcolor: message.isUser ? 'primary.main' : 'grey.100',
                      color: message.isUser ? 'primary.contrastText' : 'text.primary',
                      borderRadius: 2,
                    }}
                  >
                    <Typography variant="body2">
                      {message.content.length > 150 && !message.isExpanded
                        ? `${message.content.slice(0, 150)}...`
                        : message.content
                      }
                    </Typography>
                    
                    {message.content.length > 150 && (
                      <Button
                        size="small"
                        onClick={() => toggleMessageExpansion(message.id)}
                        sx={{ 
                          mt: 1, 
                          color: message.isUser ? 'primary.contrastText' : 'primary.main',
                          p: 0,
                          minWidth: 'auto'
                        }}
                      >
                        {message.isExpanded ? 'Show less' : 'Tell me more'}
                        {message.isExpanded ? <ExpandLessIcon fontSize="small" /> : <ExpandMoreIcon fontSize="small" />}
                      </Button>
                    )}
                  </Paper>
                  
                  <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5 }}>
                    {message.timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </Typography>
                </ListItem>
              ))}
            </List>

            {isLoading && (
              <Box display="flex" alignItems="center" gap={2} sx={{ mt: 2 }}>
                <CircularProgress size={20} />
                <Typography variant="body2" color="text.secondary">
                  AI is thinking...
                </Typography>
              </Box>
            )}

            <div ref={messagesEndRef} />
          </Box>

          {/* Input Area */}
          <Paper sx={{ p: 2, borderRadius: 0 }} elevation={1}>
            <Box display="flex" gap={1} alignItems="flex-end">
              <TextField
                ref={textFieldRef}
                fullWidth
                multiline
                maxRows={3}
                value={currentMessage}
                onChange={(e) => setCurrentMessage(e.target.value)}
                onKeyPress={handleKeyPress}
                placeholder="Ask about metrics, priorities, or next steps..."
                variant="outlined"
                size="small"
                disabled={isLoading}
              />
              <IconButton
                color="primary"
                onClick={handleSendMessage}
                disabled={!currentMessage.trim() || isLoading}
              >
                <SendIcon />
              </IconButton>
            </Box>
            
            <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
              Press Enter to send • Shift+Enter for new line
            </Typography>
          </Paper>
        </Box>
      </Drawer>

      {/* CSS Keyframes for pulsing animation */}
      <style>
        {`
          @keyframes pulse {
            0% { transform: scale(1); }
            50% { transform: scale(1.1); }
            100% { transform: scale(1); }
          }
        `}
      </style>
    </>
  );
};

export default AIAssistantOverlay;
