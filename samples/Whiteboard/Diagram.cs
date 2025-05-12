// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Azure.SignalR.Samples.Whiteboard;

public class Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

public abstract class Shape
{
    public string Color { get; set; }

    public int Width { get; set; }

    public string Fill { get; set; }
}

public class Polyline : Shape
{
    public List<Point> Points { get; set; }
}

public class Line : Shape
{
    public Point Start { get; set; }

    public Point End { get; set; }
}

public class Circle : Shape
{
    public Point Center { get; set; }

    public int Radius { get; set; }
}

public class Rect : Shape
{
    public Point TopLeft { get; set; }

    public Point BottomRight { get; set; }
}

public class Ellipse : Shape
{
    public Point TopLeft { get; set; }

    public Point BottomRight { get; set; }
}

public class Diagram
{
    private int totalUsers = 0;

    private int currentZIndex = -1;

    private readonly ConcurrentDictionary<string, Tuple<int, Shape>> shapes = new();

    public byte[] Background { get; set; }

    public string BackgroundContentType { get; set; }

    public string BackgroundId { get; set; }

    public int AddOrUpdateShape(string id, Shape shape)
    {
        var s = shapes.AddOrUpdate(id, _ => Tuple.Create(Interlocked.Increment(ref currentZIndex), shape), (_, v) => Tuple.Create(v.Item1, shape));
        return s.Item1;
    }

    public void RemoveShape(string id)
    {
        shapes.TryRemove(id, out _);
    }

    public Shape GetShape(string id)
    {
        return shapes[id].Item2;
    }

    public void ClearShapes()
    {
        shapes.Clear();
    }

    public IEnumerable<Tuple<string, int, Shape>> GetShapes()
    {
        return shapes.AsEnumerable().Select(l => Tuple.Create(l.Key, l.Value.Item1, l.Value.Item2));
    }

    public int UserEnter()
    {
        return Interlocked.Increment(ref totalUsers);
    }

    public int UserLeave()
    {
        return Interlocked.Decrement(ref totalUsers);
    }
}