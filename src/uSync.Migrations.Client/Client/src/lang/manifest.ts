const localizations: Array<UmbExtensionManifest> = [
  {
    type: "localization",
    alias: "usync.migrations.localization.en",
    name: "uSync Migrations English Localization",
    weight: 0,
    meta: {
      culture: "en",
    },
    js: () => import("./files/en"),
  },
];

export const manifests: Array<UmbExtensionManifest> = [...localizations];
