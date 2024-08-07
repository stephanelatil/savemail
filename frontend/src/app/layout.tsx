import { createTheme, CssBaseline } from '@mui/material';
import './globals.css'
import './tailwind.css'
import type { Metadata } from "next";
import LightDarkMode from '@/components/LightDarkMode';

export const metadata: Metadata = {
  title: 'SaveMail',
  description: 'Saving you emails, one at a time',
  publisher:'Stephane Latil',
  applicationName:'SaveMail',
  keywords:['mail', 'email', 'archive'],
  creator:'Stephane Latil'
}

export default function RootLayout({children}: {children: React.ReactNode}) {
  return (
    <html lang="en">
      <LightDarkMode>
      <CssBaseline />
        <body>
            {children}
        </body>
      </LightDarkMode>
    </html>
  )
}
