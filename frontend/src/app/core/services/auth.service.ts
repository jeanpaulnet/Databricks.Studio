import { Injectable } from '@angular/core';
import { OAuthService, AuthConfig } from 'angular-oauth2-oidc';
import { BehaviorSubject, map } from 'rxjs';
import { environment } from '../../../environments/environment';

const googleAuthConfig: AuthConfig = {
  issuer: environment.google.issuer,
  redirectUri: environment.google.redirectUri,
  clientId: environment.google.clientId,
  scope: environment.google.scope,
  responseType: 'code',
  strictDiscoveryDocumentValidation: false,
  showDebugInformation: !environment.production
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private loggedIn$ = new BehaviorSubject<boolean>(false);

  readonly isLoggedIn$ = this.loggedIn$.asObservable();
  readonly userName$ = this.isLoggedIn$.pipe(
    map(() => {
      const claims = this.oauthService.getIdentityClaims() as Record<string, string> | null;
      return claims ? (claims['name'] ?? claims['email'] ?? '') : '';
    })
  );

  constructor(private oauthService: OAuthService) {}

  initialize(): void {
    this.oauthService.configure(googleAuthConfig);
    this.oauthService.loadDiscoveryDocumentAndTryLogin().then(() => {
      this.loggedIn$.next(this.oauthService.hasValidIdToken());
    });
  }

  login(): void {
    this.oauthService.initCodeFlow();
  }

  logout(): void {
    this.oauthService.logOut();
    this.loggedIn$.next(false);
  }

  getAccessToken(): string {
    return this.oauthService.getIdToken(); // Google uses id_token for API auth
  }

  get isLoggedIn(): boolean {
    return this.oauthService.hasValidIdToken();
  }
}
