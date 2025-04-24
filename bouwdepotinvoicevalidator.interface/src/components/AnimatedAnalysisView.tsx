import React, { useState, useEffect, useRef } from 'react';
import {
  Box,
  Typography,
  Paper,
  LinearProgress,
  Fade,
  Grow,
  Zoom,
  Divider,
  Stack,
  Chip,
  ThemeProvider,
  createTheme,
  useTheme
} from '@mui/material';
import AlgorithmIcon from '@mui/icons-material/AutoFixHigh';
import HighlightIcon from '@mui/icons-material/HighlightAlt';
import SearchIcon from '@mui/icons-material/Search';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import WarningIcon from '@mui/icons-material/Warning';
import ErrorIcon from '@mui/icons-material/Error';
import ThumbUpIcon from '@mui/icons-material/ThumbUp';
import ThumbDownIcon from '@mui/icons-material/ThumbDown';
import BuildIcon from '@mui/icons-material/Build';
import BallotIcon from '@mui/icons-material/Ballot';
import VerifiedUserIcon from '@mui/icons-material/VerifiedUser';
import AssessmentIcon from '@mui/icons-material/Assessment';

interface AnimatedAnalysisViewProps {
  imageUrl?: string;
  validationResult?: any; // Will be replaced with proper type
  isAnalyzing: boolean;
  onAnalysisComplete?: () => void;
}

// Analysis phases to show in animation
enum AnalysisPhase {
  INITIAL = 0,
  DOCUMENT_SCANNING = 1,
  TEXT_EXTRACTION = 2,
  VENDOR_VERIFICATION = 3,
  LINE_ITEM_ANALYSIS = 4,
  RULE_CHECKING = 5,
  CONFIDENCE_SCORING = 6,
  COMPLETED = 7
}

// Animation timings (milliseconds)
const PHASE_DURATIONS: Record<number, number> = {
  [AnalysisPhase.INITIAL]: 500,
  [AnalysisPhase.DOCUMENT_SCANNING]: 3000,
  [AnalysisPhase.TEXT_EXTRACTION]: 2500, 
  [AnalysisPhase.VENDOR_VERIFICATION]: 2000,
  [AnalysisPhase.LINE_ITEM_ANALYSIS]: 3000,
  [AnalysisPhase.RULE_CHECKING]: 2000,
  [AnalysisPhase.CONFIDENCE_SCORING]: 1500,
  [AnalysisPhase.COMPLETED]: 1000
};

// Mock evidence items to highlight during analysis
// In a real implementation, these would come from the backend
const MOCK_EVIDENCE_ITEMS = [
  { label: 'Invoice #', x: 20, y: 15, width: 80, height: 20, phase: AnalysisPhase.TEXT_EXTRACTION },
  { label: 'Vendor', x: 20, y: 50, width: 200, height: 30, phase: AnalysisPhase.VENDOR_VERIFICATION },
  { label: 'Line Item 1', x: 20, y: 150, width: 350, height: 25, phase: AnalysisPhase.LINE_ITEM_ANALYSIS },
  { label: 'Line Item 2', x: 20, y: 180, width: 350, height: 25, phase: AnalysisPhase.LINE_ITEM_ANALYSIS },
  { label: 'Total Amount', x: 250, y: 250, width: 100, height: 25, phase: AnalysisPhase.RULE_CHECKING },
];

/**
 * Component that shows an animated visualization of the AI analysis process
 */
