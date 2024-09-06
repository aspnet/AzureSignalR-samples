using Azure.Identity;

using Microsoft.Azure.SignalR;

var builder = WebApplication.CreateBuilder(args);
var asrs = builder.Services.AddSignalR().AddNamedAzureSignalR(ServiceConstants.SignalRServiceName);
var app = builder.Build();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapHub<ChatSampleHub>("/chat");

app.Run();