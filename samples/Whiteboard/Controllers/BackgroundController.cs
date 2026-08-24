// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.Whiteboard;

[Route("/background")]
public class BackgroundController(IHubContext<DrawHub> context, Diagram diagram) : Controller
{
    private const long MaxBackgroundFileSize = 5 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, byte[]> ImageSignatures = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
        ["image/jpeg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        ["image/gif"] = new byte[] { 0x47, 0x49, 0x46, 0x38 }
    };

    private readonly IHubContext<DrawHub> hubContext = context;
    private readonly Diagram diagram = diagram;

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0 || file.Length > MaxBackgroundFileSize)
        {
            return BadRequest("The background image is empty or too large.");
        }

        if (!ImageSignatures.TryGetValue(file.ContentType ?? string.Empty, out var signature))
        {
            return BadRequest("The background image type is not supported.");
        }

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var background = stream.ToArray();
        if (!StartsWithSignature(background, signature))
        {
            return BadRequest("The background image content is invalid.");
        }

        diagram.BackgroundId = Guid.NewGuid().ToString().Substring(0, 8);
        diagram.Background = background;
        diagram.BackgroundContentType = file.ContentType;

        await hubContext.Clients.All.SendAsync("BackgroundUpdated", diagram.BackgroundId);

        return Ok();
    }

    [HttpGet("{id}")]
    public IActionResult Download(string id)
    {
        if (diagram.BackgroundId != id) return NotFound();

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(diagram.Background, diagram.BackgroundContentType);
    }

    private static bool StartsWithSignature(byte[] content, byte[] signature)
    {
        if (content.Length < signature.Length) return false;

        for (var i = 0; i < signature.Length; i++)
        {
            if (content[i] != signature[i]) return false;
        }

        return true;
    }
}
