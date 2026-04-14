# uSync.Migrations - The v17 Experience

> [!NOTE]
> This is very much a work in progress. We are trying out some ideas to make migrations simpler in the v17 world.
> Feel free to nose about, but at the moment, there is ZERO support for any of this - you should use this only on local development sites, where you have backups of your data and don't mind starting again.

## Brave New Migrations

For v17 we thought we would have a go at making migrating a simpler experience - integrating it with the sync process
as much as we can, and making it happen without you really noticing.

## Config and Content Migration via Serializers

Firstly, uSync has some 'migration' points already built in. Whenever a datatype or a bit of content is imported,
uSync looks for ConfigSerializers and Value mappers to get the values that actually should be imported.

So for migrations we can use these points to update config and update content if we think we need to.
(uSync already does this for blockgrid/list without you even knowing, because the format changes slightly between v13, v15 & v17).

uSync Migrations adds some extras here. You can see them in the uSync.Migrations.Migrators project.

### Limitations

These migration points are cool because they are already there, but they are not as powerful as the migration points
in old school uSync.Migrations.

v13 migrations has a concept of a migration context, which contains all sorts of information about the migration, so any
bit of code anywhere along the migration can say 'what is this content type going to be called when we finish' or add
new datatypes, or split up property values or all sorts.

With the uSync 'migration' points you are separated from all that. You know about the datatype or content value you are
looking at, so this means we can't split or merge properties during install, or rename or add new datatypes or content types.

For 90% of migrations this is fine! For the other 10%, well we might have to do something else (for 5% of them anyway).

## Upgraders

This version of uSync.Migrations has the ability to add 'upgraders' to the 'update' process inside the migrate tab.
This effectively is something that sits in between the file copy of files from a legacy uSync folder to the new one.

At this point the upgrader gets the whole file (so the XML) for a datatype, doctype, content item, or anything - so it can
do a bit more.

### The Grid

The main point of putting this feature in is so we can manipulate grid elements before we do the upgrade.
The grid upgrader is quite basic just now, but with it - we can add new datatype files (for the grid.editors.config.js file)
and new content types (again for the config).

Doing this during the copy means when you then click import later on, hopefully the right datatypes and content types
are there waiting (the serializers and mappers will then be able to convert the grid content properties into blockgrids 🤞).

This gets us another 5% of migration cases.

## Other?

So what can't we do? Well, we still can't split/merge properties across content items, and we can't reach across
different items to do fancy things like rename/rationalize all the content types.

If this is you, then we would say use uSync.Migrations v13 edition, get your site all nice on v13 and then move it to v17.

It's still true that a 'modern' v13 site (e.g. no grid, or nested content, or other 'fancy' things) - will 'just' work if you
bring the uSync files to v17. The formats are all very close, and in fact v17 uSync is tested against these upgrades already.

uSync.Migrations for v17 is aiming to do the slightly more complicated things (like the grid) - while still being something
quite simple if you haven't done anything too clever...
