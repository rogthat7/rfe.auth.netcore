# API Reference: OAuth 2.1 & Google Federated Login

This service operates as an OAuth 2.1 and OpenID Connect compliant Authorization Server using **OpenIddict**. It is designed to be easily consumed by external clients and AI developer agents.

---

## 1. Google Federated Login Endpoint

To initiate user authentication via Google:

### `GET /api/auth/google/login`

Redirects the user to the Google sign-in portal.

#### Query Parameters
- `redirectUri` (string, optional, default: `/`): The local path/URI to return to after federated authentication callback processing is complete.

#### Example Request
```http
GET /api/auth/google/login?redirectUri=/connect/authorize%3Fclient_id%3Dmock-external-app%26response_type%3Dcode%26redirect_uri%3Dhttp%253A%252F%252Flocalhost%253A3001%252Fcallback%26scope%3Dopenid%2520profile%2520email%26code_challenge%3DE9Melhoa2OwvFrEMTJguCHaoeK1t8URWBuGJSstw-cM%26code_challenge_method%3DS256 HTTP/1.1
Host: localhost
```

---

## 2. OAuth 2.1 Authorization Endpoint

Used to request user authorization (consent) for an external application.

### `GET /connect/authorize` (or `POST`)

#### Query Parameters (OAuth 2.1 Standards)
- `client_id` (string, required): The registered client ID (e.g. `mock-external-app`).
- `response_type` (string, required): Must be `code`.
- `redirect_uri` (string, required): The pre-registered client redirect URI to send the authorization code back to.
- `scope` (string, required): Allowed scopes separated by spaces (e.g. `openid profile email offline_access`).
- `code_challenge` (string, required): PKCE challenge string (SHA-256 hash base64url encoded).
- `code_challenge_method` (string, required): Must be `S256`.

#### Responses

##### 1. Unauthenticated Redirect (302 Found)
If the user is not authenticated via cookie, they will be redirected to the login flow (e.g. challenging Google login) with the current query parameters preserved in the state.

##### 2. Consent Screen (200 OK)
Renders a HTML consent form asking the user to approve or deny the requested client scopes.

##### 3. Authorization Code Redirect (302 Found)
Once approved, redirects to the client's `redirect_uri` with:
- `code` (string): The authorization code.
- `state` (string, optional): The client state.

#### Example Request
```http
GET /connect/authorize?client_id=mock-external-app&response_type=code&redirect_uri=http%3A%2F%2Flocalhost%3A3001%2Fcallback&scope=openid%20profile%20email%20offline_access&code_challenge=E9Melhoa2OwvFrEMTJguCHaoeK1t8URWBuGJSstw-cM&code_challenge_method=S256 HTTP/1.1
Host: localhost
```

---

## 3. OAuth 2.1 Token Exchange Endpoint

Exchanges the authorization code or refresh token for a JWT Access Token.

### `POST /connect/token`

Consumes form urlencoded payloads.

#### Request Headers
- `Content-Type: application/x-www-form-urlencoded`

#### Form Parameters (Authorization Code Flow)
- `grant_type` (string, required): Must be `authorization_code`.
- `client_id` (string, required): The client ID (e.g., `mock-external-app`).
- `client_secret` (string, optional): Required if the client is confidential.
- `redirect_uri` (string, required): Must match the `redirect_uri` sent during authorization.
- `code` (string, required): The authorization code received from the `/connect/authorize` redirect.
- `code_verifier` (string, required): The original plain-text PKCE verifier before hashing.

#### Form Parameters (Refresh Token Flow)
- `grant_type` (string, required): Must be `refresh_token`.
- `client_id` (string, required): The client ID.
- `refresh_token` (string, required): The refresh token issued previously.

#### Response Payload (200 OK)
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIs...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "8ab87b92c4d9...",
  "id_token": "eyJhbGciOiJSUzI1NiIs..."
}
```
