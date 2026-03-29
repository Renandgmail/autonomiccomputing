# Code Graph Visualization Feature Documentation

## Overview

The Code Graph Visualization feature provides an interactive, comprehensive view of the complete codebase structure and relationships, showing how every component connects from UI to the deepest method level.

## Key Features

### 🌐 Complete Code Structure Mapping
- **Hierarchical Visualization**: Shows code structure from namespaces down to individual methods
- **Relationship Mapping**: Displays all connections between code components
- **No Orphaned Elements**: Every method and component is shown in context of its relationships
- **Multi-Layer Architecture**: Visualizes UI → Service → Repository → Entity relationships

### 🎯 Interactive Graph Controls
- **Multiple Layout Options**: Hierarchical, Force-directed, and Circular layouts
- **Advanced Filtering**: Filter by node types, visibility levels, complexity ranges
- **Search & Navigation**: Find specific components and navigate their relationships
- **Zoom & Pan**: Interactive exploration of large codebases

### 📊 Node Types & Color Coding
- **Namespace**: Blue - Top-level organizational units
- **Class**: Green - Class definitions and implementations
- **Interface**: Orange - Contract definitions
- **Method**: Purple - Function and method implementations
- **Service**: Pink - Service layer components
- **Controller**: Indigo - API controllers and endpoints
- **Entity**: Teal - Data models and entities
- **Repository**: Brown - Data access layer

### 🔍 Advanced Analysis Features
- **Circular Dependency Detection**: Identifies and highlights architectural issues
- **Orphan Node Detection**: Finds potentially unused code
- **Complexity Visualization**: Node size reflects code complexity
- **Relationship Strength**: Edge thickness shows connection importance
- **Fan In/Out Metrics**: Shows dependency patterns

## Technical Implementation

### Data Structure
```typescript
interface GraphNode {
  id: string;
  name: string;
  type: 'namespace' | 'class' | 'interface' | 'method' | 'service' | 'controller' | 'entity' | 'repository';
  level: number;                    // Hierarchical depth
  complexity: number;               // Complexity metrics
  dependencies: string[];           // Outgoing relationships
  dependents: string[];            // Incoming relationships
  filePath: string;                // Source file location
  metrics: {
    cyclomaticComplexity: number;
    linesOfCode: number;
    fanIn: number;                 // Number of incoming dependencies
    fanOut: number;                // Number of outgoing dependencies
  };
}

interface GraphEdge {
  id: string;
  source: string;
  target: string;
  type: 'calls' | 'implements' | 'extends' | 'uses' | 'contains';
  weight: number;                  // Relationship strength
  metadata: {
    callCount?: number;
    isDirectDependency: boolean;
  };
}
```

### API Integration
```typescript
// API endpoint for code graph data
GET /api/analytics/repository/{id}/code-graph

// Returns comprehensive graph structure with metadata
interface CodeGraph {
  nodes: GraphNode[];
  edges: GraphEdge[];
  metadata: {
    totalNodes: number;
    totalEdges: number;
    maxDepth: number;
    entryPoints: string[];          // Root nodes (Controllers, etc.)
    orphanNodes: string[];          // Unconnected nodes
    circularDependencies: string[][]; // Circular dependency cycles
  };
}
```

## Configuration Requirements

### Prerequisites
To enable Code Graph Visualization, the following analysis features must be enabled:

1. **AST Analysis** (Required)
   - Parses source code into Abstract Syntax Trees
   - Extracts structural information about classes, methods, interfaces

2. **Graph Construction** (Required)
   - Builds relationship maps between code entities
   - Creates dependency graphs and call hierarchies

3. **Indexing** (Recommended)
   - Optimizes search and navigation performance
   - Enables fast lookup of related components

### Configuration Steps
1. Navigate to Repository Details page
2. Click Settings (⚙️) button
3. Expand "Expert-Level Features" section
4. Enable the following options:
   - ✅ AST (Abstract Syntax Tree) Analysis
   - ✅ Code Relationship Graph Construction
   - ✅ Code Indexing for Search (optional but recommended)
5. Save configuration and run sync

## User Interface Features

### Main Visualization Area
- **Interactive SVG Canvas**: Scalable vector graphics for crisp visualization
- **Zoom Controls**: +/- buttons and reset to 100%
- **Pan & Navigate**: Click and drag to explore large graphs
- **Node Selection**: Click nodes to view detailed information

### Filter Controls
- **Node Type Selection**: Multi-select dropdown for component types
- **Visibility Filters**: Public, Protected, Internal, Private access levels
- **Complexity Range**: Slider to filter by cyclomatic complexity
- **Search Box**: Real-time search with autocomplete
- **Connection Filter**: Show only connected vs. all nodes

