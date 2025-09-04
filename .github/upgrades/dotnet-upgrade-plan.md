# .NET 8.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET net8.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET net8.0 upgrade.
3. Upgrade C:\tmp\dotnet-upgrade-sample\eShopLegacy.Utilities\eShopLegacy.Utilities.csproj
4. Upgrade C:\tmp\dotnet-upgrade-sample\eShopPorted\eShopPorted.csproj
5. Upgrade src\eShopLegacyMVC\eShopLegacyMVC.csproj
6. Run unit tests to validate upgrade in the projects listed below:


## Settings

### Excluded projects

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|


### Aggregate NuGet packages modifications across all projects

| Package Name                        | Current Version | New Version | Description                         |
|:------------------------------------|:---------------:|:-----------:|:------------------------------------|
| Antlr                               |   3.5.0.2       |  4.6.6      | Recommended upgrade for compatibility|
| EntityFramework                     |   6.2.0         |  6.5.1      | Deprecated; recommended replacement  |
| Microsoft.EntityFrameworkCore       |   2.2.6         |  8.0.19     | Upgrade to EF Core 8 for net8.0     |
| Microsoft.EntityFrameworkCore.Design|   2.2.6         |  8.0.19     | Upgrade to EF Core 8 design package |
| Microsoft.EntityFrameworkCore.Relational|2.2.6        |  8.0.19     | Upgrade to EF Core relational       |
| Microsoft.EntityFrameworkCore.SqlServer|2.2.6         |  8.0.19     | Upgrade to EF Core SQL Server       |
| Newtonsoft.Json                     |  12.0.1;13.0.2  |  13.0.3     | Security vulnerability              |


### Project upgrade details

#### C:\tmp\dotnet-upgrade-sample\eShopLegacy.Utilities\eShopLegacy.Utilities.csproj modifications

Project properties changes:
  - Target framework should be changed from `net461` to `net8.0`
  - Convert project file to SDK-style

NuGet packages changes:
  - (none specified)

Feature upgrades:
  - (none specified)

Other changes:
  - (none specified)

#### C:\tmp\dotnet-upgrade-sample\eShopPorted\eShopPorted.csproj modifications

Project properties changes:
  - Target framework should be changed from `net461` to `net8.0`

NuGet packages changes:
  - Antlr should be updated from `3.5.0.2` to `4.6.6`
  - Microsoft.EntityFrameworkCore should be updated from `2.2.6` to `8.0.19`
  - Microsoft.EntityFrameworkCore.Design should be updated from `2.2.6` to `8.0.19`
  - Microsoft.EntityFrameworkCore.Relational should be updated from `2.2.6` to `8.0.19`
  - Microsoft.EntityFrameworkCore.SqlServer should be updated from `2.2.6` to `8.0.19`
  - Newtonsoft.Json should be updated from `13.0.2` to `13.0.3`

Feature upgrades:
  - (none specified)

Other changes:
  - (none specified)

#### src\eShopLegacyMVC\eShopLegacyMVC.csproj modifications

Project properties changes:
  - Target framework should be changed from `net472` to `net8.0`
  - Convert project file to SDK-style

NuGet packages changes:
  - Antlr should be updated from `3.5.0.2` to `4.6.6`
  - EntityFramework should be updated from `6.2.0` to `6.5.1`
  - Newtonsoft.Json should be replaced with System.Text.Json (user requested)
  - System.Diagnostics.DiagnosticSource should be updated from `4.5.1` to `8.0.1`
  - System.Diagnostics.PerformanceCounter should be updated from `4.5.0` to `8.0.1`
  - System.IO.Pipelines should be updated from `4.5.1` to `8.0.0`
  - System.Runtime.CompilerServices.Unsafe should be updated from `4.5.0` to `6.1.2`
  - System.Threading.Channels should be updated from `4.5.0` to `8.0.0`

Feature upgrades:
  - System.Web.Optimization bundling and minification should be replaced with direct tags
  - Routes registration via RouteCollection should be converted to route mappings
  - Classic EntityFramework initialization needs conversion to EF Core or adjusted initialization
  - Convert from Autofac to ASP.NET Core DI
  - Convert Global.asax.cs initialization to Program.cs and clean up Global.asax.cs

Other changes:
  - Remove obsolete framework-specific packages that are included in net8.0
