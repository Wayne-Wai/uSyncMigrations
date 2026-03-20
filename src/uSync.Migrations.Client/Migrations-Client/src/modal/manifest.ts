const modal: UmbExtensionManifest = {
  type: "modal",
  alias: "usync-migration-complete-modal",
  name: "Migration Complete Modal",
  js: () => import("./completed-modal.element"),
};

export const manifests = [modal];
