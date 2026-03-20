import { UmbConditionConfigBase } from "@umbraco-cms/backoffice/extension-api";

export type SyncMigrationUpgradeConditionConfig = UmbConditionConfigBase & {
  hasLegacyFiles: boolean;
};

export const USYNC_MIGRATION_UPGRADE_CONDITION_ALIAS =
  "usync.migration.upgrade.condition";
