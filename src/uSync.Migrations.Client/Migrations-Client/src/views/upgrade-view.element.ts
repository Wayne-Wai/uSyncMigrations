import {
  css,
  customElement,
  html,
  nothing,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { UUITextStyles } from "@umbraco-cms/backoffice/external/uui";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import SyncMigrationContext from "../context/migration-context";
import { USYNC_MIGRATIONS_CONTEXT } from "../context/types";
import { SyncUpgradeCheckResponse, SyncUpgradeMessage } from "../api";
import { umbConfirmModal, umbOpenModal } from "@umbraco-cms/backoffice/modal";
import { SyncCompleteMessage, USYNC_SIGNALR_CONTEXT_TOKEN } from "@jumoo/usync";
import { USYNC_MIGRATION_COMPLETE_MODAL_TOKEN } from "../modal/token";

@customElement("usync-migrations-upgrade-view")
export class SyncUpgradeViewElement extends UmbLitElement {
  #upgradeContext?: SyncMigrationContext;

  @state()
  legacy: SyncUpgradeCheckResponse | undefined;
  @state()
  stage: "analyse" | "upgrade" | "import" | "importing" | "complete" =
    "analyse";

  @state()
  working: boolean = false;

  @state()
  results: SyncUpgradeMessage[] = [];

  @state()
  clientId?: string | null;

  @state()
  complete?: SyncCompleteMessage;

  @state()
  importCount: number = 0;

  @state()
  importComplete: boolean = false;

  constructor() {
    super();

    this.consumeContext(USYNC_SIGNALR_CONTEXT_TOKEN, (instance) => {
      if (!instance) return;

      this.observe(instance.connected, (connected) => {
        if (connected) {
          this.clientId = instance.getClientId();
        }
      });

      this.observe(instance.complete, async (completed) => {
        if (!completed) return;
        this.importCount++;

        if (!this.importComplete) {
          this.importComplete = this.importCount >= 2;
          if (this.importComplete) {
            // popup the complete dialog
            await this.#openCompleteModal();
          }
        }
        this.complete = completed;
      });
    });

    this.consumeContext(USYNC_MIGRATIONS_CONTEXT, (instance) => {
      if (!instance) return;
      this.#upgradeContext = instance;

      this.#upgradeContext.checkForLegacyAsync();

      this.observe(this.#upgradeContext.legacy, (legacy) => {
        if (!legacy) return;

        this.legacy = legacy;
      });
    });
  }

  async #openCompleteModal() {
    // Open the complete modal
    var result = await umbOpenModal(
      this,
      USYNC_MIGRATION_COMPLETE_MODAL_TOKEN,
      {},
    );
    console.log("Complete modal closed with result:", result);
  }

  #onIgnore() {
    umbConfirmModal(this, {
      headline: this.localize.term("usyncmigrations_ignoreConfirmHeadline"),
      content: this.localize.term("usyncmigrations_ignoreConfirmMessage"),
      color: "warning",
      confirmLabel: this.localize.term("general_confirm"),
    })
      .then(async () => {
        await this.#upgradeContext?.ignoreLegacy();
        window.location.reload();
      })
      .catch(() => {
        // Modal was closed without confirming
      });
  }

  async #onAnalyse() {
    //  trigger the analysis, the context will update and the UI will react to that

    this.working = true;

    const results = await this.#upgradeContext?.analyse();
    this.results = results ?? [];
    this.stage = "upgrade";

    this.working = false;
  }

  async #onImport() {
    //  trigger the import, the context will update and the UI will react to that
    console.log("Importing with clientId", this.clientId);

    this.working = true;
    await this.#upgradeContext?.import(this.clientId);
    this.stage = "importing";
    this.working = false;
  }

  async #onUpgrade() {
    umbConfirmModal(this, {
      headline: this.localize.term("usyncmigrations_upgradeConfirmHeadline"),
      content: this.localize.term("usyncmigrations_upgradeConfirmMessage"),
      color: "warning",
      confirmLabel: this.localize.term("general_confirm"),
    })
      .then(async () => {
        this.#doUpgrade();
      })
      .catch(() => {
        // Modal was closed without confirming
      });
  }

  async #doUpgrade() {
    this.working = true;
    this.results = (await this.#upgradeContext?.upgrade()) ?? [];
    this.stage = "import";
    this.working = false;
  }

  #reset() {
    this.stage = "analyse";
    this.importCount = 0;
    this.importComplete = false;
    this.complete = undefined;
    this.results = [];
  }

  override render() {
    return html`<umb-body-layout>
      <div class="layout">
      <div class="two-boxes">
        ${this.#renderHeader()}
        ${this.#renderIgnoreBox()}
      </div>
        ${this.#renderActions()}</div>
      </div>
    </umb-body-layout>`;
  }

  #renderHeader() {
    return html`<uui-box
      headline=${this.localize.term("usyncmigrations_title")}
    >
      <p>${this.localize.term("usyncmigrations_description")}</p>
    </uui-box>`;
  }

  #renderActions() {
    return html`
      <uui-box
        headline=${this.localize.term("usyncmigrations_upgradeHeadline")}
      >
        <div slot="header-actions">
          ${when(
            this.stage !== "analyse",
            () =>
              html`<uui-button
                compact
                label="Reset"
                look="secondary"
                @click=${this.#reset}
                ><umb-icon name="icon-refresh"></umb-icon
              ></uui-button>`,
          )}
        </div>
        <div class="action">
          <div class="description">
            <umb-localize
              key="usyncmigrations_upgradeDescription"
              .args=${[
                this.legacy?.legacyFolderPath,
                this.legacy?.lastestFolder,
              ]}
            ></umb-localize>
          </div>
          <div class="action-buttons">
            ${this.#renderAnalyse()} ${this.#renderUpgrade()}
            ${this.#renderImport()}
          </div>
        </div>
      </uui-box>

      <div class="results">
        ${this.#renderResults()} ${this.#renderImportResults()}
      </div>
    `;
  }

  #renderIgnoreBox() {
    return html` <uui-box
      headline=${this.localize.term("usyncmigrations_ignoreHeadline")}
    >
      <div class="action">
        <div class="description">
          <umb-localize
            key="usyncmigrations_ignoreDescription"
            .args=${[this.legacy?.legacyFolderPath]}
          ></umb-localize>
        </div>
        <uui-button
          label=${this.localize.term("usyncmigrations_ignoreButton")}
          look="secondary"
          @click=${this.#onIgnore}
        ></uui-button>
      </div>
    </uui-box>`;
  }

  #renderUpgrade() {
    return html`<uui-button
      label=${this.localize.term("usyncmigrations_upgradeButton")}
      look="primary"
      color="warning"
      @click=${this.#onUpgrade}
      .state=${this.working ? "waiting" : undefined}
      .disabled=${this.stage !== "upgrade"}
    ></uui-button>`;
  }

  #renderAnalyse() {
    return html`<uui-button
      label=${this.localize.term("usyncmigrations_analyseButton")}
      look="primary"
      color="positive"
      .disabled=${this.stage !== "analyse"}
      @click=${this.#onAnalyse}
      .state=${this.working ? "waiting" : undefined}
    ></uui-button>`;
  }

  #renderImport() {
    return html`<uui-button
      label=${this.localize.term("usyncmigrations_importButton")}
      look="secondary"
      color="default"
      .disabled=${this.stage !== "import"}
      .state=${this.working ? "waiting" : undefined}
      @click=${this.#onImport}
    ></uui-button>`;
  }

  #renderResults() {
    if (this.stage === "importing") {
      return this.#renderProgress();
    }
    if (this.working) {
      return html`<uui-loader center></uui-loader>`;
    }
    if (this.results.length === 0) return null;
    return html`<uui-box headline="Results"
      ><usync-migrations-results
        .results=${this.results}
      ></usync-migrations-results
    ></uui-box>`;
  }

  #renderProgress() {
    if (this.importComplete) return nothing;
    return html`<uui-box headline="Progress">
      <usync-progress-box
        title="Migration Import ${this.importCount + 1}/2"
      ></usync-progress-box>
    </uui-box>`;
  }

  #renderImportResults() {
    if (!this.importComplete) return nothing;
    return html`<uui-box headline="Import Results">
      <usync-results .results=${this.complete?.actions}></usync-results
    ></uui-box>`;
  }

  static override styles = [
    UUITextStyles,
    css`
      .layout {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-5);
      }

      .two-boxes {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
        gap: var(--uui-size-4);
      }

      .action {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--uui-size-2);
      }

      .action-buttons {
        display: flex;
        gap: var(--uui-size-2);
        padding: var(--uui-size-2) 0;
      }

      .action uui-button {
        width: 200px;
      }
    `,
  ];
}

export default SyncUpgradeViewElement;
