# Vision: AuthCore

## Problem
Applications that implement their own authentication and access control
duplicate effort and introduce security risks. AuthCore centralizes user
identity, authentication, and role-based access control in one reusable service.

## Target audience
Developers who need a standalone service to manage users, roles and
permissions for their applications and services.

## Value proposition
For developers who need centralized identity and access control, AuthCore is
an authentication and authorization service that issues JWTs and manages
users, roles and permissions. Unlike building auth into every application,
AuthCore provides one consistent, independently verifiable source of access
decisions.

## Key features
- User registration and authentication, JWT-based
- RBAC: roles and permissions managed centrally
- OAuth 2.0 provider: registered client applications obtain tokens from AuthCore
- Audit trail of security-relevant actions

## Scope

### In scope (v1)
- User registration with email and password
- Authentication, issuing JWT access tokens and refresh tokens
- Token refresh, logout and revocation
- RBAC:
  - roles <-> permissions: many-to-many
  - users <-> roles: many-to-many
- Management of users, roles and permissions (create, update, assign, revoke)
- OAuth 2.0 provider: registration of client applications,
  supported grant types: <TBD>
- Admin interface for managing clients, roles and users: panel
- Exposing public keys (JWKS) so other services validate JWTs independently
- Audit log: who, what, when, from where (IP)

### Data stored
- Users: identity (email), account status, timestamps
- Credentials: password hashes (kept separate from user identity)
- Roles and permissions, with their assignments:
  user <-> role, role <-> permission (many-to-many)
- Sessions and refresh tokens (stored as hashes)
- OAuth clients: registered applications with secret hashes,
  allowed grant types and scopes
- Audit log: who, what, when, from where (IP)

### Data deliberately not stored
- Access tokens (stateless JWTs)
- Plaintext passwords or client secrets
- Personal data beyond email

### Out of scope (v1)
- User profile data (name, avatar, settings)
- Business logic of consumer services
- Login through third-party providers (Google, GitHub)
- Multi-factor authentication
- Attribute-based or resource-level access control
- End-user UI


## Constraints and assumptions
- Consumer services validate JWTs locally and enforce permissions themselves;
  AuthCore only defines and issues them
- No personal data beyond email is stored

## Success metrics
- A new service can integrate by validating JWTs without calling AuthCore
  per request
- Token issuance p95 < 200 ms
- Every security-relevant action appears in the audit log
