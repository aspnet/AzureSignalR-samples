// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Microsoft.Azure.SignalR.Samples.Whiteboard;

public class DrawHub(Diagram diagram) : Hub
{
    private readonly Diagram diagram = diagram;

    private async Task UpdateShape(string id, Shape shape)
    {
        diagram.Shapes[id] = shape;
        await Clients.Others.SendAsync("ShapeUpdated", id, shape.GetType().Name, shape);
    }

    public override Task OnConnectedAsync()
    {
        var t = Task.WhenAll(diagram.Shapes.AsEnumerable().Select(l => Clients.Client(Context.ConnectionId).SendAsync("ShapeUpdated", l.Key, l.Value.GetType().Name, l.Value)));
        if (diagram.Background != null) t = t.ContinueWith(_ => Clients.Client(Context.ConnectionId).SendAsync("BackgroundUpdated", diagram.BackgroundId));
        return t.ContinueWith(_ => Clients.All.SendAsync("UserUpdated", diagram.UserEnter()));
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        return Clients.All.SendAsync("UserUpdated", diagram.UserLeave());
    }

    public async Task RemoveShape(string id)
    {
        diagram.Shapes.Remove(id, out _);
        await Clients.Others.SendAsync("ShapeRemoved", id);
    }

    public async Task AddOrUpdatePolyline(string id, Polyline polyline)
    {
        await this.UpdateShape(id, polyline);
    }

    public async Task PatchPolyline(string id, Polyline polyline)
    {
        if (diagram.Shapes[id] is not Polyline p) throw new InvalidOperationException($"Shape {id} does not exist or is not a polyline.");
        if (polyline.Color != null) p.Color = polyline.Color;
        if (polyline.Width != 0) p.Width = polyline.Width;
        p.Points.AddRange(polyline.Points);
        await Clients.Others.SendAsync("ShapePatched", id, polyline);
    }

    public async Task AddOrUpdateLine(string id, Line line)
    {
        await this.UpdateShape(id, line);
    }

    public async Task AddOrUpdateCircle(string id, Circle circle)
    {
        await this.UpdateShape(id, circle);
    }

    public async Task AddOrUpdateRect(string id, Rect rect)
    {
        await this.UpdateShape(id, rect);
    }

    public async Task AddOrUpdateEllipse(string id, Ellipse ellipse)
    {
        await this.UpdateShape(id, ellipse);
    }

    public async Task Clear()
    {
        diagram.Shapes.Clear();
        diagram.Background = null;
        await Clients.Others.SendAsync("Clear");
    }

    public async Task SendMessage(string name, string message)
    {
        await Clients.Others.SendAsync("NewMessage", name, message);
    }
}