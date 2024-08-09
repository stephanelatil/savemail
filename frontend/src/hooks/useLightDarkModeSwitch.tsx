'use client'

import { ColorModeContext } from '@/components/context/ColorModeContext';
import { useContext } from 'react';


export const useLightDarkModeSwitch = () => useContext(ColorModeContext);
