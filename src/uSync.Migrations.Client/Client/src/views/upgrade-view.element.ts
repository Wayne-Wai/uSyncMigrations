import {
  css,
  customElement,
  html,
  state,
} from "@umbraco-cms/backoffice/external/lit";
import { UUITextStyles } from "@umbraco-cms/backoffice/external/uui";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import SyncMigrationContext from "../context/migration-context";
import { USYNC_MIGRATIONS_CONTEXT } from "../context/types";
import { SyncUpgradeCheckResponse } from "../api";
import { umbConfirmModal } from "@umbraco-cms/backoffice/modal";

@customElement("usync-migrations-upgrade-view")
export class SyncUpgradeViewElement extends UmbLitElement {
  #upgradeContext?: SyncMigrationContext;

  @state()
  _legacy: SyncUpgradeCheckResponse | undefined;

  @state()
  upgradeButtonState: "waiting" | "failed" | "success" | undefined;

  constructor() {
    super();

    this.consumeContext(USYNC_MIGRATIONS_CONTEXT, (instance) => {
      if (!instance) return;
      this.#upgradeContext = instance;

      this.#upgradeContext.checkForLegacyAsync();

      this.observe(this.#upgradeContext.legacy, (legacy) => {
        if (!legacy) return;

        this._legacy = legacy;
      });
    });
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

  async #onUpgrade() {
    umbConfirmModal(this, {
      headline: this.localize.term("usyncmigrations_upgradeConfirmHeadline"),
      content: this.localize.term("usyncmigrations_upgradeConfirmMessage"),
      color: "warning",
      confirmLabel: this.localize.term("general_confirm"),
    })
      .then(async () => {
        await this.#upgradeContext?.upgrade();
        this.upgradeButtonState = "success";
      })
      .catch(() => {
        // Modal was closed without confirming
      });
  }

  override render() {
    return html`<umb-body-layout>
      <div class="layout">${this.#renderHeader()}
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
      <div class="actions">
        <uui-box
          headline=${this.localize.term("usyncmigrations_upgradeHeadline")}
        >
          <div class="action">
            <div class="description">
              <umb-localize
                key="usyncmigrations_upgradeDescription"
                .args=${[
                  this._legacy?.legacyFolderPath,
                  this._legacy?.lastestFolder,
                ]}
              ></umb-localize>
            </div>
            <uui-button
              label=${this.localize.term("usyncmigrations_upgradeButton")}
              look="primary"
              @click=${this.#onUpgrade}
              .state=${this.upgradeButtonState}
            ></uui-button>
          </div>
        </uui-box>
        <uui-box
          headline=${this.localize.term("usyncmigrations_ignoreHeadline")}
        >
          <div class="action">
            <div class="description">
              <umb-localize
                key="usyncmigrations_ignoreDescription"
                .args=${[this._legacy?.legacyFolderPath]}
              ></umb-localize>
            </div>
            <uui-button
              label=${this.localize.term("usyncmigrations_ignoreButton")}
              look="secondary"
              @click=${this.#onIgnore}
            ></uui-button>
          </div>
        </uui-box>
      </div>
    `;
  }

  static override styles = [
    UUITextStyles,
    css`
      .layout {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-5);
      }

      .actions {
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

      .action uui-button {
        width: 200px;
      }
    `,
  ];
}

export default SyncUpgradeViewElement;
