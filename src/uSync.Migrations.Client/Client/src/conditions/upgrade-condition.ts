import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbConditionControllerArguments } from "@umbraco-cms/backoffice/extension-api";
import { UmbConditionBase } from "@umbraco-cms/backoffice/extension-registry";
import { SyncMigrationUpgradeConditionConfig } from "./types";
import { MigrationsService } from "../api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";

export class SyncMigrationUpgradeCondition extends UmbConditionBase<SyncMigrationUpgradeConditionConfig> {
  config: SyncMigrationUpgradeConditionConfig;

  constructor(
    host: UmbControllerHost,
    args: UmbConditionControllerArguments<SyncMigrationUpgradeConditionConfig>,
  ) {
    super(host, args);
    this.config = args.config;

    this.checkForLegacyAsync().then((response) => {
      this.permitted = response?.hasLegacyFolder ?? false;
    });
  }

  async checkForLegacyAsync() {
    var result = (await tryExecute(this, MigrationsService.check())).data;
    return result;
  }
}
