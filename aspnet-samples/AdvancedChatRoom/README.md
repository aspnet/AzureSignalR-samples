Azure SignalR Service Advanced Chat Room
=================================

Just like [ChatRoom sample](../ChatRoom), you can leverage Azure SignalR Service to handle more clients and offload the connection management part. This sample demonstrates more operations available in Azure SignalR Service. Don't forget to add ConnectionString into Web.config before starting the project.

Now the sample supports:

* Echo
* Broadcast
* Join Group / Leave Group
* Send to Group / Groups / Group except connection
* Send to User / Users
* Cookie / JWT based Authentication
* Role / Claim / Policy based Authrization

You can add the following into `Web.config` to enable diagnostics traces for Azure SignalR SDK:
```xml
 <system.diagnostics>
    <sources>
      <source name="Microsoft.Azure.SignalR" switchName="SignalRSwitch">
        <listeners>
          <add name="ASRS" />
        </listeners>
      </source>
    </sources>
    <!-- Sets the trace verbosity level -->
    <switches>
      <add name="SignalRSwitch" value="Information" />
    </switches>
    <!-- Specifies the trace writer for output -->
    <sharedListeners>
      <add name="ASRS" type="System.Diagnostics.TextWriterTraceListener" initializeData="asrs.log.txt" />
    </sharedListeners>
    <trace autoflush="true" />
  </system.diagnostics>
```
