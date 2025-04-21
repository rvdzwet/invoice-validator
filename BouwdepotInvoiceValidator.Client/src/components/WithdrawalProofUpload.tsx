import React, { useState, useRef, useCallback, useEffect } from 'react';
import { 
  Box, 
  Button, 
  Modal, // Restore Modal
  Typography, 
  Paper, 
  Alert, 
  CircularProgress, 
  Divider,
  IconButton
} from '@mui/material';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import CameraAltIcon from '@mui/icons-material/CameraAlt';
import CloseIcon from '@mui/icons-material/Close';
import { apiService } from '../services/api';
import { ValidationContextView } from './ValidationContextView';
import { ComprehensiveWithdrawalProofResponse } from '../types/comprehensiveModels';

// Define interface for ValidationContext
interface ValidationContext {
  id: string;
  inputDocument: {
    fileName: string;
    fileSizeBytes: number;
    fileType: string;
    uploadTimestamp: string;
  };
  comprehensiveValidationResult: ComprehensiveWithdrawalProofResponse;
  overallOutcome: string;
  overallOutcomeSummary: string;
  processingSteps: Array<{
    stepName: string;
    description: string;
    status: string;
    timestamp: string;
  }>;
  issues: Array<{
    issueType: string;
    description: string;
    severity: string;
    field: string | null;
    timestamp: string;
    stackTrace: string | null;
  }>;
  aiModelsUsed: Array<{
    modelName: string;
    modelVersion: string;
    operation: string;
    tokenCount: number;
    timestamp: string;
  }>;
  validationResults: Array<{
    ruleId: string;
    ruleName: string;
    description: string;
    result: boolean;
    severity: string;
    message: string;
  }>;
  elapsedTime: string;
}

// Props interface
interface WithdrawalProofUploadProps {
  onValidationComplete?: (validationContext: ValidationContext) => void;
  onValidationStart?: () => void;
  onValidationError?: (errorMessage: string) => void;
}

/**
 * Component for uploading and validating withdrawal proof documents
 */
