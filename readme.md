# Hood CMS
[![GitHub release (Latest by date including pre-releases)](https://img.shields.io/github/v/release/HoodDigital/Hood?include_prereleases&label=Latest%20Release)](https://github.com/HoodDigital/Hood/releases)

A fully customisable content management system built in ASP.NET Core 5 & Bootstrap 5.

## Clone demo project

Clone the demo ASP.NET Core 6 Web Project from our repository [here](https://github.com/HoodDigital/Hood.Demo). Or enter the following command in Git Bash or your command prompt.
```
$ git clone https://github.com/HoodDigital/Hood.Demo
```

## Create new ASP.NET Core 6 Web Project via dotnet CLI

Coming soon.

## Nuget Installation 
[![NuGet](https://img.shields.io/nuget/v/hood?label=NuGet%20Stable)](https://www.nuget.org/packages/Hood/)
[![MyGet Latest](https://img.shields.io/myget/hood/vpre/hood?label=MyGet)](https://www.myget.org/feed/hood/package/nuget/Hood)

Install Hood CMS via Package Manager.
```
> Install-Package Hood
```
or via .NET CLI
```
> dotnet add package Hood
```

## Client Side Code
[![npm Package](https://img.shields.io/npm/v/hoodcms)](https://www.npmjs.com/package/hoodcms)

The client side code is not required to run Hood CMS as all required JS/CSS are served via jsdelivr. However, if you want to extend or modify the client side code, you can download this npm package, which contains the required distribution CSS and JavaScript, as well as source SCSS and TypeScript files. 

https://www.npmjs.com/package/hoodcms

To install Hood CMS client side code via NPM.
```
> npm install hoodcms
```
or
```
> yarn add hoodcms
```

> To use your own client side code, you will also need to update script/link references in your theme's HTML or Razor C# files to use your own version of the code, rather than the CDN.

## Database Installation/Update

Ensure your database is up to date with the version of Hood CMS that you are using. 
### Fresh installation

1. Create your database 
2. Execute file `/sql/latest.sql`.

### Upgrading from previous versions < `v6.1.x`

1. Update your code to the latest version of Hood `v6.0.x`
2. Migrate your database to match the current code using ef core migrations.
3. Run the script `/sql/6.0/migrate.sql` to migrate your database to script based migrations.
4. Run the update scripts for each minor version, sequentially until you reach your desired version.
   For example to update to `v6.2.x`, run the script `/sql/6.1/update.sql`, then run the script `/sql/6.2/update.sql`.


## Full documentation
Documentation is a work in progress!

Also, feel free to add your issues or pull requests to our GitHub, we always welcome contributions!
