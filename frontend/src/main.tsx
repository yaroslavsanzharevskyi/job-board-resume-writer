import React from 'react'
import ReactDOM from 'react-dom/client'
import { MsalProvider } from '@azure/msal-react'
import App from './App.tsx'
import { msalInstance } from './authConfig.ts'
import './index.css'

msalInstance.initialize().then(() => {
  ReactDOM.createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
      <MsalProvider instance={msalInstance}>
        <App />
      </MsalProvider>
    </React.StrictMode>,
  )
})
