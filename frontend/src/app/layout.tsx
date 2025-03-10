import './globals.css'
import './tailwind.css'
import type { Metadata } from "next";
import LightDarkMode from '@/components/LightDarkMode';
import NotificationSnackbar from '@/components/NotificationSnackbarProvider';
import { AppRouterCacheProvider } from '@mui/material-nextjs/v14-appRouter';
import { PublicEnvScript } from 'next-runtime-env';

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
      <head>
        <link rel="apple-touch-icon" sizes="180x180" href="/apple-touch-icon.png"/>
        <link rel="icon" type="image/png" sizes="32x32" href="/favicon-32x32.png"/>
        <link rel="icon" type="image/png" sizes="16x16" href="/favicon-16x16.png"/>
        <link rel="manifest" href="/site.webmanifest" />
        <PublicEnvScript />
      </head>
      <meta name="format-detection" content="telephone=no, date=no, email=no, address=no" />
        <body>
          <AppRouterCacheProvider>
            <LightDarkMode>
              <NotificationSnackbar>
                  {children}
              </NotificationSnackbar>
            </LightDarkMode>
          </AppRouterCacheProvider>
        </body>
    </html>
  )
}
