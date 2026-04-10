# CodeLens Platform Infrastructure Deployment Script
# Automates the setup of Kong API Gateway and monitoring

param(
    [string]$Environment = "development",
    [switch]$SkipKong = $false,
    [switch]$SkipMonitoring = $false,
    [switch]$DryRun = $false
)

Write-Host "🚀 CodeLens Platform Infrastructure Deployment" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Gray
Write-Host "Time: $(Get-Date)" -ForegroundColor Gray

# Check prerequisites
Write-Host "`n📋 Checking Prerequisites..." -ForegroundColor Yellow

$prerequisites = @{
    "kubectl" = "kubectl version --client"
    "helm" = "helm version"
    "docker" = "docker --version"
}

$missingTools = @()

foreach ($tool in $prerequisites.Keys) {
    try {
        $null = Invoke-Expression $prerequisites[$tool] -ErrorAction Stop
        Write-Host "✅ $tool is available" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ $tool is not available" -ForegroundColor Red
        $missingTools += $tool
    }
}

if ($missingTools.Count -gt 0) {
    Write-Host "`n💡 Missing tools: $($missingTools -join ', ')" -ForegroundColor Red
    Write-Host "Please install the missing tools and try again." -ForegroundColor Red
    exit 1
}

# Function to execute kubectl commands
function Invoke-KubernetesCommand {
    param($Command, $Description)
    
    if ($DryRun) {
        Write-Host "🔍 [DRY RUN] $Description" -ForegroundColor Magenta
        Write-Host "   Command: $Command" -ForegroundColor Gray
        return
    }
    
    Write-Host "🔧 $Description..." -ForegroundColor Blue
    try {
        $result = Invoke-Expression $Command -ErrorAction Stop
        Write-Host "✅ Success: $Description" -ForegroundColor Green
        return $result
    }
    catch {
        Write-Host "❌ Failed: $Description" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Gray
        throw
    }
}

# Deploy Kong API Gateway
if (-not $SkipKong) {
    Write-Host "`n🏗️  Deploying Kong API Gateway..." -ForegroundColor Yellow
    
    # Apply Kong configuration
    Invoke-KubernetesCommand "kubectl apply -f kong-gateway-setup.yaml" "Deploy Kong API Gateway configuration"
    
    # Wait for Kong to be ready
    if (-not $DryRun) {
        Write-Host "⏳ Waiting for Kong to be ready..." -ForegroundColor Blue
        Start-Sleep -Seconds 30
        
        # Check Kong status
        $kongPods = kubectl get pods -n codelens-platform -l app=kong-gateway -o jsonpath='{.items[*].status.phase}'
        Write-Host "Kong pod status: $kongPods" -ForegroundColor Gray
    }
}

# Deploy monitoring stack
if (-not $SkipMonitoring) {
    Write-Host "`n📊 Setting up Monitoring Stack..." -ForegroundColor Yellow
    
    # Add Prometheus Helm repository
    Invoke-KubernetesCommand "helm repo add prometheus-community https://prometheus-community.github.io/helm-charts" "Add Prometheus Helm repository"
    
    # Update Helm repositories
    Invoke-KubernetesCommand "helm repo update" "Update Helm repositories"
    
    # Create monitoring namespace
    Invoke-KubernetesCommand "kubectl create namespace monitoring --dry-run=client -o yaml | kubectl apply -f -" "Create monitoring namespace"
    
    # Install Prometheus stack
    $prometheusValues = @"
prometheus:
  prometheusSpec:
    serviceMonitorSelectorNilUsesHelmValues: false
    podMonitorSelectorNilUsesHelmValues: false
    retention: 30d
    storageSpec:
      volumeClaimTemplate:
        spec:
          accessModes: ["ReadWriteOnce"]
          resources:
            requests:
              storage: 10Gi

grafana:
  adminPassword: admin123
  service:
    type: LoadBalancer
  persistence:
    enabled: true
    size: 2Gi

alertmanager:
  alertmanagerSpec:
    storage:
      volumeClaimTemplate:
        spec:
          accessModes: ["ReadWriteOnce"]
          resources:
            requests:
              storage: 2Gi
"@
    
    $prometheusValues | Out-File -FilePath "prometheus-values.yaml" -Encoding UTF8
    
    Invoke-KubernetesCommand "helm upgrade --install kube-prometheus-stack prometheus-community/kube-prometheus-stack -n monitoring -f prometheus-values.yaml" "Install Prometheus monitoring stack"
}

