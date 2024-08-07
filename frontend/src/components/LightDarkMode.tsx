'use client'

import { createTheme, ThemeProvider } from "@mui/material";
import React, { PropsWithChildren } from "react";

type ColorMode = 'light' | 'dark'

const lightTheme = createTheme({
    palette: {
        mode: 'light'
    }
})

const darkTheme = createTheme({
    palette: {
        mode: 'dark'
    }
})

function getActiveTheme(themeMode: 'light' | 'dark') {
    return themeMode === 'light' ? lightTheme : darkTheme;
}

const LightDarkMode:React.FC<PropsWithChildren> = ({children}) => {

    const [mode, setMode] = React.useState<ColorMode>('light');
    const colorMode = React.useMemo(
      () => ({
        // The dark mode switch would invoke this method
        toggleColorMode: () => {
            setMode((prevMode: ColorMode) =>
                prevMode === 'light' ? 'dark' : 'light',
            );
            },
        }),
      [],
    );
  
    // Update the theme only if the mode changes
    const theme = React.useMemo(() => getActiveTheme(mode), [mode]);

    return (
        <ThemeProvider theme={theme}>
            {children}
        </ThemeProvider>);
}

export default LightDarkMode;