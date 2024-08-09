import { ColorModeContext } from '@/components/context/ColorModeContext';
import { useContext } from 'react';


export const useLightDarkModeSwitch = () => {
  return useContext(ColorModeContext);
};
