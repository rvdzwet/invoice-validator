import React, { useState, useRef } from 'react';
import {
  Box,
  Typography,
  Paper,
  Divider,
  Tooltip,
  Chip,
  IconButton,
  Popover,
  Card,
  CardContent,
  Stack,
  useTheme,
  Fade,
  Zoom,
  Button,
  ToggleButton,
  ToggleButtonGroup
} from '@mui/material';
import InfoIcon from '@mui/icons-material/Info';
import CloseIcon from '@mui/icons-material/Close';
import VisibilityIcon from '@mui/icons-material/Visibility';
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff';
import ZoomInIcon from '@mui/icons-material/ZoomIn';
import ZoomOutIcon from '@mui/icons-material/ZoomOut';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorIcon from '@mui/icons-material/Error';
import WarningIcon from '@mui/icons-material/Warning';
import HelpOutlineIcon from '@mui/icons-material/HelpOutline';
import FilterCenterFocusIcon from '@mui/icons-material/FilterCenterFocus';

// Types for evidence items to be displayed on the invoice
interface EvidenceItem {
  id: string;
  label: string;
  description: string;
  x: number;
  y: number;
  width: number;
  height: number;
  type: 'positive' | 'negative' | 'neutral';
  confidence: number;
  details?: string;
  rule?: string;
}

// Props for the component
interface AnnotatedInvoiceViewProps {
  imageUrl: string;
  evidenceItems: EvidenceItem[];
  explanations?: {
    summary: string;
    detailed: string;
    technical?: string;
  };
  title?: string;
}

/**
 * Component that displays an invoice image with interactive annotations
 * showing the AI's evidence and reasoning behind its decision
 */
