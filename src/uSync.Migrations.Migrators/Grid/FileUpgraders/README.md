## Grid FileUpgraders.

The file upgraders run, when you click upgrade in the 'migrate' tab inside uSync. 
they create new uSync config files, for datatypes and content types that will be 
needed to actually migrate the content. 

if the upgraders haven't ran, then the datatype and content type config files won't be there
and the migration of the properties and types when you import with uSync will fail.

most things don't need the upgraders, its mainly the grid, because we have to create the 
types for all the bits that were just the grid previously. 