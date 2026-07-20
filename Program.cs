using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using CoachManagerPwa.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<CoachManagerPwa.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

var supabaseUrl = "https://spyalzbjcfdrkbyqkopa.supabase.co";
var supabaseAnonKey = "sb_publishable_TTHrQfMHDtcJfKWNu9SG-w_eaSeIiyR";

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

await builder.Build().RunAsync();