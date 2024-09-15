using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorAppTest01.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();


// Load configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


// Add HttpClient for API calls
builder.Services.AddHttpClient("BlazorAppTest01.ServerAPI", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
})
.AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();


builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("BlazorAppTest01.ServerAPI"));

// Add OIDC authentication
builder.Services.AddOidcAuthentication(options =>
{
    var webAppVc = builder.Configuration.GetSection("WebAppVerifiableCredential");
    options.ProviderOptions.Authority = webAppVc["Authority"];
    options.ProviderOptions.ClientId = webAppVc["ClientId"];
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
});

// Add configuration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);


await builder.Build().RunAsync();
