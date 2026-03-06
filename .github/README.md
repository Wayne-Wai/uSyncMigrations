# uSync.Migrations. the v17 experience.

> [!NOTE]
> This is very much a work in progress, we are trying out some ideas to make migrations simpler in the v17 world.
> Feel free to nose about, but at the moment, there is ZERO support for any of this, and it will probibly break your site.

## Brave new migrations.

for v17 we throught we would have a go at making migrating a simpler experience - intergrating it with the sync process
as much as we can, and making it happen without you really noticing.

## Config and Content migration via serializers.

So firstly uSync has some 'migration' points already built in, when ever a datatype, or a bit of content is imported
uSync looks for ConfigSerializers and Value mappers, to get the values that actually should be imported.

So for migrations we can use these points to update config, and update config if we think we need to.
(uSync already does this for blockgrid/list without you even knowing, because the format changes slightly between v13,15 & v17).

uSync Migrations adds some extras here. you can see them in the uSync.Migrations.Migrators project.

### Limitations

These migration points are cool, because they are already there but they are not as powerful as the migration points
in old school uSync.Migrations.

v13 migrations has a concept of a migration context, which contains all sorts of information about the migration, so any
bit of code anywhere along the migration can say 'what is this content type going to be called when we finish' or add
new datatypes, or split up property values or all sorts.

with the uSync 'migration' points you are seperated from all that, you know about the datatype or content value you are
looking at. so this means we can't split or merge properties during install, or rename or add new datatypes or content types.

for 90% of migrations this is fine ! for the other 10% , well we might have to do somthing else (for 5% of them anyway.)

## Upgraders

this version of uSync.Migrations has the ability to add 'upgraders' to the 'update' process inside the migrate tab.
this effectively is something that sits in between the file copy of files from a legacy uSync folder to the new one.

at this point the upgrader gets the whole file (so the xml) for a datatype, doctype, content item, or anything. so it can
do a bit more.

### the grid

The main point of putting this feature in is so we can manipulate grid elements before we do the upgrade.
the grid upgrader is quite basic just now, but with it - we can add new datatype files (for the grid.editors.config.js file)
and new content types (again for the config).

doing this during the copy means, when you then click import later on, hopefully the right datatypes and content types
are there waiting (the serializers and mappers will then convert be able the grid content properties into blockgrids🤞).

this gets us another 5% of migration cases.

## Other?

So what can't we do, well we still can't split/merge properties across content items, and we can't reach across
different items to do fancy things like rename / rationalize all the content types.

if this is you, then we would say using uSync.Migrations v13 edition, get your site all nice on v13 and then move it to v17.

it's still true that a 'modern' v13 site (e.g no grid, or nested content, or other 'fancy' things) - will 'just' work if you
bring the uSync files to v17 - the formats are all very close, and infact v17 uSync is tested against these upgrades already
.

uSync.Migrations for v17 is aiming to do the slightly more complicated things (like the grid) - while still being something
quite simple if you haven't don't anything too clever...
