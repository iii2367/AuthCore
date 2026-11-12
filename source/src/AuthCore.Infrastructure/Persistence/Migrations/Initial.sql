CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE permissions (
    id uuid NOT NULL,
    name character varying(128) NOT NULL,
    description character varying(512) NOT NULL,
    CONSTRAINT "PK_permissions" PRIMARY KEY (id)
);

CREATE TABLE roles (
    id uuid NOT NULL,
    name character varying(64) NOT NULL,
    description character varying(512) NOT NULL,
    CONSTRAINT "PK_roles" PRIMARY KEY (id)
);

CREATE TABLE users (
    id uuid NOT NULL,
    email character varying(256) NOT NULL,
    status character varying(32) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL,
    CONSTRAINT "PK_users" PRIMARY KEY (id)
);

CREATE TABLE role_permissions (
    role_id uuid NOT NULL,
    permission_id uuid NOT NULL,
    CONSTRAINT "PK_role_permissions" PRIMARY KEY (role_id, permission_id),
    CONSTRAINT "FK_role_permissions_permissions_permission_id" FOREIGN KEY (permission_id) REFERENCES permissions (id) ON DELETE CASCADE,
    CONSTRAINT "FK_role_permissions_roles_role_id" FOREIGN KEY (role_id) REFERENCES roles (id) ON DELETE CASCADE
);

CREATE TABLE credential (
    user_id uuid NOT NULL,
    password_hash character varying(512) NOT NULL,
    CONSTRAINT "PK_credential" PRIMARY KEY (user_id),
    CONSTRAINT "FK_credential_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE sessions (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    client_id uuid,
    user_agent character varying(512) NOT NULL,
    ip_address character varying(45) NOT NULL,
    status character varying(32) NOT NULL,
    created_at timestamptz NOT NULL,
    expires_at timestamptz NOT NULL,
    ended_at timestamptz,
    CONSTRAINT "PK_sessions" PRIMARY KEY (id),
    CONSTRAINT "FK_sessions_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE user_roles (
    user_id uuid NOT NULL,
    role_id uuid NOT NULL,
    CONSTRAINT "PK_user_roles" PRIMARY KEY (user_id, role_id),
    CONSTRAINT "FK_user_roles_roles_role_id" FOREIGN KEY (role_id) REFERENCES roles (id) ON DELETE CASCADE,
    CONSTRAINT "FK_user_roles_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE refresh_tokens (
    id uuid NOT NULL,
    token_hash character varying(512) NOT NULL,
    session_id uuid NOT NULL,
    created_at timestamptz NOT NULL,
    expires_at timestamptz NOT NULL,
    revoked_at timestamptz,
    replaced_by_token_id uuid,
    CONSTRAINT "PK_refresh_tokens" PRIMARY KEY (id),
    CONSTRAINT "FK_refresh_tokens_refresh_tokens_replaced_by_token_id" FOREIGN KEY (replaced_by_token_id) REFERENCES refresh_tokens (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_refresh_tokens_sessions_session_id" FOREIGN KEY (session_id) REFERENCES sessions (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_permissions_name" ON permissions (name);

CREATE INDEX "IX_refresh_tokens_replaced_by_token_id" ON refresh_tokens (replaced_by_token_id);

CREATE INDEX "IX_refresh_tokens_session_id" ON refresh_tokens (session_id);

CREATE UNIQUE INDEX "IX_refresh_tokens_token_hash" ON refresh_tokens (token_hash);

CREATE INDEX "IX_role_permissions_permission_id" ON role_permissions (permission_id);

CREATE UNIQUE INDEX "IX_roles_name" ON roles (name);

CREATE INDEX "IX_sessions_user_id_status" ON sessions (user_id, status);

CREATE INDEX "IX_user_roles_role_id" ON user_roles (role_id);

CREATE UNIQUE INDEX "IX_users_email" ON users (email);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261106071103_Initial', '10.0.14');

COMMIT;