export const WithdrawalProofUpload: React.FC<WithdrawalProofUploadProps> = ({ 
  onValidationComplete,
  onValidationStart,
  onValidationError
}) => {
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [validationContext, setValidationContext] = useState<ValidationContext | null>(null);
  const [isCameraOpen, setIsCameraOpen] = useState(false);
  const [cameraStream, setCameraStream] = useState<MediaStream | null>(null);
  const [cameraError, setCameraError] = useState<string | null>(null);
  const [isVideoReady, setIsVideoReady] = useState(false); // New state

  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);

  /**
   * Handle file input change
   */
  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.files && event.target.files.length > 0) {
      setFile(event.target.files[0]);
      setError(null);
    }
    setError(null); // Clear file selection error if camera is used
    setCameraError(null); // Clear previous camera errors
  };

  /**
   * Opens the camera modal and requests camera access
   */
  const openCamera = useCallback(async () => {
    setCameraError(null);
    setIsVideoReady(false); // Reset video ready state
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
      console.log("Requesting camera access...");
      try {
        const stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' } });
        setCameraStream(stream); 
        setIsCameraOpen(true); 
        console.log("Camera stream obtained, opening modal.");
        // Stream attached in useEffect
      } catch (err) {
        console.error("Error accessing camera:", err);
        if (err instanceof Error) {
            if (err.name === "NotAllowedError") {
                setCameraError("Camera permission denied. Please enable camera access in your browser settings.");
            } else if (err.name === "NotFoundError") {
                setCameraError("No camera found. Please ensure a camera is connected and enabled.");
            } else {
                setCameraError(`Could not access camera: ${err.message}`);
            }
        } else {
             setCameraError("An unknown error occurred while accessing the camera.");
        }
        setIsCameraOpen(false); // Ensure modal doesn't open on error
      }
    } else {
      setCameraError("Camera access is not supported by this browser.");
    }
  }, []);

  /**
   * Closes the camera modal and stops the stream
   */
  const closeCamera = useCallback(() => {
    if (cameraStream) {
      cameraStream.getTracks().forEach(track => track.stop());
    }
    setCameraStream(null);
    setIsCameraOpen(false);
    setCameraError(null); // Clear errors when closing
  }, [cameraStream]);

  /**
   * Captures a photo from the video stream
   */
  const takePhoto = useCallback(() => {
    if (videoRef.current && canvasRef.current && cameraStream && isVideoReady) { // Check isVideoReady
      const video = videoRef.current;
      const canvas = canvasRef.current;
      const context = canvas.getContext('2d');
      // const A4_ASPECT_RATIO = Math.sqrt(2); // Removed A4 cropping
      // const OUTPUT_WIDTH = 1000; // Removed fixed width

      if (context) {
        console.log("takePhoto: Attempting capture (full frame)...");
        console.log(`takePhoto: Video offsetParent: ${video.offsetParent}`); // Check if element is considered visible
        console.log(`takePhoto: Video dimensions: ${video.videoWidth}x${video.videoHeight}`);
        console.log(`takePhoto: Video readyState: ${video.readyState}`);
        console.log(`takePhoto: Video paused: ${video.paused}`);
        console.log(`takePhoto: Video currentTime: ${video.currentTime}`);

        // Ensure video dimensions are valid before proceeding
        if (video.videoWidth <= 0 || video.videoHeight <= 0 || video.readyState < video.HAVE_CURRENT_DATA) { // Check readyState too
            console.error("takePhoto: Video not ready or dimensions invalid.", { width: video.videoWidth, height: video.videoHeight, readyState: video.readyState });
            setCameraError("Video data not available yet. Please wait a moment and try again.");
            return; 
        }
        
        // Restore A4 cropping with single rAF
        requestAnimationFrame(() => {
          console.log("takePhoto (rAF - Full Frame): Drawing frame...");
          try {
            const videoWidth = video.videoWidth;
            const videoHeight = video.videoHeight;

            // Set canvas dimensions to match video dimensions
            canvas.width = videoWidth;
            canvas.height = videoHeight;
            console.log(`takePhoto (rAF - Full Frame): Canvas dimensions set to ${canvas.width}x${canvas.height}`);

            // Draw the full video frame onto the canvas
            context.clearRect(0, 0, canvas.width, canvas.height);
            context.drawImage(
              video, 
              0, 0, videoWidth, videoHeight, // Source: full video frame
              0, 0, canvas.width, canvas.height // Destination: full canvas
            );
            console.log("takePhoto (rAF - Full Frame): Full drawImage completed.");

            // Convert canvas to Blob
            console.log("takePhoto (rAF - Full Frame): Converting canvas to blob...");
            canvas.toBlob((blob) => {
              if (blob) {
                console.log(`takePhoto (rAF - Full Frame): Blob created, size: ${blob.size} bytes`);
                const photoFile = new File([blob], `receipt-photo-${Date.now()}.jpg`, { type: 'image/jpeg' });
                setFile(photoFile); // Set the captured photo as the selected file
                setError(null); // Clear any previous file selection errors
                closeCamera();
              } else {
                console.error("takePhoto (rAF - Full Frame): Could not create blob from canvas.");
                setCameraError("Failed to capture photo data.");
              }
            }, 'image/jpeg', 0.9); // Keep quality setting

          } catch (drawError) {
              console.error("takePhoto (rAF - Full Frame): Error during drawImage:", drawError);
              setCameraError("An error occurred during photo capture.");
          }
        }); // End requestAnimationFrame

      } else {
         console.error("takePhoto: Could not get 2D context from canvas.");
         setCameraError("Failed to initialize photo capture context.");
      }
    } else {
        console.error("Camera stream, video reference, canvas reference not available, or video not ready.");
        setCameraError("Camera not ready or component error.");
    }
  }, [cameraStream, closeCamera, isVideoReady]); // Add isVideoReady dependency

  // Effect to handle attaching the stream to the video element when the modal opens
  useEffect(() => {
    console.log(`useEffect triggered: isCameraOpen=${isCameraOpen}, cameraStream=${!!cameraStream}, videoRef.current=${!!videoRef.current}`); // Log entry and state

    let currentVideoRef: HTMLVideoElement | null = null; // Initialize as null
    let handleCanPlay: (() => void) | undefined;
    let handlePlaying: (() => void) | undefined;
    let handleStalled: (() => void) | undefined;
    let handleSuspend: (() => void) | undefined;
    let timeoutId: number | undefined; // Store timeout ID for cleanup

    if (isCameraOpen && cameraStream) { // Check stream first
        // Use setTimeout to delay execution slightly, allowing the ref to populate
        timeoutId = window.setTimeout(() => {
            currentVideoRef = videoRef.current; // Re-check ref inside timeout
            if (!currentVideoRef) {
                console.error("useEffect (setTimeout): videoRef is still null after delay. Cannot attach stream.");
                setCameraError("Failed to initialize camera view. Please try closing and reopening the camera.");
                return;
            }

            console.log("useEffect (setTimeout): Attaching stream and listeners to video element", currentVideoRef);

            // Define event handlers within the scope where they are added
            handleCanPlay = () => console.log("Video Event: canplay");
            handlePlaying = () => console.log("Video Event: playing");
            handleStalled = () => console.log("Video Event: stalled");
            handleSuspend = () => console.log("Video Event: suspend");

            // Add listeners
            currentVideoRef.addEventListener('canplay', handleCanPlay);
            currentVideoRef.addEventListener('playing', handlePlaying);
            currentVideoRef.addEventListener('stalled', handleStalled);
            currentVideoRef.addEventListener('suspend', handleSuspend);

            if (currentVideoRef.srcObject !== cameraStream) {
                currentVideoRef.srcObject = cameraStream;
                console.log("useEffect (setTimeout): srcObject assigned.");
            } else {
                console.log("useEffect (setTimeout): srcObject already assigned.");
            }

            currentVideoRef.load();
            console.log("useEffect (setTimeout): load() called.");

            currentVideoRef.play().then(() => {
                console.log("useEffect (setTimeout): Video playback initiated successfully.");
            }).catch(err => {
                console.error("useEffect (setTimeout): Error initiating video playback:", err);
                setCameraError(`Could not play camera feed: ${err.message}. Check permissions/camera availability.`);
                setIsVideoReady(false);
            });
        }, 50); // Small delay (e.g., 50ms)

    } else if (isCameraOpen && !cameraStream) {
         console.warn("useEffect: Modal is open but stream is not yet available.");
    }

    // Cleanup function
    return () => {
        clearTimeout(timeoutId); // Clear the timeout if the effect cleans up before it runs

        // Use the ref value directly at the time of cleanup
        const videoElement = videoRef.current;
        if (videoElement) {
            console.log("useEffect cleanup: Removing event listeners and resources.");

            // Attempt to remove listeners - requires handlers to be accessible or defined consistently
            // If handlers were defined *only* inside setTimeout, direct removal here won't work.
            // A more robust pattern might use refs for handlers if needed across scopes.
            // For now, we rely on the fact that the element itself might be removed or srcObject cleared.
            // Example (might not work perfectly if handlers are scoped to timeout):
            // if (handleCanPlay) videoElement.removeEventListener('canplay', handleCanPlay);
            // if (handlePlaying) videoElement.removeEventListener('playing', handlePlaying);
            // ... etc ...

            // Clear srcObject
            if (videoElement.srcObject) {
                 console.log("useEffect cleanup: Clearing video srcObject.");
                 videoElement.srcObject = null;
            }
        }
        // Ensure stream is stopped regardless of video element state if it exists and modal is closing/component unmounting
        if (cameraStream) { // Check if stream exists
             // Check if the modal is intended to be closed OR if the component is unmounting (videoElement might be null then)
             if (!isCameraOpen || !videoElement) {
                 console.log("useEffect cleanup: Stopping stream tracks because modal is closing or component unmounting.");
                 cameraStream.getTracks().forEach(track => track.stop());
             }
        }
    };
  }, [isCameraOpen, cameraStream]); // Keep dependencies


  /**
   * Handle form submission
   */
  const handleSubmit = async () => {
    if (!file) {
      const errorMsg = 'Please select a file to upload';
      setError(errorMsg);
      if (onValidationError) {
        onValidationError(errorMsg);
      }
      return;
    }

    setLoading(true);
    setError(null);
    
    // Notify parent component that validation has started
    if (onValidationStart) {
      onValidationStart();
    }
    
    try {
      const result = await apiService.validateWithdrawalProof(file);
      console.log('Withdrawal proof validation result:', result);
      
      // Set local state
      setValidationContext(result);
      
      // Pass result to parent component if callback provided
      if (onValidationComplete) {
        onValidationComplete(result);
      }
    } catch (err) {
      console.error('Validation error:', err);
      const errorMsg = 'An error occurred during validation. Please try again.';
      setError(errorMsg);
      
      // Pass error to parent component if callback provided
      if (onValidationError) {
        onValidationError(errorMsg);
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box sx={{ maxWidth: 1200, mx: 'auto', p: 3 }}>
      <Typography variant="h4" gutterBottom>Construction Fund Withdrawal Validation</Typography>
      
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {cameraError && <Alert severity="warning" sx={{ mb: 2 }}>{cameraError}</Alert>}
      
      <Paper elevation={3} sx={{ p: 3, mb: 4 }}>
        <Typography variant="h5" gutterBottom>Submit Withdrawal Proof</Typography>
        <Divider sx={{ mb: 3 }} />
        
        <Typography variant="body1" sx={{ mb: 2 }}>
          Submit a construction-related invoice, receipt, or quotation for analysis.
          The system will determine eligibility for construction fund withdrawal.
        </Typography>
        
        <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', my: 3 }}>
          <input
            accept="application/pdf,image/*"
            style={{ display: 'none' }}
            id="file-upload"
            type="file"
            onChange={handleFileChange}
            disabled={loading} // Disable while loading
          />
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', alignItems: 'center' }}>
            <label htmlFor="file-upload">
              <Button 
                variant="contained" 
                component="span" 
                startIcon={<UploadFileIcon />}
                disabled={loading} // Disable while loading
              >
                Select Document
              </Button>
            </label>
            <Button 
              variant="outlined" 
              onClick={openCamera} 
              startIcon={<CameraAltIcon />}
              disabled={loading} // Disable while loading
            >
              Use Camera
            </Button>
          </Box>
          
          {file && (
            <Box sx={{ mt: 3, textAlign: 'center' }}>
              <Typography variant="body1">Selected file: {file.name}</Typography>
              <Button 
                variant="contained" 
                color="primary" 
                onClick={handleSubmit} 
                disabled={loading}
                sx={{ mt: 2 }}
              >
                {loading ? <CircularProgress size={24} /> : 'Validate Document'}
              </Button>
            </Box>
          )}
        </Box>
        
        <Typography variant="body2" color="text.secondary">
          Note: Each submission must contain exactly one document. Multiple documents combined into a single file
          will be rejected. Supported formats include PDF and common image formats.
        </Typography>
      </Paper>
      
      {/* Only render the ValidationContextView if we're not using the parent's callback */}
      {validationContext && !onValidationComplete && (
        <ValidationContextView validationContext={validationContext} />
      )}

      {/* Restore Camera Modal */}
      <Modal
        open={isCameraOpen}
        onClose={closeCamera}
        aria-labelledby="camera-modal-title"
        aria-describedby="camera-modal-description"
        sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}
      >
        <Paper sx={{ p: 3, position: 'relative', outline: 'none', maxWidth: '90vw', maxHeight: '90vh', overflow: 'auto' }}>
           <IconButton 
             aria-label="close camera"
             onClick={closeCamera}
             sx={{ position: 'absolute', top: 8, right: 8 }}
           >
             <CloseIcon />
           </IconButton>
          <Typography id="camera-modal-title" variant="h6" component="h2" gutterBottom>
            Take Photo
          </Typography>
          {cameraError && <Alert severity="error" sx={{ mb: 2 }}>{cameraError}</Alert>}
          <Box sx={{ position: 'relative', display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
            <video 
              ref={videoRef} 
              autoPlay 
              playsInline
              muted // Mute video to avoid feedback loops and improve autoplay chances
              style={{ maxWidth: '100%', maxHeight: '60vh', marginBottom: '16px', border: '1px solid lightgray', backgroundColor: '#000' }} // Added background color
              onLoadedData={() => {
                  console.log("onLoadedData: Video data loaded. Dimensions:", videoRef.current?.videoWidth, videoRef.current?.videoHeight);
                  setIsVideoReady(true); // Mark video as ready
                  setCameraError(null); // Clear errors on successful load
              }}
              onError={(e) => {
                  console.error("Video element error:", e);
                  setCameraError("Error displaying camera feed. Please ensure camera is connected and permissions are granted.");
                  setIsVideoReady(false); // Mark video as not ready on error
              }}
            />
            {/* Hidden canvas for capturing the image */}
            <canvas ref={canvasRef} style={{ display: 'none' }}></canvas>
            
            <Button 
              variant="contained" 
              color="primary" 
              onClick={takePhoto} 
              disabled={!cameraStream || !!cameraError || !isVideoReady} // Disable if no stream, error, or video not ready
              sx={{ mt: 2 }}
            >
              Take Photo
            </Button>
          </Box>
        </Paper>
      </Modal>
    </Box>
  );
};
