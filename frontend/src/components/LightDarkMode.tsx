'use client'

import { ColorMode } from "@/models/helpers";
import { createTheme, ThemeProvider } from "@mui/material";
import React, { PropsWithChildren } from "react";
import { ColorModeContext } from "./context/ColorModeContext";


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

    const MODE_KEY = 'LIGHT_DARK_MODE';

    const [mode, setMode] = React.useState<ColorMode>(global?.localStorage?.getItem(MODE_KEY) as ColorMode || 'light');
    const toggleMode = React.useCallback(
                () => {
                        const modeToSet = mode === 'light' ? 'dark' : 'light';
                        setMode(modeToSet);
                        try{
                            global?.localStorage?.setItem(MODE_KEY, modeToSet);
                        }catch{} },
                [mode]);
  
    // Update the theme only if the mode changes
    const theme = React.useMemo(() => getActiveTheme(mode), [mode]);

    return (
        <ColorModeContext.Provider value={{mode, toggleMode}}>
            <ThemeProvider theme={theme}>
                {children}
            </ThemeProvider>
        </ColorModeContext.Provider>);
}

export default LightDarkMode;