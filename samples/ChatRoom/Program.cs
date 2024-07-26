using Azure.Identity;

using Microsoft.Azure.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR().AddAzureSignalR(option =>
{
    // Here are sample codes for sovereign clouds
    //
    // var credentialOptions = new ClientSecretCredentialOptions()
    // {
    //     AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
    //     // AuthorityHost = AzureAuthorityHosts.AzureGovernment, // for fairfax region
    //     // AuthorityHost = AzureAuthorityHosts.AzureChina, // for mooncake region
    // };

    // var tenantId = "";
    // var clientId = "";
    // var clientSecret = "";

    // option.Endpoints = new ServiceEndpoint[] {
    //     new(new Uri("https://<hostname>"), new ClientSecretCredential(tenantId, clientId, clientSecret, credentialOptions))
    // };
});
var app = builder.Build();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapHub<ChatSampleHub>("/chat");

app.Run();