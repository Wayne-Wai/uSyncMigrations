import { USYNC_MIGRATION_UPGRADE_CONDITION_ALIAS } from "./types";
import { SyncMigrationUpgradeCondition } from "./upgrade-condition";

export const upgradeCondition: UmbExtensionManifest = {
  type: "condition",
  alias: USYNC_MIGRATION_UPGRADE_CONDITION_ALIAS,
  name: "uSync Migrations Upgrade Condition",
  api: SyncMigrationUpgradeCondition,
};

export const manifests = [upgradeCondition];
