'use client'

import { ModalTypeMap } from "@mui/material";
// import { ColorMode } from "@/models/helpers";
import { createContext } from "react";



export type ColorModeEdit ={
    mode?:any,
    toggleMode: ()=>void
}

export const ColorModeContext = createContext<ColorModeEdit>({mode:'light', toggleMode:()=>{}});