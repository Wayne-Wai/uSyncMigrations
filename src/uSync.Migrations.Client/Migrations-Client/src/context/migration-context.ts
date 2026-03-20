import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { USYNC_MIGRATIONS_CONTEXT } from "./types";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { MigrationsService, SyncUpgradeCheckResponse } from "../api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";

export class SyncMigrationContext extends UmbContextBase {
  public readonly workspaceAlias = "usync.workspace";

  getEntityType(): string {
    return "usync-root";
  }

  constructor(host: UmbControllerHost) {
    super(host, USYNC_MIGRATIONS_CONTEXT);
  }

  #legacy = new UmbObjectState<SyncUpgradeCheckResponse | undefined>(undefined);
  public readonly legacy = this.#legacy.asObservable();

  async checkForLegacyAsync() {
    const response = (await tryExecute(this, MigrationsService.check())).data;
    this.#legacy.setValue(response);
  }

  async ignoreLegacy() {
    await tryExecute(this, MigrationsService.ignore());
    this.checkForLegacyAsync();
  }

  async upgrade() {
    return (await tryExecute(this, MigrationsService.upgrade())).data ?? [];
  }

  async analyse() {
    return (await tryExecute(this, MigrationsService.analyze())).data ?? [];
  }

  async import(clientId?: string | null) {
    return (
      (
        await tryExecute(
          this,
          MigrationsService.import({
            query: {
              clientId: clientId ?? undefined,
            },
          }),
        )
      ).data ?? []
    );
  }
}

export default SyncMigrationContext;
