'use client'

import { ColorMode } from "@/models/helpers";
import { createContext } from "react";



export type ColorModeEdit ={
    mode:ColorMode,
    toggleMode: ()=>void
}

export const ColorModeContext = createContext<ColorModeEdit>({mode:'light', toggleMode:()=>{}});