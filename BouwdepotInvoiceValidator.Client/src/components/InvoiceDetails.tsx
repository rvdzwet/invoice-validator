import React from 'react';
import { 
  Box, 
  Typography, 
  Table, 
  TableBody, 
  TableCell, 
  TableContainer, 
  TableHead, 
  TableRow, 
  Paper, 
  Divider
} from '@mui/material';
import { Invoice } from '../types/models';

interface InvoiceDetailsProps {
  invoice: Invoice;
}

/**
 * Component for displaying detailed invoice information
 */
const InvoiceDetails: React.FC<InvoiceDetailsProps> = ({ invoice }) => {
  // Format currency values
  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('nl-NL', { 
      style: 'currency', 
      currency: 'EUR' 
    }).format(value);
  };

  // Format date values
  const formatDate = (dateString: string | null): string => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('nl-NL');
  };

  return (
    <Box sx={{ mt: 2 }}>
      <Typography variant="h5" gutterBottom>
        Invoice Details
      </Typography>

      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 4, mb: 3 }}>
        <Box sx={{ minWidth: 200 }}>
          <Typography variant="subtitle2" color="text.secondary">
            Invoice Number
          </Typography>
          <Typography variant="body1">{invoice.invoiceNumber || 'N/A'}</Typography>
        </Box>
        
        <Box sx={{ minWidth: 200 }}>
          <Typography variant="subtitle2" color="text.secondary">
            Invoice Date
          </Typography>
          <Typography variant="body1">{formatDate(invoice.invoiceDate)}</Typography>
        </Box>
        
        <Box sx={{ minWidth: 200 }}>
          <Typography variant="subtitle2" color="text.secondary">
            Total Amount
          </Typography>
          <Typography variant="body1" fontWeight="bold">
            {formatCurrency(invoice.totalAmount)}
          </Typography>
        </Box>
        
        <Box sx={{ minWidth: 200 }}>
          <Typography variant="subtitle2" color="text.secondary">
            VAT Amount
          </Typography>
          <Typography variant="body1">{formatCurrency(invoice.vatAmount)}</Typography>
        </Box>
      </Box>

      <Divider sx={{ my: 2 }} />
      
      <Typography variant="h6" gutterBottom>
        Vendor Information
      </Typography>
      
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 4, mb: 3 }}>
        <Box sx={{ minWidth: 200 }}>
          <Typography variant="subtitle2" color="text.secondary">
            Vendor Name
          </Typography>
          <Typography variant="body1">{invoice.vendorName || 'N/A'}</Typography>
        </Box>
        
        <Box sx={{ minWidth: 200 }}>
          <Typography variant="subtitle2" color="text.secondary">
            Vendor Address
          </Typography>
          <Typography variant="body1">{invoice.vendorAddress || 'N/A'}</Typography>
        </Box>
        
        <Box sx={{ minWidth: 200 }}>
          <Typography variant="subtitle2" color="text.secondary">
            KVK Number
          </Typography>
          <Typography variant="body1">{invoice.vendorKvkNumber || 'N/A'}</Typography>
        </Box>
        
        <Box sx={{ minWidth: 200 }}>
          <Typography variant="subtitle2" color="text.secondary">
            BTW Number
          </Typography>
          <Typography variant="body1">{invoice.vendorBtwNumber || 'N/A'}</Typography>
        </Box>
      </Box>

      {invoice.lineItems && invoice.lineItems.length > 0 && (
        <>
          <Divider sx={{ my: 2 }} />
          
          <Typography variant="h6" gutterBottom>
            Line Items
          </Typography>
          
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Description</TableCell>
                  <TableCell align="right">Quantity</TableCell>
                  <TableCell align="right">Unit Price</TableCell>
                  <TableCell align="right">VAT Rate</TableCell>
                  <TableCell align="right">Total Price</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {invoice.lineItems.map((item, index) => (
                  <TableRow key={index}>
                    <TableCell>{item.description}</TableCell>
                    <TableCell align="right">{item.quantity}</TableCell>
                    <TableCell align="right">{formatCurrency(item.unitPrice)}</TableCell>
                    <TableCell align="right">{item.vatRate}%</TableCell>
                    <TableCell align="right">{formatCurrency(item.totalPrice)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </>
      )}

      <Divider sx={{ my: 2 }} />
      
      <Typography variant="h6" gutterBottom>
        File Information
      </Typography>
      
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 4 }}>
        <Box sx={{ minWidth: 200 }}>
          <Typography variant="subtitle2" color="text.secondary">
            File Name
          </Typography>
          <Typography variant="body1">{invoice.fileName || 'N/A'}</Typography>
        </Box>
        
        <Box sx={{ minWidth: 200 }}>
          <Typography variant="subtitle2" color="text.secondary">
            File Size
          </Typography>
          <Typography variant="body1">
            {invoice.fileSizeBytes ? `${Math.round(invoice.fileSizeBytes / 1024)} KB` : 'N/A'}
          </Typography>
        </Box>
        
        <Box sx={{ minWidth: 200 }}>
          <Typography variant="subtitle2" color="text.secondary">
            Creation Date
          </Typography>
          <Typography variant="body1">{formatDate(invoice.fileCreationDate)}</Typography>
        </Box>
        
        <Box sx={{ minWidth: 200 }}>
          <Typography variant="subtitle2" color="text.secondary">
            Modification Date
          </Typography>
          <Typography variant="body1">{formatDate(invoice.fileModificationDate)}</Typography>
        </Box>
      </Box>
    </Box>
  );
};

export default InvoiceDetails;