const AnnotatedInvoiceView: React.FC<AnnotatedInvoiceViewProps> = ({
  imageUrl,
  evidenceItems,
  explanations = {
    summary: "AI analysis completed with annotations.",
    detailed: "The document has been analyzed and key elements have been identified."
  },
  title = "AI Decision Visualization"
}) => {
  const theme = useTheme();
  const [zoomLevel, setZoomLevel] = useState<number>(1);
  const [selectedEvidenceId, setSelectedEvidenceId] = useState<string | null>(null);
  const [anchors, setAnchors] = useState<Record<string, HTMLElement | null>>({});
  const [highlightFilter, setHighlightFilter] = useState<'all' | 'positive' | 'negative'>('all');
  const [showHighlights, setShowHighlights] = useState<boolean>(true);
  const [showExplanationPanel, setShowExplanationPanel] = useState<boolean>(true);
  const containerRef = useRef<HTMLDivElement>(null);

  // Handle zoom in/out
  const handleZoomIn = () => setZoomLevel(prev => Math.min(prev + 0.25, 3));
  const handleZoomOut = () => setZoomLevel(prev => Math.max(prev - 0.25, 0.5));
  const handleResetZoom = () => setZoomLevel(1);

  // Handle evidence selection
  const handleEvidenceClick = (id: string, element: HTMLElement) => {
    setSelectedEvidenceId(id);
    setAnchors(prev => ({ ...prev, [id]: element }));
  };

  // Handle closing the evidence popover
  const handleClosePopover = () => {
    setSelectedEvidenceId(null);
  };

  // Handle evidence filter change
  const handleFilterChange = (
    event: React.MouseEvent<HTMLElement>,
    newFilter: 'all' | 'positive' | 'negative' | null
  ) => {
    if (newFilter !== null) {
      setHighlightFilter(newFilter);
    }
  };

  // Get the selected evidence item
  const getSelectedEvidence = () => {
    return evidenceItems.find(item => item.id === selectedEvidenceId) || null;
  };

  // Filter evidence items based on current filter
  const filteredEvidenceItems = evidenceItems.filter(item => {
    if (!showHighlights) return false;
    if (highlightFilter === 'all') return true;
    return item.type === highlightFilter;
  });

  // Get color for evidence type
  const getEvidenceColor = (type: 'positive' | 'negative' | 'neutral') => {
    switch (type) {
      case 'positive':
        return theme.palette.success.main;
      case 'negative':
        return theme.palette.error.main;
      case 'neutral':
      default:
        return theme.palette.info.main;
    }
  };

  // Get icon for evidence type
  const getEvidenceIcon = (type: 'positive' | 'negative' | 'neutral') => {
    switch (type) {
      case 'positive':
        return <CheckCircleIcon color="success" />;
      case 'negative':
        return <ErrorIcon color="error" />;
      case 'neutral':
      default:
        return <InfoIcon color="info" />;
    }
  };

  // Render evidence highlights on the invoice
  const renderEvidenceHighlights = () => {
    return filteredEvidenceItems.map(item => (
      <Tooltip
        key={item.id}
        title={item.label}
        placement="top"
        arrow
      >
        <Box
          sx={{
            position: 'absolute',
            left: `${item.x}px`,
            top: `${item.y}px`,
            width: `${item.width}px`,
            height: `${item.height}px`,
            border: `2px solid ${getEvidenceColor(item.type)}`,
            backgroundColor: `${getEvidenceColor(item.type)}22`,
            borderRadius: 1,
            cursor: 'pointer',
            transition: 'all 0.2s ease',
            '&:hover': {
              backgroundColor: `${getEvidenceColor(item.type)}44`,
              boxShadow: `0 0 10px ${getEvidenceColor(item.type)}`
            },
            zIndex: item.id === selectedEvidenceId ? 3 : 2
          }}
          onClick={(e) => handleEvidenceClick(item.id, e.currentTarget)}
        >
          <Chip 
            icon={getEvidenceIcon(item.type)}
            label={item.label}
            size="small"
            sx={{ 
              position: 'absolute', 
              top: '-20px', 
              backgroundColor: getEvidenceColor(item.type),
              color: 'white',
              fontSize: '0.7rem',
              height: '24px',
              maxWidth: '150px',
              '.MuiChip-icon': {
                color: 'white'
              }
            }}
          />
        </Box>
      </Tooltip>
    ));
  };

  // Render the evidence detail popover
  const renderEvidencePopover = () => {
    const selectedEvidence = getSelectedEvidence();
    if (!selectedEvidence) return null;

    const anchor = anchors[selectedEvidence.id];
    
    return (
      <Popover
        open={!!selectedEvidenceId}
        anchorEl={anchor}
        onClose={handleClosePopover}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'center',
        }}
        transformOrigin={{
          vertical: 'top',
          horizontal: 'center',
        }}
        sx={{ mt: 1 }}
      >
        <Card sx={{ maxWidth: 320, minWidth: 280 }}>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
              <Typography variant="subtitle1" color={getEvidenceColor(selectedEvidence.type)}>
                {selectedEvidence.label}
              </Typography>
              <IconButton size="small" onClick={handleClosePopover}>
                <CloseIcon fontSize="small" />
              </IconButton>
            </Box>
            
            <Typography variant="body2" sx={{ mb: 1 }}>
              {selectedEvidence.description}
            </Typography>
            
            {selectedEvidence.details && (
              <>
                <Divider sx={{ my: 1 }} />
                <Typography variant="caption" color="text.secondary">
                  Details:
                </Typography>
                <Typography variant="body2" sx={{ mt: 0.5 }}>
                  {selectedEvidence.details}
                </Typography>
              </>
            )}

            {selectedEvidence.rule && (
              <>
                <Divider sx={{ my: 1 }} />
                <Typography variant="caption" color="text.secondary">
                  Applied Rule:
                </Typography>
                <Typography variant="body2" sx={{ mt: 0.5 }}>
                  {selectedEvidence.rule}
                </Typography>
              </>
            )}
            
            <Divider sx={{ my: 1 }} />
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="caption" color="text.secondary">
                Confidence:
              </Typography>
              <Chip 
                label={`${selectedEvidence.confidence}%`} 
                size="small"
                color={selectedEvidence.confidence > 75 ? "success" : 
                      selectedEvidence.confidence > 50 ? "info" : "warning"}
              />
            </Box>
          </CardContent>
        </Card>
      </Popover>
    );
  };

  // Render toolbar with controls
  const renderToolbar = () => {
    return (
      <Box sx={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        mb: 1,
        flexWrap: 'wrap',
        gap: 1
      }}>
        <Stack direction="row" spacing={1}>
          <IconButton 
            onClick={handleZoomIn} 
            size="small" 
            title="Zoom in"
          >
            <ZoomInIcon />
          </IconButton>
          <IconButton 
            onClick={handleZoomOut} 
            size="small" 
            title="Zoom out"
          >
            <ZoomOutIcon />
          </IconButton>
          <IconButton 
            onClick={handleResetZoom} 
            size="small" 
            title="Reset zoom"
          >
            <FilterCenterFocusIcon />
          </IconButton>
          <IconButton 
            onClick={() => setShowHighlights(!showHighlights)}
            size="small"
            title={showHighlights ? "Hide annotations" : "Show annotations"}
          >
            {showHighlights ? <VisibilityOffIcon /> : <VisibilityIcon />}
          </IconButton>
        </Stack>
        
        <ToggleButtonGroup
          value={highlightFilter}
          exclusive
          onChange={handleFilterChange}
          aria-label="highlight filter"
          size="small"
        >
          <ToggleButton value="all" aria-label="all highlights">
            All
          </ToggleButton>
          <ToggleButton value="positive" aria-label="positive highlights">
            Positive
          </ToggleButton>
          <ToggleButton value="negative" aria-label="negative highlights">
            Negative
          </ToggleButton>
        </ToggleButtonGroup>
      </Box>
    );
  };

  // Render explanation panel
  const renderExplanationPanel = () => {
    if (!showExplanationPanel) return null;
    
    return (
      <Paper 
        elevation={0} 
        variant="outlined" 
        sx={{ 
          mt: 2, 
          p: 2, 
          bgcolor: 'background.paper',
          borderRadius: 1,
          position: 'relative'
        }}
      >
        <Box sx={{ 
          display: 'flex', 
          justifyContent: 'space-between', 
          alignItems: 'flex-start',
          mb: 1
        }}>
          <Typography variant="h6">
            AI Analysis Explanation
          </Typography>
          <IconButton 
            size="small" 
            onClick={() => setShowExplanationPanel(false)}
            sx={{ mt: -1, mr: -1 }}
          >
            <CloseIcon fontSize="small" />
          </IconButton>
        </Box>
        
        <Divider sx={{ mb: 2 }} />
        
        <Typography variant="subtitle1" gutterBottom>
          Summary
        </Typography>
        <Typography variant="body2" paragraph>
          {explanations.summary}
        </Typography>
        
        <Typography variant="subtitle1" gutterBottom>
          Detailed Explanation
        </Typography>
        <Typography variant="body2" paragraph>
          {explanations.detailed}
        </Typography>
        
        {explanations.technical && (
          <>
            <Divider sx={{ my: 2 }} />
            <Typography variant="subtitle1" gutterBottom>
              Technical Details
            </Typography>
            <Typography variant="body2" sx={{ 
              fontFamily: 'monospace', 
              backgroundColor: 'grey.100',
              p: 1, 
              borderRadius: 1,
              fontSize: '0.8rem' 
            }}>
              {explanations.technical}
            </Typography>
          </>
        )}
      </Paper>
    );
  };

  return (
    <Box>
      <Paper 
        sx={{ 
          p: 2, 
          mb: 2,
          borderTop: 4,
          borderColor: 'primary.main'
        }}
      >
        <Typography variant="h5" gutterBottom>
          {title}
          <Button 
            variant="text" 
            size="small" 
            startIcon={<InfoIcon />}
            onClick={() => setShowExplanationPanel(!showExplanationPanel)}
            sx={{ ml: 2 }}
          >
            {showExplanationPanel ? 'Hide Explanation' : 'Show Explanation'}
          </Button>
        </Typography>
        
        <Divider sx={{ mb: 2 }} />
        
        {/* Toolbar */}
        {renderToolbar()}
        
        {/* Invoice image with annotations */}
        <Box 
          ref={containerRef}
          sx={{ 
            position: 'relative', 
            width: '100%', 
            height: 600, 
            backgroundColor: 'grey.100',
            borderRadius: 1,
            overflow: 'auto'
          }}
        >
          <Box
            sx={{
              position: 'relative',
              transformOrigin: 'top left',
              transform: `scale(${zoomLevel})`,
              width: 'fit-content',
              height: 'fit-content',
              backgroundImage: `url(${imageUrl})`,
              backgroundSize: 'contain',
              backgroundPosition: 'top left',
              backgroundRepeat: 'no-repeat'
            }}
          >
            {/* Render evidence highlights on the invoice */}
            {renderEvidenceHighlights()}
          </Box>
        </Box>
        
        {/* Render the evidence details popover */}
        {renderEvidencePopover()}
        
        {/* Explanation panel */}
        {renderExplanationPanel()}
      </Paper>
    </Box>
  );
};

export default AnnotatedInvoiceView;
