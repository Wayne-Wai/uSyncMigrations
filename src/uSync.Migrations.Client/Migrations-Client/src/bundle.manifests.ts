import { manifests as entrypoints } from "./entrypoints/manifest.js";
import { manifests as context } from "./context/manifest.js";
import { manifests as views } from "./views/manifest.js";
import { manifests as conditions } from "./conditions/manifest.js";
import { manifests as lang } from "./lang/manifest.js";
import { manifests as modals } from "./modal/manifest.js";

export * from "./components/index.js";

// Job of the bundle is to collate all the manifests from different parts of the extension and load other manifests
// We load this bundle from umbraco-package.json
export const manifests: Array<UmbExtensionManifest> = [
  ...context,
  ...entrypoints,
  ...views,
  ...conditions,
  ...lang,
  ...modals,
];
