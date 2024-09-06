using Projects;

var builder = DistributedApplication.CreateBuilder(args);
#pragma warning disable AZPROVISION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var asrs = builder.AddAzureSignalR(ServiceConstants.SignalRServiceName, (_, _, r) => r.AssignProperty(s => s.Sku.Name, "'Standard_S1'"));
#pragma warning restore AZPROVISION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// Use WithReplicas to start multiple instances of ChatRoom
builder.AddProject<ChatRoom>(nameof(ChatRoom)).WithReplicas(2).WithReference(asrs);
builder.AddProject<AckableChatRoom>(nameof(AckableChatRoom)).WithReference(asrs);
builder.AddProject<AdvancedChatRoom>(nameof(AdvancedChatRoom)).WithReference(asrs);
builder.AddProject<BlazorChat>(nameof(BlazorChat)).WithReference(asrs);
builder.AddProject<FlightMap>(nameof(FlightMap)).WithReference(asrs);

builder.AddProject<ClientInvocationSample>(nameof(ClientInvocationSample)).WithReference(asrs);

builder.AddProject<GitHubChat>(nameof(GitHubChat)).WithReference(asrs);
builder.AddProject<ServerSideBlazor>(nameof(ServerSideBlazor)).WithReference(asrs);

builder.Build().Run();
