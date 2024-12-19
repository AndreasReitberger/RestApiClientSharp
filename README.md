# RestApiClientSharp
Is a common library for a REST API client solution.

# Support me
If you want to support me, you can order over following affilate links (I'll get a small share from your purchase from the corresponding store).

- Prusa: http://www.prusa3d.com/#a_aid=AndreasReitberger *
- Jake3D: https://tidd.ly/3x9JOBp * 
- Amazon: https://amzn.to/2Z8PrDu *
- Coinbase: https://advanced.coinbase.com/join/KTKSEBP * (10€ in BTC for you if you open an account)
- TradeRepublic: https://refnocode.trade.re/wfnk80zm * (10€ in stocks for you if you open an account)

(*) Affiliate link
Thank you very much for supporting me!

# Nuget
Get the latest version from nuget.org<br>
[![NuGet](https://img.shields.io/nuget/v/RestApiClientSharp.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/RestApiClientSharp/)
[![NuGet](https://img.shields.io/nuget/dt/RestApiClientSharp.svg)](https://www.nuget.org/packages/RestApiClientSharp)

# How to use
## Available REST functions

```cs
public virtual async Task<IRestApiRequestRespone?> SendRestApiRequestAsync(...);
public virtual async Task<IRestApiRequestRespone?> SendMultipartFormDataFileRestApiRequestAsync(...);
public virtual async Task<byte[]?> DownloadFileFromUriAsync(...);
```

### SendRestApiRequestAsync()
This function allows you to send a simple REST request to your REST-API-Server endpoint.
If succeeded, you can access the received `string` and `byte[]` as followed.

```cs
IRestApiRequestRespone.Result
IRestApiRequestRespone.RawBytes
```

### SendMultipartFormDataFileRestApiRequestAsync()
This function allows you to send a multi form data REST request to your REST-API-Server endpoint.
If succeeded, you can access the received `string` and `byte[]`, the same as with the `SendRestApiRequestAsync()` function.

### DownloadFileFromUriAsync()
This function allows you to download a file as `byte[]`. 

## Integration
You just can inherit from the `RestApiClient` class to get access to all common REST functions of our client.

```cs
public partial class MyCustomRestClient : RestApiClient
{
    #region Constructor
    public MyCustomRestClient() : base() 
    { }
    
    public MyCustomRestClient(string apiKey, string tokenName = "token") : base(
        new AuthenticationHeader() { Token = apiKey, Target = AuthenticationHeaderTarget.UrlSegment},
        tokenName) 
    { }
    
    public MyCustomRestClient(string webAddress, string apiKey, string tokenName = "token") : base(
        new AuthenticationHeader() { Token = apiKey, Target = AuthenticationHeaderTarget.UrlSegment },
        tokenName, webAddress)
    { }
    #endregion
}
```

Example of communication
```cs
public async Task<StockQuoteResult?> GetStockQuoteAsync(string symbol)
{
    IRestApiRequestRespone? result = null;
    StockQuoteResult? resultObject = null;
    Dictionary<string, string> parameters = [];
    try
    {
        if (!string.IsNullOrEmpty(symbol)) parameters.Add("symbol", symbol);
        string targetUri = $"";
        result = await SendRestApiRequestAsync(
               requestTargetUri: targetUri,
               method: Method.Get,
               command: "quote",
               jsonObject: null,
               authHeaders: AuthHeaders,
               // Parameters
               urlSegments: parameters,
               cts: default
               )
            .ConfigureAwait(false);
        resultObject = GetObjectFromJson<StockQuoteResult>(result?.Result, NewtonsoftJsonSerializerSettings);
        return resultObject;
    }
    catch (Exception exc)
    {
        OnError(new UnhandledExceptionEventArgs(exc, false));
        return resultObject;
    }
}
```

You will find some complete, production examples here: 
- https://github.com/AndreasReitberger/FinnhubStockApiSharp [in progress...]
- https://github.com/AndreasReitberger/LexOfficeClientSharp [in progress...]
