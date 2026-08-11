using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using CoachManagerPwa.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<CoachManagerPwa.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
//Dev
//var supabaseUrl = "https://spyalzbjcfdrkbyqkopa.supabase.co";
//var supabaseAnonKey = "sb_publishable_TTHrQfMHDtcJfKWNu9SG-w_eaSeIiyR";

//Prod
var supabaseUrl = "https://wwzrsibzpjlckjocjfpn.supabase.co";
var supabaseAnonKey = "sb_publishable_tlMLmxvltr6IeipkwS0Tdg_X6QqpJLq";


builder.Services.AddScoped(sp =>
    new Supabase.Client(supabaseUrl, supabaseAnonKey, new Supabase.SupabaseOptions
    {
        AutoConnectRealtime = false,
        Headers = new Dictionary<string, string>
        {
            { "apikey", supabaseAnonKey }
        }
    })
);

builder.Services.AddScoped<IDataService, SupabaseDataService>();
builder.Services.AddSingleton<AppState>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IFeatureService, FeatureService>();
builder.Services.AddSingleton<ILocalityService>(sp =>
    new LocalityService(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }));
builder.Services.AddSingleton(sp =>
    new ContractGeneratorService(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }));


await builder.Build().RunAsync();