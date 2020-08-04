// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Watch.BrowserRefresh
{
    /// <summary>
    /// Helper class that handles the HTML injection into
    /// a string or byte array.
    /// </summary>
    public static class WebSocketScriptInjection
    {
        private const string DotNetWatchMarker = "<!-- dotnet-watch-browser-refresh -->";
        private const string BodyMarker = "</body>";

        private static readonly byte[] _bodyBytes = Encoding.UTF8.GetBytes(BodyMarker);
        private static readonly byte[] _markerBytes = Encoding.UTF8.GetBytes(DotNetWatchMarker);
        private static readonly byte[] _scriptInjectionBytes = Encoding.UTF8.GetBytes(GetWebSocketClientJavaScript());

        public static async ValueTask<bool> TryInjectLiveReloadScriptAsync(byte[] buffer, int offset, int count, Stream baseStream)
        {
            buffer = buffer.AsSpan(offset, count).ToArray();
            var index = buffer.AsSpan().LastIndexOf(_markerBytes);
            if (index > -1)
            {
                await baseStream.WriteAsync(buffer, 0, buffer.Length);
                return false;
            }

            index = buffer.AsSpan().LastIndexOf(_bodyBytes);
            if (index == -1)
            {
                await baseStream.WriteAsync(buffer, 0, buffer.Length);
                return false;
            }

            var endIndex = index + _bodyBytes.Length;

            // Write pre-marker buffer
            await baseStream.WriteAsync(buffer, 0, Math.Max(0, index - 1));

            // Write the injected script
            await baseStream.WriteAsync(_scriptInjectionBytes);

            // Write the rest of the buffer/HTML doc
            await baseStream.WriteAsync(buffer, endIndex, buffer.Length - endIndex);
            return true;
        }

        private static string GetWebSocketClientJavaScript()
        {
            var hostString = Environment.GetEnvironmentVariable("DOTNET_WATCH_BROWSER_REFRESH_URL");
            var jsFileName = "Microsoft.AspNetCore.Watch.BrowserRefresh.WebSocketScriptInjection.js";
            using var reader = new StreamReader(typeof(WebSocketScriptInjection).Assembly.GetManifestResourceStream(jsFileName)!);
            var script = reader.ReadToEnd().Replace("{{hostString}}", hostString);

            return $"<script>{script}</script>";
        }
    }
}
