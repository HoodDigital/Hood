# Hood

[![AppVeyor Build Status](https://ci.appveyor.com/api/projects/status/7j755tgusxqrw6nl/branch/master?svg=true)](https://ci.appveyor.com/project/hooddigital/hood/branch/master)
[![Visual Studio Version](https://img.shields.io/badge/Visual%20Studio-2019-magenta.svg?colorB=770ca3)](https://www.visualstudio.com/)
[![Demo](https://img.shields.io/badge/dynamic/json.svg?label=Demo&url=http%3A%2F%2Fcms.hooddigital.com%2Fhood%2Fversion&query=%24.version&colorB=%23eab92d&prefix=v)](http://cms.hooddigital.com/)

## About Hood CMS

Hood CMS Hood Digital's own content management system, built entirely in ASP.NET Core 2.2. With a fully functional CMS admin area and themable front site, including subscriptions powered by Stripe. 
Access and security is all based on the latest patterns for ASP.NET Core & EF Core Identity provider.

## Installation

This package can be installed via NuGet and Bower, or you can download the [latest release](https://github.com/HoodDigital/Hood/releases).

### NuGet package
[![NuGet](https://img.shields.io/nuget/v/Hood.svg?label=NuGet)](https://www.nuget.org/packages/Hood/)
[![MyGet](https://img.shields.io/myget/hood/v/hood.svg?label=MyGet&colorB=008000)](https://www.myget.org/feed/hood/package/nuget/Hood)
[![MyGet Latest](https://img.shields.io/myget/hood/vpre/hood.svg?label=MyGet%20Latest&colorB=97ca00)](https://www.myget.org/feed/hood/package/nuget/Hood)

To install Hood .NET from NuGet, run the following command in the Package Manager Console:
```
PM> Install-Package Hood
```
To use the latest version, ensure you set your feed source to MyGet: https://www.myget.org/feed/hood/package/nuget/Hood

## Client Side Code

[![npm Package](https://img.shields.io/npm/v/hoodcms)](https://www.npmjs.com/package/hoodcms)
[![Bower](https://img.shields.io/myget/hood/vpre/hood.svg?label=Bower&colorB=ffcc2f)](https://bower.io/search/?q=hood)

You can download and use the client side code, containing all JavaScript, SCSS, Less and compiled CSS, including the default theme for Hood CMS here: 
https://www.npmjs.com/package/hoodcms

### Recommended Installation 
To install Hood JS & LESS via LibMan, add the following lines to your libman.json file:
```
{
    "destination": "wwwroot/hood",
    "library": "hoodcms@3.0.8",
    "provider": "jsdelivr"
}
```

### Legacy Themes Bower Installation 
To install Hood JS & LESS for legacy themes, run the following command in the command line (bower required):
```
bower install --save hood
```
### Note
This package only contains the required SCSS/JavaScript and CSS files required to run Hood CMS with the current default theme. 


## Full documentation

Documentation is a work in progress!

You can check our growing documentation for HoodCMS here where you can also raise support requests: 
https://hooddigital.atlassian.net/servicedesk/customer/portals

Also, feel free to add your issues or pull requests to this repo, we always welcome contributions!