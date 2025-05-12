// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;

namespace Microsoft.Azure.SignalR.Samples.Whiteboard;

public class DrawHub(Diagram diagram) : Hub
{
    private readonly Diagram diagram = diagram;

    private async Task<int> UpdateShape(string id, Shape shape)
    {
        var z = diagram.AddOrUpdateShape(id, shape);
        await Clients.Others.SendAsync("ShapeUpdated", id, shape.GetType().Name, shape, z);
        return z;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("UserUpdated", diagram.UserEnter());
        if (diagram.Background != null) await Clients.Client(Context.ConnectionId).SendAsync("BackgroundUpdated", diagram.BackgroundId);
        foreach (var s in diagram.GetShapes())
            await Clients.Caller.SendAsync("ShapeUpdated", s.Item1, s.Item3.GetType().Name, s.Item3, s.Item2);
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        return Clients.All.SendAsync("UserUpdated", diagram.UserLeave());
    }

    public async Task RemoveShape(string id)
    {
        diagram.RemoveShape(id);
        await Clients.Others.SendAsync("ShapeRemoved", id);
    }

    public async Task<int> AddOrUpdatePolyline(string id, Polyline polyline)
    {
        return await this.UpdateShape(id, polyline);
    }

    public async Task PatchPolyline(string id, Polyline polyline)
    {
        if (diagram.GetShape(id) is not Polyline p) throw new InvalidOperationException($"Shape {id} does not exist or is not a polyline.");
        if (polyline.Color != null) p.Color = polyline.Color;
        if (polyline.Width != 0) p.Width = polyline.Width;
        p.Points.AddRange(polyline.Points);
        await Clients.Others.SendAsync("ShapePatched", id, polyline);
    }

    public async Task<int> AddOrUpdateLine(string id, Line line)
    {
        return await this.UpdateShape(id, line);
    }

    public async Task<int> AddOrUpdateCircle(string id, Circle circle)
    {
        return await this.UpdateShape(id, circle);
    }

    public async Task<int> AddOrUpdateRect(string id, Rect rect)
    {
        return await this.UpdateShape(id, rect);
    }

    public async Task<int> AddOrUpdateEllipse(string id, Ellipse ellipse)
    {
        return await this.UpdateShape(id, ellipse);
    }

    public async Task Clear()
    {
        diagram.ClearShapes();
        diagram.Background = null;
        await Clients.Others.SendAsync("Clear");
    }

    public async Task SendMessage(string name, string message)
    {
        await Clients.Others.SendAsync("NewMessage", name, message);
    }
}