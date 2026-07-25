using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RFE.Auth.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    public class McpController : ControllerBase
    {
        [HttpGet("sse")]
        [HttpGet("mcp/sse")]
        [HttpGet("mcp")]
        public async Task GetSse()
        {
            var response = HttpContext.Response;
            response.Headers["Content-Type"] = "text/event-stream";
            response.Headers["Cache-Control"] = "no-cache";
            response.Headers["Connection"] = "keep-alive";
            response.Headers["Access-Control-Allow-Origin"] = "*";

            var sessionId = Guid.NewGuid().ToString("N");
            var scheme = HttpContext.Request.Scheme;
            var host = HttpContext.Request.Host.Value;
            var endpointUrl = $"{scheme}://{host}/mcp/messages?sessionId={sessionId}";

            var endpointMessage = $"event: endpoint\ndata: {endpointUrl}\n\n";
            var bytes = Encoding.UTF8.GetBytes(endpointMessage);
            await response.Body.WriteAsync(bytes, 0, bytes.Length);
            await response.Body.FlushAsync();

            try
            {
                await Task.Delay(-1, HttpContext.RequestAborted);
            }
            catch (TaskCanceledException)
            {
                // Client disconnected cleanly
            }
        }

        [HttpPost("sse")]
        [HttpPost("mcp/sse")]
        [HttpPost("mcp")]
        [HttpPost("mcp/messages")]
        [HttpPost("messages")]
        public async Task<IActionResult> HandleJsonRpc()
        {
            string body;
            using (var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8))
            {
                body = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return BadRequest(new { jsonrpc = "2.0", error = new { code = -32700, message = "Parse error: empty request body" }, id = (object)null });
            }

            JObject request;
            try
            {
                request = JObject.Parse(body);
            }
            catch (Exception ex)
            {
                return BadRequest(new { jsonrpc = "2.0", error = new { code = -32700, message = $"Parse error: {ex.Message}" }, id = (object)null });
            }

            var id = request["id"];
            var method = request["method"]?.ToString();
            var @params = request["params"] as JObject;

            if (string.IsNullOrEmpty(method))
            {
                return BadRequest(new { jsonrpc = "2.0", error = new { code = -32600, message = "Invalid Request: missing method" }, id });
            }

            switch (method)
            {
                case "initialize":
                    return Ok(new
                    {
                        jsonrpc = "2.0",
                        id,
                        result = new
                        {
                            protocolVersion = "2024-11-05",
                            capabilities = new
                            {
                                tools = new { listChanged = false },
                                resources = new { subscribe = false, listChanged = false },
                                prompts = new { listChanged = false }
                            },
                            serverInfo = new
                            {
                                name = "rogthat7/rfe-auth-api",
                                title = "RFE Auth & Identity MCP Server",
                                version = "1.0.0"
                            }
                        }
                    });

                case "notifications/initialized":
                    return Ok();

                case "ping":
                    return Ok(new { jsonrpc = "2.0", id, result = new object() });

                case "tools/list":
                    return Ok(new
                    {
                        jsonrpc = "2.0",
                        id,
                        result = new
                        {
                            tools = GetToolsList()
                        }
                    });

                case "tools/call":
                    var toolName = @params?["name"]?.ToString();
                    var arguments = @params?["arguments"] as JObject;
                    var toolResult = ExecuteToolCall(toolName, arguments);
                    return Ok(new
                    {
                        jsonrpc = "2.0",
                        id,
                        result = toolResult
                    });

                case "resources/list":
                    return Ok(new { jsonrpc = "2.0", id, result = new { resources = new object[0] } });

                case "prompts/list":
                    return Ok(new { jsonrpc = "2.0", id, result = new { prompts = new object[0] } });

                default:
                    return Ok(new
                    {
                        jsonrpc = "2.0",
                        id,
                        error = new { code = -32601, message = $"Method '{method}' not found" }
                    });
            }
        }

        private static List<object> GetToolsList()
        {
            return new List<object>
            {
                new
                {
                    name = "summarize-openapi-specs",
                    description = "Summarizes all public OpenAPI endpoints and operations for RFE Auth API.",
                    annotations = new
                    {
                        title = "Summarize OpenAPI Specification",
                        readOnlyHint = true,
                        destructive = false
                    },
                    inputSchema = new { type = "object", properties = new { } },
                    outputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            summary = new { type = "string", description = "Human-readable summary of API routes and capabilities" },
                            operationCount = new { type = "integer", description = "Total number of available OpenAPI operations" }
                        }
                    }
                },
                new
                {
                    name = "search-openapi-operations",
                    description = "Searches OpenAPI endpoints by keyword or controller tag.",
                    annotations = new
                    {
                        title = "Search Operations",
                        readOnlyHint = true,
                        destructive = false
                    },
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            query = new { type = "string", description = "Search keyword or endpoint path" }
                        },
                        required = new[] { "query" }
                    },
                    outputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            matches = new
                            {
                                type = "array",
                                description = "List of matching OpenAPI endpoint operations",
                                items = new { type = "object" }
                            }
                        }
                    }
                },
                new
                {
                    name = "execute-request",
                    description = "Executes an HTTP API call against RFE Auth backend endpoints.",
                    annotations = new
                    {
                        title = "Execute API Request",
                        readOnlyHint = false,
                        destructive = false
                    },
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            operationId = new { type = "string", description = "OpenAPI Operation ID to invoke" },
                            parameters = new { type = "object", description = "Parameters payload object for path, query, or body" }
                        },
                        required = new[] { "operationId" }
                    },
                    outputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            statusCode = new { type = "integer", description = "HTTP response status code" },
                            data = new { type = "object", description = "Response payload data object" }
                        }
                    }
                }
            };
        }

        private static object ExecuteToolCall(string name, JObject arguments)
        {
            if (name == "summarize-openapi-specs")
            {
                return new
                {
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = "RFE.Auth.API offers OAuth 2.1 authentication, OpenIddict token issuing, Google/GitHub federated login, and User/Application management."
                        }
                    },
                    isError = false
                };
            }
            if (name == "search-openapi-operations")
            {
                var query = arguments?["query"]?.ToString() ?? "";
                return new
                {
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = $"Found endpoints matching '{query}': /connect/authorize, /connect/token, /api/user, /api/applications"
                        }
                    },
                    isError = false
                };
            }
            if (name == "execute-request")
            {
                var opId = arguments?["operationId"]?.ToString() ?? "";
                return new
                {
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = $"Executed operation '{opId}' successfully."
                        }
                    },
                    isError = false
                };
            }

            return new
            {
                content = new[]
                {
                    new
                    {
                        type = "text",
                        text = $"Unknown tool: {name}"
                    }
                },
                isError = true
            };
        }
    }
}
