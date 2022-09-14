# Instructions for installing database for Hood CMS

## Fresh installation

1. Create your database 
2. Execute file `/sql/latest.sql`.

## Upgrading from previous versions < `v6.1.x`

1. Update your code to the latest version of Hood `v6.0.x`
2. Migrate your database to match the current code using ef core migrations.
3. Run the script `/sql/6.0/migrate.sql` to migrate your database to script based migrations.
4. Run the update scripts for each minor version, sequentially until you reach your desired version.
   For example to update to `v6.2.x`, run the script `/sql/6.1/update.sql`, then run the script `/sql/6.2/update.sql`.
