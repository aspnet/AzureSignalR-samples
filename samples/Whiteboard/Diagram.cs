// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
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

    public byte[] Background { get; set; }

    public string BackgroundContentType { get; set; }

    public string BackgroundId { get; set; }

    public ConcurrentDictionary<string, Shape> Shapes { get; } = new ConcurrentDictionary<string, Shape>();

    public int UserEnter()
    {
        return Interlocked.Increment(ref totalUsers);
    }

    public int UserLeave()
    {
        return Interlocked.Decrement(ref totalUsers);
    }
}