### Layout Options
1. **Hierarchical Layout**: Tree-like structure showing call hierarchies
2. **Force-Directed Layout**: Physics-based layout emphasizing relationships
3. **Circular Layout**: Radial arrangement highlighting central components

### Statistics Dashboard
- **Node Count**: Total and filtered component counts
- **Relationship Count**: Total edges and connections
- **Max Depth**: Deepest level in the hierarchy
- **Orphan Nodes**: Components with no relationships

## Quality Insights

### Architectural Analysis
- **Circular Dependencies**: Automatic detection of cycles that need refactoring
- **Orphan Components**: Identification of potentially unused code
- **Dependency Patterns**: Visual identification of tightly coupled components
- **Layer Violations**: Detection of improper cross-layer dependencies

### Code Quality Indicators
- **Node Size**: Proportional to lines of code and complexity
- **Color Intensity**: Indicates complexity or technical debt levels
- **Edge Thickness**: Shows relationship strength and usage frequency
- **Clustering**: Groups related components for better organization

## Performance Considerations

### Large Repository Support
- **Progressive Loading**: Loads graph incrementally for large codebases
- **Level-of-Detail**: Shows high-level structure first, details on demand
- **Filtering Performance**: Real-time filtering without re-rendering entire graph
- **Memory Management**: Efficient handling of large node/edge sets

### Scalability Features
- **Virtualization**: Only renders visible portions of large graphs
- **Caching**: Stores processed graph data for faster subsequent loads
- **Background Processing**: Graph generation doesn't block UI
- **Export Options**: Save graph data for external analysis

## Integration Benefits

### Development Workflow
- **Code Navigation**: Quick navigation to related components
- **Impact Analysis**: Understand effects of proposed changes
- **Refactoring Planning**: Visualize dependencies before restructuring
- **Architecture Review**: Visual validation of design patterns

### Team Collaboration
- **Onboarding**: New team members can quickly understand codebase structure
- **Knowledge Transfer**: Visual documentation of component relationships
- **Architecture Discussions**: Common visual reference for technical decisions
- **Code Reviews**: Context for understanding change impacts

### Quality Assurance
- **Dependency Validation**: Ensure proper layer separation
- **Dead Code Detection**: Identify components with no relationships
- **Complexity Monitoring**: Visual tracking of architectural complexity
- **Pattern Recognition**: Identify and enforce architectural patterns

## Future Enhancements

### Advanced Visualizations
- **3D Graph Rendering**: Three-dimensional representation of complex relationships
- **Time-based Animation**: Show evolution of relationships over time
- **Heat Maps**: Overlay metrics like change frequency or bug density
- **Virtual Reality**: Immersive exploration of large codebases

### Enhanced Analytics
- **Machine Learning**: Automatic pattern detection and recommendations
- **Predictive Analysis**: Forecast areas likely to require changes
- **Risk Assessment**: Identify components with high maintenance risk
- **Optimization Suggestions**: Recommend architectural improvements

### Collaboration Features
- **Annotations**: Add notes and comments to specific nodes
- **Sharing**: Export and share graph visualizations
- **Collaborative Filtering**: Save and share custom filter presets
- **Integration**: Connect with documentation and issue tracking systems

## Troubleshooting

### Common Issues
1. **Empty Graph Display**
   - Ensure AST Analysis is enabled in configuration
   - Verify repository sync has completed successfully
   - Check that supported programming languages are present

2. **Performance Issues**
   - Use filters to reduce node count for large repositories
   - Enable only necessary node types in filter settings
   - Consider upgrading browser for better SVG performance

3. **Missing Relationships**
   - Verify Graph Construction is enabled
   - Ensure all relevant file types are being analyzed
   - Check that dependencies are properly resolved

### Support Information
- **Required Features**: AST Analysis + Graph Construction
- **Supported Languages**: C#, TypeScript, JavaScript (extensible)
- **Browser Requirements**: Modern browsers with SVG support
- **Performance**: Optimized for repositories up to 100k+ lines of code

## API Documentation

### Endpoints
```
GET /api/analytics/repository/{id}/code-graph
  Returns: Complete code graph structure

GET /api/analytics/repository/{id}/code-graph/node/{nodeId}
  Returns: Detailed information for specific node

GET /api/analytics/repository/{id}/code-graph/relationships/{nodeId}
  Returns: All relationships for specific node
```

### Data Models
Complete TypeScript interfaces are defined in `CodeGraphVisualization.tsx` for:
- `GraphNode`: Individual code components
- `GraphEdge`: Relationships between components
- `CodeGraph`: Complete graph structure with metadata
- `GraphFilter`: User filter preferences

This comprehensive code graph visualization ensures that no method or component exists in isolation, providing complete visibility into the codebase structure and enabling better architectural understanding and decision-making.
