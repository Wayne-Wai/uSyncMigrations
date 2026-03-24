import { UMB_WORKSPACE_CONDITION_ALIAS } from "@umbraco-cms/backoffice/workspace";
import {
  SyncMigrationUpgradeConditionConfig,
  USYNC_MIGRATION_UPGRADE_CONDITION_ALIAS,
} from "../conditions/types.js";

const workspaceView: UmbExtensionManifest = {
  type: "workspaceView",
  alias: "usync.migrations.upgrade.view",
  name: "uSync Migrations Upgrade Workspace View",
  weight: 160,
  js: () => import("./upgrade-view.element.js"),
  meta: {
    label: "#usyncmigrations_tab",
    icon: "icon-paper-plane color-green",
    pathname: "upgrade",
  },
  conditions: [
    {
      alias: UMB_WORKSPACE_CONDITION_ALIAS,
      match: "usync.workspace",
    },
    {
      alias: USYNC_MIGRATION_UPGRADE_CONDITION_ALIAS,
      hasLegacyFiles: true,
    } as SyncMigrationUpgradeConditionConfig,
  ],
};

export const manifests = [workspaceView];