# Create service placeholders
Write-Host "`n🎯 Creating Service Placeholders..." -ForegroundColor Yellow

$services = @(
    "portfolio-service",
    "repository-service",
    "ast-analysis-service",
    "auth-service",
    "search-service",
    "analytics-service",
    "file-service"
)

foreach ($service in $services) {
    $serviceYaml = @"
apiVersion: v1
kind: Service
metadata:
  name: $service
  namespace: codelens-platform
  labels:
    app: $service
    version: v1.0.0
spec:
  selector:
    app: $service
  ports:
  - port: 80
    targetPort: 8080
  type: ClusterIP
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: $service
  namespace: codelens-platform
  labels:
    app: $service
spec:
  replicas: 1
  selector:
    matchLabels:
      app: $service
  template:
    metadata:
      labels:
        app: $service
    spec:
      containers:
      - name: $service
        image: nginx:latest
        ports:
        - containerPort: 8080
        env:
        - name: SERVICE_NAME
          value: $service
        - name: ENVIRONMENT
          value: $Environment
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 10
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 15
          periodSeconds: 20
"@
    
    $serviceYaml | Out-File -FilePath "$service-placeholder.yaml" -Encoding UTF8
    Invoke-KubernetesCommand "kubectl apply -f $service-placeholder.yaml" "Create $service placeholder"
    
    # Clean up temporary file
    if (-not $DryRun) {
        Remove-Item "$service-placeholder.yaml" -Force -ErrorAction SilentlyContinue
    }
}

# Display deployment status
Write-Host "`n📊 Deployment Status Summary:" -ForegroundColor Cyan

if (-not $DryRun) {
    Write-Host "`n🔍 Checking Kong Gateway status..." -ForegroundColor Blue
    kubectl get pods -n codelens-platform -l app=kong-gateway
    
    Write-Host "`n🔍 Checking service status..." -ForegroundColor Blue
    kubectl get svc -n codelens-platform
    
    if (-not $SkipMonitoring) {
        Write-Host "`n🔍 Checking monitoring stack..." -ForegroundColor Blue
        kubectl get pods -n monitoring
        
        Write-Host "`n📊 Grafana Access Information:" -ForegroundColor Green
        Write-Host "   Username: admin" -ForegroundColor White
        Write-Host "   Password: admin123" -ForegroundColor White
        Write-Host "   URL: http://localhost:3000 (after port-forward)" -ForegroundColor White
        Write-Host "   Command: kubectl port-forward -n monitoring svc/kube-prometheus-stack-grafana 3000:80" -ForegroundColor Gray
    }
    
    Write-Host "`n🌐 Kong Gateway Access Information:" -ForegroundColor Green
    Write-Host "   Admin API: http://localhost:8001" -ForegroundColor White
    Write-Host "   Proxy: http://localhost:8000" -ForegroundColor White
    Write-Host "   Command: kubectl port-forward -n codelens-platform svc/kong-admin-service 8001:8001" -ForegroundColor Gray
}

Write-Host "`n🎉 Infrastructure deployment completed!" -ForegroundColor Cyan
Write-Host "`n🔧 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Extract Authentication Service from monolith" -ForegroundColor White
Write-Host "2. Extract AST Analysis Service for CPU-intensive operations" -ForegroundColor White
Write-Host "3. Set up database-per-service configurations" -ForegroundColor White
Write-Host "4. Implement service-to-service communication patterns" -ForegroundColor White
Write-Host "5. Configure monitoring and alerting rules" -ForegroundColor White

Write-Host "`n📚 Documentation:" -ForegroundColor Yellow
Write-Host "• CODELENS-TRANSFORMATION-STATUS.md - Complete transformation status" -ForegroundColor White
Write-Host "• IMMEDIATE-ACTION-ITEMS.md - 30-day microservices roadmap" -ForegroundColor White
Write-Host "• MICROSERVICES-ARCHITECTURE-ANALYSIS.md - Service decomposition plan" -ForegroundColor White

if ($DryRun) {
    Write-Host "`n💡 This was a dry run. To execute for real, run without -DryRun flag." -ForegroundColor Magenta
}