const AnimatedAnalysisView: React.FC<AnimatedAnalysisViewProps> = ({
  imageUrl,
  validationResult,
  isAnalyzing,
  onAnalysisComplete
}) => {
  const theme = useTheme();
  const [currentPhase, setCurrentPhase] = useState<AnalysisPhase>(AnalysisPhase.INITIAL);
  const [phaseProgress, setPhaseProgress] = useState<number>(0);
  const [showScanLine, setShowScanLine] = useState<boolean>(false);
  const [scanLinePosition, setScanLinePosition] = useState<number>(0);
  const [highlights, setHighlights] = useState<any[]>([]);
  const [phaseMessages, setPhaseMessages] = useState<string[]>([]);
  const scanIntervalRef = useRef<NodeJS.Timeout | null>(null);
  const progressIntervalRef = useRef<NodeJS.Timeout | null>(null);
  const phaseTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  
  // Reset animation when isAnalyzing changes
  useEffect(() => {
    if (isAnalyzing) {
      // Start animation sequence
      resetAnimation();
      startAnalysisSequence();
    } else {
      // Clean up if animation is stopped externally
      cleanup();
    }
    
    return () => {
      cleanup();
    };
  }, [isAnalyzing]);
  
  // Clean up all intervals and timeouts
  const cleanup = () => {
    if (scanIntervalRef.current) {
      clearInterval(scanIntervalRef.current);
      scanIntervalRef.current = null;
    }
    
    if (progressIntervalRef.current) {
      clearInterval(progressIntervalRef.current);
      progressIntervalRef.current = null;
    }
    
    if (phaseTimeoutRef.current) {
      clearTimeout(phaseTimeoutRef.current);
      phaseTimeoutRef.current = null;
    }
  };
  
  // Reset animation state
  const resetAnimation = () => {
    setCurrentPhase(AnalysisPhase.INITIAL);
    setPhaseProgress(0);
    setShowScanLine(false);
    setScanLinePosition(0);
    setHighlights([]);
    setPhaseMessages([]);
  };
  
  // Start the analysis animation sequence
  const startAnalysisSequence = () => {
    // Start from initial phase
    advanceToNextPhase();
  };
  
  // Advance to the next phase in the sequence
  const advanceToNextPhase = () => {
    const nextPhase = currentPhase + 1;
    
    if (nextPhase > AnalysisPhase.COMPLETED) {
      // Animation complete
      if (onAnalysisComplete) {
        onAnalysisComplete();
      }
      return;
    }
    
    setCurrentPhase(nextPhase);
    setPhaseProgress(0);
    
    // Add phase message
    addPhaseMessage(getPhaseMessage(nextPhase));
    
    // Update highlights based on phase
    updateHighlightsForPhase(nextPhase);
    
    // Start scan line animation for document scanning phase
    if (nextPhase === AnalysisPhase.DOCUMENT_SCANNING) {
      startScanLineAnimation();
    } else {
      setShowScanLine(false);
      if (scanIntervalRef.current) {
        clearInterval(scanIntervalRef.current);
        scanIntervalRef.current = null;
      }
    }
    
    // Start progress animation for this phase
    startProgressAnimation(nextPhase);
    
    // Schedule next phase
    phaseTimeoutRef.current = setTimeout(() => {
      advanceToNextPhase();
    }, PHASE_DURATIONS[nextPhase]);
  };
  
  // Start scan line animation
  const startScanLineAnimation = () => {
    setShowScanLine(true);
    setScanLinePosition(0);
    
    scanIntervalRef.current = setInterval(() => {
      setScanLinePosition(prev => {
        if (prev >= 100) {
          return 0;
        }
        return prev + 2; // Speed of scan line movement
      });
    }, 50);
  };
  
  // Animate progress bar for current phase
  const startProgressAnimation = (phase: AnalysisPhase) => {
    if (progressIntervalRef.current) {
      clearInterval(progressIntervalRef.current);
    }
    
    const duration = PHASE_DURATIONS[phase as number];
    const interval = 50; // Update every 50ms
    const increment = (interval / duration) * 100;
    
    progressIntervalRef.current = setInterval(() => {
      setPhaseProgress(prev => {
        const next = prev + increment;
        if (next >= 100) {
          if (progressIntervalRef.current) {
            clearInterval(progressIntervalRef.current);
            progressIntervalRef.current = null;
          }
          return 100;
        }
        return next;
      });
    }, interval);
  };
  
  // Add a message to the phase messages list
  const addPhaseMessage = (message: string) => {
    setPhaseMessages(prev => [...prev, message]);
  };
  
  // Get message for the current phase
  const getPhaseMessage = (phase: AnalysisPhase): string => {
    switch (phase) {
      case AnalysisPhase.DOCUMENT_SCANNING:
        return "Scanning document...";
      case AnalysisPhase.TEXT_EXTRACTION:
        return "Extracting invoice data...";
      case AnalysisPhase.VENDOR_VERIFICATION:
        return "Verifying vendor information...";
      case AnalysisPhase.LINE_ITEM_ANALYSIS:
        return "Analyzing line items...";
      case AnalysisPhase.RULE_CHECKING:
        return "Checking compliance with Bouwdepot rules...";
      case AnalysisPhase.CONFIDENCE_SCORING:
        return "Calculating confidence scores...";
      case AnalysisPhase.COMPLETED:
        return "Analysis complete!";
      default:
        return "Preparing analysis...";
    }
  };
  
  // Update highlights based on the current phase
  const updateHighlightsForPhase = (phase: AnalysisPhase) => {
    // Filter evidence items relevant to this phase
    const relevantItems = MOCK_EVIDENCE_ITEMS.filter(item => item.phase <= phase);
    setHighlights(relevantItems);
  };
  
  // Get icon for the current phase
  const getPhaseIcon = (phase: AnalysisPhase) => {
    switch (phase) {
      case AnalysisPhase.DOCUMENT_SCANNING:
        return <SearchIcon />;
      case AnalysisPhase.TEXT_EXTRACTION:
        return <BallotIcon />;
      case AnalysisPhase.VENDOR_VERIFICATION:
        return <VerifiedUserIcon />;
      case AnalysisPhase.LINE_ITEM_ANALYSIS:
        return <AlgorithmIcon />;
      case AnalysisPhase.RULE_CHECKING:
        return <BuildIcon />;
      case AnalysisPhase.CONFIDENCE_SCORING:
        return <AssessmentIcon />;
      case AnalysisPhase.COMPLETED:
        return <CheckCircleIcon />;
      default:
        return <HighlightIcon />;
    }
  };
  
  // Render the current phase status
  const renderPhaseStatus = () => {
    return (
      <Box sx={{ mt: 2 }}>
        <Typography variant="h6" gutterBottom>
          {getPhaseMessage(currentPhase)}
        </Typography>
        
        <Box sx={{ display: 'flex', alignItems: 'center', mt: 1, mb: 1 }}>
          <Box sx={{ mr: 1, color: theme.palette.primary.main }}>
            {getPhaseIcon(currentPhase)}
          </Box>
          <Box sx={{ width: '100%' }}>
            <LinearProgress 
              variant="determinate" 
              value={phaseProgress} 
              sx={{ height: 8, borderRadius: 4 }}
            />
          </Box>
        </Box>
        
        <Typography variant="body2" color="text.secondary">
          Phase {currentPhase} of {AnalysisPhase.COMPLETED}
        </Typography>
      </Box>
    );
  };
  
  // Render the message log
  const renderMessageLog = () => {
    return (
      <Paper 
        elevation={0} 
        variant="outlined" 
        sx={{ 
          mt: 2, 
          p: 2, 
          maxHeight: 200, 
          overflow: 'auto',
          bgcolor: 'grey.50'
        }}
      >
        <Typography variant="subtitle2" gutterBottom>
          Analysis Log
        </Typography>
        
        <Box sx={{ pl: 1 }}>
          {phaseMessages.map((message, index) => (
            <Fade in key={index} timeout={(index + 1) * 500}>
              <Typography 
                variant="body2" 
                sx={{ 
                  py: 0.5, 
                  color: index === phaseMessages.length - 1 ? 'text.primary' : 'text.secondary',
                  fontWeight: index === phaseMessages.length - 1 ? 'medium' : 'normal'
                }}
              >
                {message}
              </Typography>
            </Fade>
          ))}
        </Box>
      </Paper>
    );
  };
  
  // Render the evidence highlights
  const renderHighlights = () => {
    if (!imageUrl) return null;
    
    return highlights.map((item, index) => (
      <Grow 
        in 
        key={index} 
        timeout={500}
      >
        <Box
          sx={{
            position: 'absolute',
            left: `${item.x}px`,
            top: `${item.y}px`,
            width: `${item.width}px`,
            height: `${item.height}px`,
            border: `2px solid ${theme.palette.primary.main}`,
            backgroundColor: `${theme.palette.primary.main}22`,
            borderRadius: 1,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 2
          }}
        >
          <Chip 
            label={item.label} 
            size="small" 
            color="primary" 
            sx={{ 
              position: 'absolute', 
              top: '-15px',
              fontSize: '0.7rem',
              height: '20px'
            }} 
          />
        </Box>
      </Grow>
    ));
  };
  
  // Render the scan line animation
  const renderScanLine = () => {
    if (!showScanLine || !imageUrl) return null;
    
    return (
      <Box
        sx={{
          position: 'absolute',
          left: 0,
          top: `${scanLinePosition}%`,
          width: '100%',
          height: '2px',
          backgroundColor: theme.palette.secondary.main,
          boxShadow: `0 0 10px ${theme.palette.secondary.main}`,
          zIndex: 3,
          opacity: 0.8
        }}
      />
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
          AI Analysis in Progress
        </Typography>
        
        <Divider sx={{ my: 2 }} />
        
        {/* Image with analysis visualization */}
        {imageUrl && (
          <Box 
            sx={{ 
              position: 'relative', 
              width: '100%', 
              height: 400, 
              backgroundColor: 'grey.100',
              borderRadius: 1,
              overflow: 'hidden',
              backgroundImage: `url(${imageUrl})`,
              backgroundSize: 'contain',
              backgroundPosition: 'center',
              backgroundRepeat: 'no-repeat'
            }}
          >
            {/* Scan line */}
            {renderScanLine()}
            
            {/* Evidence highlights */}
            {renderHighlights()}
          </Box>
        )}
        
        {/* Progress indicators */}
        {renderPhaseStatus()}
        
        {/* Message log */}
        {renderMessageLog()}
      </Paper>
    </Box>
  );
};

export default AnimatedAnalysisView;
