import { UmbModalToken } from "@umbraco-cms/backoffice/modal";

export type CompleteModalData = {};
export type CompleteModalResult = {};

export const USYNC_MIGRATION_COMPLETE_MODAL_TOKEN = new UmbModalToken<
  CompleteModalData,
  CompleteModalResult
>("usync-migration-complete-modal", {
  modal: {
    type: "dialog",
  },
});
