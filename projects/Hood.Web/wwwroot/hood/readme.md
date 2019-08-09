# Hood CMS

[![npm Package](https://img.shields.io/npm/v/hoodcms)](https://www.npmjs.com/package/hoodcms)
[![Visual Studio Version](https://img.shields.io/badge/Visual%20Studio-2019-magenta.svg?colorB=770ca3)](https://www.visualstudio.com/)
[![Demo](https://img.shields.io/badge/dynamic/json.svg?label=Demo&url=http%3A%2F%2Fcms.hooddigital.com%2Fhood%2Fversion&query=%24.version&colorB=%23eab92d&prefix=v)](http://cms.hooddigital.com/)

You can download and use the full ASP.NET Core Source Code form Hood CMS here: [Hood CMS](https://github.com/HoodDigital/Hood)

## About Hood CMS
Hood CMS Hood Digital's own content management system, built entirely in ASP.NET Core 2.2. With a fully functional CMS admin area and themable front site, including subscriptions powered by Stripe. 
Access and security is all based on the latest patterns for ASP.NET Core & EF Core Identity provider.

## Installation in Visual Studio 2019 Template Project
This template can be installed via NuGet and npm, or you can download the [latest release](https://github.com/HoodDigital/Hood/releases).

### Recommended Installation 
To install Hood JS & LESS via LibMan, add the following lines to your libman.json file:
```
{
    "destination": "wwwroot/hood",
    "library": "hoodcms@3.0.0-pre4",
    "provider": "jsdelivr"
}
```

### Legacy Themes Bower Installation 
To install Hood JS & LESS for legacy themes, run the following command in the command line (bower required):
```
bower install --save hood
```

### Less/JavaScript
This package only contains the required SCSS/JavaScript and CSS files required to run Hood CMS with the current default theme. 

##Hood CMS ASP.NET Core Source Code & Pacakge
[![Bower](https://img.shields.io/github/tag/HoodDigital/Hood?label=GitHub&colorB=000000)](https://bower.io/search/?q=hood)