import {
  css,
  customElement,
  html,
  property,
} from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { SyncUpgradeMessage } from "../api";

@customElement("usync-migrations-result")
export class SyncUpgradeAnalysisResultElement extends UmbLitElement {
  @property({ type: Object })
  result?: SyncUpgradeMessage;

  protected override render() {
    if (!this.result) return null;

    return html`<div class="result ${this.result.status.toLocaleLowerCase()}">
      ${this.#renderIcon()}
      <strong>${this.result.fileName}</strong>
      <div>${this.result.message}</div>
    </div>`;
  }

  #renderIcon() {
    switch (this.result?.status) {
      case "Error":
        return html`<umb-icon name="icon-alert color-red"></umb-icon>`;
      case "Warning":
        return html`<umb-icon name="icon-alert color-orange"></umb-icon>`;
      case "Info":
        return html`<umb-icon name="icon-check color-green"></umb-icon>`;
      case "Success":
        return html`<umb-icon name="icon-check color-green"></umb-icon>`;
      default:
        return null;
    }
  }

  static override styles = [
    css`
      .result {
        display: grid;
        grid-template-columns: 30px 3fr 5fr;
        align-items: center;
        gap: var(--uui-size-3);
        padding: var(--uui-size-3);
        border-bottom: 1px solid var(--uui-color-border);
      }

      .result > * {
        word-break: break-all;
      }

      .result.error {
        border-bottom-color: var(--uui-color-danger-emphasis);
      }

      .result.warning {
        border-bottom-color: var(--uui-color-warning-emphasis);
      }
    }`,
  ];
}

export default SyncUpgradeAnalysisResultElement;
