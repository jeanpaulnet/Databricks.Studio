export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:51765',
  google: {
    clientId: 'YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com',
    redirectUri: 'http://localhost:4200',
    scope: 'openid profile email',
    issuer: 'https://accounts.google.com'
  }
};
