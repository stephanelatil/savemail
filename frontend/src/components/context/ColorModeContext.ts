import { ColorMode } from "@/models/helpers";
import { createContext, useState } from "react";

export type ColorModeEdit ={
    mode:ColorMode,
    setMode: (mode:ColorMode)=>void
}

export const ColorModeContext = createContext<ColorModeEdit|null>(null);