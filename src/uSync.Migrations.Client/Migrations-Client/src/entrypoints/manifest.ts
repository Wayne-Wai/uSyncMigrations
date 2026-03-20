export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "uSync Migrations Client Entrypoint",
    alias: "uSync.Migrations.Client.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint.js"),
  },
];
