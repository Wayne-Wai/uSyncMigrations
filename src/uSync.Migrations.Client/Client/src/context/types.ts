import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { SyncMigrationContext } from "./migration-context";

export const USYNC_MIGRATIONS_CONTEXT_ALIAS = "usync.migrations.context";

export const USYNC_MIGRATIONS_CONTEXT =
  new UmbContextToken<SyncMigrationContext>(USYNC_MIGRATIONS_CONTEXT_ALIAS);
