# vies-dotnet-api

[![Build & Test](https://github.com/zapadi/vies-dotnet/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/zapadi/vies-dotnet/actions/workflows/build.yml)
[![NuGet package](https://img.shields.io/nuget/v/vies-dotnet-api.svg)](https://www.nuget.org/packages/vies-dotnet-api)
![Nuget](https://img.shields.io/nuget/dt/vies-dotnet-api)

---

European(EU)  VIES API VAT validation for dotnet based on the most current information from the official source

The objective of this API is to allow persons involved in the intra-Community supply of goods or of services to obtain confirmation of the validity of the VAT identification number of any specified person.


# Installing

The fastest way of getting started using Vies api is to install the NuGet package.

**Package Manager:**
```
Install-Package vies-dotnet-api -Version 2.1.1
```
**.NET CLI:**
```
dotnet add package vies-dotnet-api --version 2.1.1
```
**Package Reference**
```
<PackageReference Include="vies-dotnet-api" Version="2.1.1" />
```
# Usage

### Checking a VAT number using the vies-dotnet-api

Checking if a EU or GB VAT number is **valid**
```
 var result = ViesManager.IsValid("RO123456789");
```
 or

``` 
 var result = ViesManager.IsValid("RO","123456789");
```

Checking if a EU VAT number is **active**

```
var viesManager = new ViesManager();

var result = await viesManager.IsActiveAsync("RO123456789");
```
or

```
var result = await viesManager.IsActiveAsync("RO","123456789");
```

### Clarification

Since January 1, 2021 the UK is no longer a member of the European Union and as a result, the VIES service provided by the European Commission no longer validates VAT ID's for the UK.

<a href="https://www.buymeacoffee.com/vXCNnz9" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/lato-yellow.png" alt="Buy Me A Coffee" height="37" ></a>

## License
[![GitHub license](https://img.shields.io/github/license/zapadi/vies-dotnet?color=blue)](https://github.com/zapadi/vies-dotnet/blob/master/LICENSE)

The API is released under Apache 2 open-source license. You can use it for both personal and commercial purposes, build upon it and modify it.

## Thanks

* [JetBrains](http://www.jetbrains.com/) for my Open Source [![Resharper](https://github.com/zapadi/vies-dotnet/blob/master/logo-resharper.gif)](http://www.jetbrains.com/resharper/) licence

* AppVeyor for allowing free build CI services for Open Source projects
