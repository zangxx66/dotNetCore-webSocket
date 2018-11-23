using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace dotNetCore_websokect_demo {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            services.AddMvc ().SetCompatibilityVersion (CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            } else {
                app.UseHsts ();
            }

            var webSocketOptions = new WebSocketOptions () {
                KeepAliveInterval = TimeSpan.FromSeconds (3600),
                ReceiveBufferSize = 1024 * 1024
            };
            app.UseWebSockets (webSocketOptions);
            app.Use (async (context, next) => {
                if (context.Request.Path == "/ws") {
                    if (context.WebSockets.IsWebSocketRequest) {
                        var websocket = await context.WebSockets.AcceptWebSocketAsync ();
                        await Echo (context, websocket);
                    } else {
                        context.Response.StatusCode = 400;
                    }
                } else {
                    await next ();
                }
            });
            app.UseFileServer();
        }

        private async Task Echo (HttpContext context, WebSocket webSocket) {
            var buffer = new byte[1024 * 1024];
            var result = await webSocket.ReceiveAsync (new ArraySegment<byte> (buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue) {
                await webSocket.SendAsync (new ArraySegment<byte> (buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync (new ArraySegment<byte> (buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync (result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}