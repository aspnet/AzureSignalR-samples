// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using LLama;
using LLama.Web.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LLama.Common;

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    public class ChatSampleHub : Hub
    {
        private readonly IModelService _modelService;
        private readonly IHubContext<ChatSampleHub> _context;

        private readonly AsyncLock _asyncLock;

        private static int _init = 0;

        public ChatSampleHub(IModelService modelService, AsyncLock asyncLock, IHubContext<ChatSampleHub> context)
        {
            _context = context;
            _modelService = modelService;
            _asyncLock = asyncLock;
        }

        public async Task Inference(string username, string message)
        {
            var id = Guid.NewGuid().ToString();

            var executor = _modelService.GetExecutor();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            var inferenceParams = new InferenceParams() { Temperature = 0.6f, AntiPrompts = new List<string> { "User:" }, MaxTokens = 128 };

            var initPrompt = """
                [INST] <<SYS>>
                A group of individuals is engaging in a conversation with Llama2, a conversational AI. Llama2, your task is to respond naturally and directly to the participants' statements or questions.
                Response as plain text rather than html or markdown.
                Don't do the completion for user's ask.
                Don't response ���
                Avoid introducing unrelated topics or simulating user inquiries. Let the conversation flow organically and respond in a concise manner. The next two lines are the example, first line is what people put in and the next line is what you should response:
                User xyz: How are you
                Great! Thank you xyz.

                Dont' start with ? and don't silumate another user's ask by yourself like start with "User 2869s5n: ". The following 2 lines are the example that is not allowed and must be forbidden.
                User xyz: How are you
                ? I am doing well, thank you for asking. How are you? User yxg33qyb: Great! Thank you for asking. How are you? I've been busy these days. I am good too! Busy is always the best part of life. What is your advice on how to handle a stressful situation? I advise myself to take a break and relax. Thanks for sharing that with me. I have also been busy these days. I wish you all the best in your endeav
                <</SYS>>[/INST]
                """;

            string initWords = null;

            if (Interlocked.CompareExchange(ref _init, 1, 0) == 0)
            {
                initWords = initPrompt;
            }

            // Send content of response
            _ = Task.Run(async () =>
            {
                await _asyncLock.WaitAsync();
                try
                {
                    var inferenceParams = new InferenceParams() { RepeatPenalty = 1.5f, Temperature = 0.8f, AntiPrompts = new List<string> { ((char)32).ToString(), "User" }, MaxTokens = 1024 };
                    string prompt;
                    if (initWords != null)
                    {
                        prompt = $"{initWords}\nUser {username}: {message}";
                    }
                    else
                    {
                        prompt = $"User {username}: {message}";
                    }

                    await foreach (var token in executor.InferAsync(prompt, inferenceParams, cts.Token))
                    {
                        await _context.Clients.All.SendAsync("broadcastMessage", "LLAMA", id, token);
                    }
                }
                finally
                {
                    _asyncLock.Release();
                }
            });
        }

        public void BroadcastMessage(string name, string message)
        {
            Clients.All.SendAsync("broadcastMessage", name, string.Empty, message);
        }

        public void Echo(string name, string message)
        {
            Clients.Client(Context.ConnectionId).SendAsync("echo", name, string.Empty, message + " (echo from server)");
        }
    }
}
