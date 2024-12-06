'use client'

import { ColorMode } from "@/models/helpers";
import { createTheme, CssBaseline, PaletteOptions, ThemeProvider, TypeText } from "@mui/material";
import React, { PropsWithChildren } from "react";
import { ColorModeContext } from "./context/ColorModeContext";
import { useMountEffect } from "@/utils/utils";
import { SWRConfig } from "swr";

//module augmentation to 
declare module '@mui/material/styles' {
  interface TypeText {
      success: string,
      warning: string,
      error: string
  }
} 

const lightTheme = createTheme({
  palette: {
      mode: 'light',
      text: {
          primary: '#000000',
          secondary: '#666666',
          success: '#1b5e20', // darker green
          warning: '#b35900', // darker orange
          error: '#b71c1c', // darker red
      }
  },
  components: {
      MuiTypography: {
          styleOverrides: {
              root: ({ theme, ownerState }) => ({
                  ...(ownerState.color === 'success' && {
                      color: theme.palette.text.success,
                  }),
                  ...(ownerState.color === 'warning' && {
                      color: theme.palette.text.warning,
                  }),
                  ...(ownerState.color === 'error' && {
                      color: theme.palette.text.error,
                  }),
              }),
          },
      },
  },
})

const darkTheme = createTheme({
  palette: {
      mode: 'dark',
      text: {
          primary: '#ffffff',
          secondary: '#b3b3b3',
          success: '#66bb6a', // green
          warning: '#ffb74d', // orange
          error: '#ef5350', // red
      }
  },
  components: {
      MuiTypography: {
          styleOverrides: {
              root: ({ theme, ownerState }) => ({
                  ...(ownerState.color === 'success' && {
                      color: theme.palette.text.success,
                  }),
                  ...(ownerState.color === 'warning' && {
                      color: theme.palette.text.warning,
                  }),
                  ...(ownerState.color === 'error' && {
                      color: theme.palette.text.error,
                  }),
              }),
          },
      },
  },
})

function getActiveTheme(themeMode: 'light' | 'dark') {
    return themeMode === 'light' ? lightTheme : darkTheme;
}

const LightDarkMode:React.FC<PropsWithChildren> = ({children}) => {

    const MODE_KEY = 'LIGHT_DARK_MODE';
    const [mode, setMode] = React.useState<ColorMode>('dark');
    const toggleMode = React.useCallback(
                () => {
                        const modeToSet = mode === 'light' ? 'dark' : 'light';
                        setMode(modeToSet);
                        try{
                            window?.localStorage?.setItem(MODE_KEY, modeToSet);
                        }catch{} },
                [mode]);
  
    // Update the theme only if the mode changes
    const theme = React.useMemo(() => getActiveTheme(mode), [mode]);
    useMountEffect(()=>{
        var wantedMode = window?.localStorage?.getItem(MODE_KEY) as ColorMode;
        if (!!mode && mode !== wantedMode)
            setMode(wantedMode);
    });

    return (
        <ColorModeContext.Provider value={{mode, toggleMode}}>
            <ThemeProvider theme={theme}>
                <CssBaseline />
                <SWRConfig value={{
                        refreshWhenOffline:false,
                        revalidateOnFocus:false,
                        refreshWhenHidden:false,
                        shouldRetryOnError:false,
                        dedupingInterval: 60000,  // Cache data for 60 seconds
                        refreshInterval:0,
                  }}>
                  {children}
                </SWRConfig>
            </ThemeProvider>
        </ColorModeContext.Provider>);
}

export default LightDarkMode;