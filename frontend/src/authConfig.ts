import { PublicClientApplication, Configuration, LogLevel } from '@azure/msal-browser'

const tenantId = import.meta.env.VITE_AAD_TENANT_ID as string
const clientId = import.meta.env.VITE_AAD_CLIENT_ID as string
const backendScope = import.meta.env.VITE_AAD_BACKEND_SCOPE as string

const msalConfig: Configuration = {
  auth: {
    clientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      loggerCallback: (_level, message, containsPii) => {
        if (!containsPii) console.log(message)
      },
      logLevel: LogLevel.Warning,
    },
  },
}

export const msalInstance = new PublicClientApplication(msalConfig)

export const apiScopes = [backendScope]
