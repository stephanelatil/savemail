import { AlertColor } from '@mui/material/Alert';
import { useState, useCallback } from 'react';

const useSnackbar = () => {
  const [snackbar, setSnackbar] = useState<{isOpen:boolean, message:string, severity:AlertColor}>({ isOpen: false, message: '', severity: 'success' });

  const showSnackbar = useCallback((message:string, severity:AlertColor = 'success') => {
    setSnackbar({ isOpen: true, message, severity });
  }, []);

  const hideSnackbar = useCallback(() => {
    setSnackbar({ isOpen: false, message: '', severity: 'success' });
  }, []);

  return {
    snackbar,
    showSnackbar,
    hideSnackbar,
  };
};

export default useSnackbar;
