# CurrencyTile

App for pinning currency rates and stock ticker prices to Windows 10's start menu.

It won't crash or anything on Windows 11, but the pinned tiles won't be live, 'cause Live Tiles are gone in 11.

## Building

### Requirements
 - WinUI 3+
 - Windows 10 SDK (10.0.19041.0)

This app connects to CurrencyBeacon and FinancialModelingPrep APIs, which each require (free!) API keys.
Once you've obtained them, create two text files in the `CurrencyTile.TimerTask` project:
 - api_key_currencybeacon.txt
 - api_key_financialmodelingprep.txt

Each file should contain the respective API key, and nothing else.

**Make sure to set the files' build actions to `Embedded Resource`.** i.e. make the following additions to the
.csproj:

```xml
  <ItemGroup>
    <None Remove="api_key_currencybeacon.txt" />
    <None Remove="api_key_financialmodelingprep.txt" />
    <EmbeddedResource Include="api_key_currencybeacon.txt" />
    <EmbeddedResource Include="api_key_financialmodelingprep.txt" />
  </ItemGroup>
```

Otherwise, F5 and joy.