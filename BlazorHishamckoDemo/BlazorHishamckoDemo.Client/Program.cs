using BlazorHishamckoDemo.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddScoped<ExcelService>();

await builder.Build().RunAsync();
