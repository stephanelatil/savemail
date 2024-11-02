'use client'

import { AlertColor } from "@mui/material";
import { createContext } from "react";


export type NotificationContent = {
    message:string,
    severity:AlertColor,
    isShown:boolean
  };
  
export const NotificationContext = createContext<(message:string, severity:AlertColor) => void>((message:string,severity:AlertColor)=>{});