import {
  customElement,
  html,
  property,
} from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { SyncUpgradeMessage } from "../api";

@customElement("usync-migrations-results")
export class SyncUpgradeAnalysisResultsElement extends UmbLitElement {
  @property({ type: Array })
  results: SyncUpgradeMessage[] = [];

  protected override render() {
    var results = this.results.map(
      (result) =>
        html`<usync-migrations-result
          .result=${result}
        ></usync-migrations-result>`,
    );

    return html`${results}`;
  }
}

export default SyncUpgradeAnalysisResultsElement;
