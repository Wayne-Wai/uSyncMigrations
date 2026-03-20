export default {
  usyncmigrations: {
    tab: "Migrate",
    title: "uSync.Migrations",
    description: "Migrate your uSync files to the latest version.",

    analyseButton: "Analyse",
    importButton: "Import",

    upgradeHeadline: "Upgrade",
    upgradeDescription:
      "Upgrade this folder to the new format, this will move all files from the old {0} folder to the {1} folder and update the file formats to be compatible with the latest version of uSync.",
    upgradeButton: "Upgrade",
    ignoreHeadline: "Ignore",
    ignoreDescription: "Ignore the {0} folder and continue.",
    ignoreButton: "Ignore",
    ignoreConfirmHeadline: "Ignore legacy folder?",
    ignoreConfirmMessage:
      "Are you sure you want to ignore the legacy folder? You can re-enable it later if needed.",

    upgradeConfirmHeadline: "Upgrade legacy folder?",
    upgradeConfirmMessage:
      "This will move all files from the old folder to the latest version folder and update the file formats to be compatible with the latest version of uSync.",

    migrationCompleteHeadline: "Migration Complete",
    migrationCompleteDescription: `<p>The migration process has completed successfully. You can now close this dialog and continue using the application.</p>
      <p>It's recommended to review the migrated items to ensure everything has been transferred correctly.</p>
      <p>The uSync files on disk have been imported to your site, but these files do not respresent the latest stage of your site,
       data in them is being migrated as part of the import</p>
       <p>Once you are happy with the migrated data, you should run a clean export to get uSync files that represent the current state of your site.</p>
      `,
  },
};
