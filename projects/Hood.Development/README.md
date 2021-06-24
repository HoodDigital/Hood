# Hood CMS Client Side Code
[![Build Status](https://dev.azure.com/hooddigital/Hood/_apis/build/status/HoodDigital.Hood)](https://dev.azure.com/hooddigital/Hood/_build/latest?definitionId=4)
[![GitHub release (Latest by date including pre-releases)](https://img.shields.io/github/v/release/HoodDigital/Hood?include_prereleases&label=Latest%20Release)](https://github.com/HoodDigital/Hood/releases)

A fully customisable content management system built in ASP.NET Core 5 & Bootstrap 5.

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

To install Hood CMS client side code via Yarn/NPM.
```
> yarn install hoodcms --save-dev
or
> npm install hoodcms --save-dev
```

> To use your own client side code, you will also need to update script/link references in HTML or Razor files in your views or themes folder to use your own version of the code, rather than the CDN.

## Full documentation
Documentation is a work in progress!

Also, feel free to add your issues or pull requests to our GitHub, we always welcome contributions!
