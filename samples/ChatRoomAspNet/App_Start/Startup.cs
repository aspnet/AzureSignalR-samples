// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Configuration;
using Owin;

namespace ChatRoomAspNet
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseAzureSignalR(ConfigurationManager.AppSettings["AzureSignalRConnectionString"],
                builder =>
                {
                    builder.UseHub<Chat>();
                });
        }
    }
}