import {
  css,
  customElement,
  html,
  property,
} from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import {
  UmbModalContext,
  UmbModalExtensionElement,
} from "@umbraco-cms/backoffice/modal";
import { CompleteModalData, CompleteModalResult } from "./token";
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";

@customElement("usync-migration-complete-modal")
export class MigrationCompleteModalElement
  extends UmbLitElement
  implements UmbModalExtensionElement<CompleteModalData, CompleteModalResult>
{
  @property({ attribute: false })
  modalContext?: UmbModalContext<CompleteModalData, CompleteModalResult>;

  override render() {
    return html`
      <umb-body-layout .headline=${this.localize.term("usyncmigrations_migrationCompleteHeadline")}>
        <div>
          <umb-localize key="usyncmigrations_migrationCompleteDescription"></umb-localize>
        </div>
        </div>
        <div slot="actions">
          <uui-button
            look="primary"
            color="positive"
            label="close"
            @click=${this.#close}
            >Close</uui-button
          >
        </div>
      </umb-body-layout>
    `;
  }

  #close() {
    this.modalContext?.submit();
  }

  static override styles = [
    UmbTextStyles,
    css`
      umb-body-layout {
        max-width: 520px;
      }
    `,
  ];
}

export default MigrationCompleteModalElement;

declare global {
  interface HTMLElementTagNameMap {
    "usync-migration-complete-modal": MigrationCompleteModalElement;
  }
}
