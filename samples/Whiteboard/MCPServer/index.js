import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import { z } from 'zod';
import { HubConnectionBuilder } from '@microsoft/signalr';
import dotenv from 'dotenv';

dotenv.config();

const logger = new class {
  log = (level, message) => level > 1 && console.error(`[${level}] ${message}`);
};

const connection = new HubConnectionBuilder().withUrl(`${process.env['WHITEBOARD_ENDPOINT'] || 'http://localhost:5000'}/draw`).withAutomaticReconnect().configureLogging(logger).build();

const server = new McpServer({
  name: 'Whiteboard',
  version: '1.0.0'
});

let color = z.string().describe('color of the shape, valid values are: black, grey, darkred, red, orange, yellow, green, deepskyblue, indigo, purple');
let width = z.number().describe('width of the shape, valid values are: 1, 2, 4, 8');
let point = z.object({
  x: z.number().describe('x coordinate of the point, 0 denotes the left edge of the whiteboard'),
  y: z.number().describe('y coordinate of the point, 0 denotes the top edge of the whiteboard')
});
let id = z.string().describe('unique identifier of the shape, if it does not exist, it will be created, if it exists, it will be updated');

server.tool('send_message', 'post a message on whiteboard', { name: z.string(), message: z.string() }, async ({ name, message }) => {
  await connection.send('sendMessage', name, message);
  return { content: [{ type: 'text', text: 'Message sent' }] }
});

server.tool(
  'add_or_update_polyline', 'add or update a polyline on whiteboard',
  {
    id, polyline: z.object({
      color, width,
      points: z.array(point).describe('array of points that define the polyline')
    })
  },
  async ({ id, polyline }) => {
    await connection.send('addOrUpdatePolyline', id, polyline);
    return { content: [{ type: 'text', text: 'Polyline added or updated' }] };
  });

server.tool(
  'add_or_update_line', 'add or update a line on whiteboard',
  {
    id, line: z.object({
      color, width,
      start: point.describe('start point of the line'),
      end: point.describe('end point of the line')
    })
  },
  async ({ id, line }) => {
    await connection.send('addOrUpdateLine', id, line);
    return { content: [{ type: 'text', text: 'Line added or updated' }] };
  });

server.tool(
  'add_or_update_circle', 'add or update a circle on whiteboard',
  {
    id, circle: z.object({
      color, width,
      center: point.describe('center point of the circle'),
      radius: z.number().describe('radius of the circle')
    })
  },
  async ({ id, circle }) => {
    await connection.send('addOrUpdateCircle', id, circle);
    return { content: [{ type: 'text', text: 'Circle added or updated' }] };
  });

server.tool(
  'add_or_update_rect', 'add or update a rectangle on whiteboard',
  {
    id, rect: z.object({
      color, width,
      topLeft: point.describe('top left corner of the rectangle'),
      bottomRight: point.describe('bottom right of the rectangle')
    })
  },
  async ({ id, rect }) => {
    await connection.send('addOrUpdateRect', id, rect);
    return { content: [{ type: 'text', text: 'Rectangle added or updated' }] };
  });

server.tool(
  'add_or_update_ellipse', 'add or update an ellipse on whiteboard',
  {
    id, ellipse: z.object({
      color, width,
      topLeft: point.describe('top left corner of the bounding rectangle of the ellipse'),
      bottomRight: point.describe('bottom right of the bounding rectangle of the ellipse')
    })
  },
  async ({ id, ellipse }) => {
    await connection.send('addOrUpdateEllipse', id, ellipse);
    return { content: [{ type: 'text', text: 'Ellipse added or updated' }] };
  });

server.tool(
  'remove_shape', 'remove a shape from whiteboard',
  { id },
  async ({ id }) => {
    await connection.send('removeShape', id);
    return { content: [{ type: 'text', text: 'Shape removed' }] };
  });

server.tool(
  'clear', 'clear the whiteboard',
  {},
  async () => {
    await connection.send('clear');
    return { content: [{ type: 'text', text: 'Whiteboard cleared' }] };
  });

const transport = new StdioServerTransport();

await server.connect(transport);

const sleep = ms => new Promise(resolve => setTimeout(resolve, ms));
for (;;) {
  try {
    await connection.start();
    break;
  } catch (e) {
    console.error('Failed to start SignalR connection: ' + e.message);
    await sleep(5000);
  }
}