const contextManifest: UmbExtensionManifest = {
  type: "globalContext",
  alias: "usync.migrations.context",
  name: "uSync Migrations Context",
  api: () => import("./migration-context.js"),
};

export const manifests = [contextManifest];
