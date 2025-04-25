// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.Whiteboard;

[Route("/background")]
public class BackgroundController(IHubContext<DrawHub> context, Diagram diagram) : Controller
{
    private readonly IHubContext<DrawHub> hubContext = context;
    private readonly Diagram diagram = diagram;

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        diagram.BackgroundId = Guid.NewGuid().ToString().Substring(0, 8);
        diagram.Background = new byte[file.Length];
        diagram.BackgroundContentType = file.ContentType;
        using (var stream = new MemoryStream(diagram.Background))
        {
            await file.CopyToAsync(stream);
        }

        await hubContext.Clients.All.SendAsync("BackgroundUpdated", diagram.BackgroundId);

        return Ok();
    }

    [HttpGet("{id}")]
    public IActionResult Download(string id)
    {
        if (diagram.BackgroundId != id) return NotFound();
        return File(diagram.Background, diagram.BackgroundContentType);
    }
}