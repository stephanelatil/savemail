'use client'

import { ColorMode } from "@/models/helpers";
import { createTheme, Experimental_CssVarsProvider, experimental_extendTheme, ThemeProvider, useColorScheme } from "@mui/material";
import React, { PropsWithChildren } from "react";
import { ColorModeContext } from "./context/ColorModeContext";

const theme = experimental_extendTheme({
    colorSchemes: {
      light: { // palette for light mode
        palette: {}
      },
      dark: { // palette for dark mode
        palette: {}
      }
    }
  })

// const lightTheme = createTheme({
//     palette: {
//         mode: 'light'
//     }
// })

// const darkTheme = createTheme({
//     palette: {
//         mode: 'dark'
//     }
// })

// function getActiveTheme(themeMode: 'light' | 'dark') {
//     return themeMode === 'light' ? theme : darkTheme;
// }

const LightDarkMode:React.FC<PropsWithChildren> = ({children}) => {

    const MODE_KEY = 'LIGHT_DARK_MODE';
    const {mode='dark', setMode, setColorScheme} = useColorScheme();
    // const [mode, setMode] = React.useState<ColorMode>(global?.localStorage?.getItem(MODE_KEY) as ColorMode || 'light');

    const toggleMode = React.useCallback(
                () => {
                        const modeToSet = mode === 'light' ? 'dark' : 'light';
                        setMode(modeToSet);
                        setColorScheme(modeToSet)
                    },
                [mode]);
  
    // Update the theme only if the mode changes
    // const theme = React.useMemo(() => getActiveTheme(mode), [mode]);

    return (
        <ColorModeContext.Provider value={{mode, toggleMode}}>
            <Experimental_CssVarsProvider theme={theme}>
                {children}
            </Experimental_CssVarsProvider>
        </ColorModeContext.Provider>);
}

export default LightDarkMode;