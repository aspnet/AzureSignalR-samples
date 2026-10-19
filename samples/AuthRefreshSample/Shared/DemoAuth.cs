// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

namespace AuthRefreshSample;

// DEMO ONLY. Shared HS256 signing material so the client can mint app tokens the server validates
// without a real identity provider. In production the app token comes from your IdP and you must
// never hard-code signing keys. This single file is linked into both the Server and Client projects.
internal static class DemoAuth
{
    public const string Issuer = "auth-refresh-sample";

    public const string Audience = "auth-refresh-sample-hub";

    // Must be >= 256 bits (32 bytes) for HS256.
    public const string SigningKey = "auth-refresh-sample-demo-signing-key-please-change-0123456789";
}
