# Complete Guide: Publishing RFE Auth MCP Server

This guide outlines how to submit, register, and publish the `RFE.Auth.Netcore` MCP server across the three main ecosystems: **ChatGPT Plugin Directory**, **Claude & MCP Registries**, and **Docker MCP Toolkit**.

---

## 1. ChatGPT Plugin / OpenAI App Store

### Files Created
- [`ai-plugin.json`](file:///c:/Users/rogth/source/repos/rogthat7/rfe.auth.netcore/RFE.Auth.API/wwwroot/.well-known/ai-plugin.json)
- [`openai-app-metadata.md`](file:///c:/Users/rogth/source/repos/rogthat7/rfe.auth.netcore/docs/openai-app-metadata.md)

### Publishing Steps
1. Deploy `RFE.Auth.Netcore` to a public HTTPS URL (e.g. `https://auth.yourdomain.com`).
2. Verify `https://auth.yourdomain.com/.well-known/ai-plugin.json` is publicly reachable.
3. Log into [OpenAI Platform Dashboard](https://platform.openai.com/).
4. Create a new App / Plugin capability, pointing to your domain.
5. Configure OAuth 2.1 authorization using your OpenIddict endpoints:
   - Authorization URL: `https://auth.yourdomain.com/connect/authorize`
   - Token URL: `https://auth.yourdomain.com/connect/token`
6. Submit for OpenAI review. Once approved, users can search for and install your plugin directly inside ChatGPT's Plugin Directory.

---

## 2. Claude Desktop & MCP Registries (Smithery & Official Registry)

### Files Created
- [`smithery.yaml`](file:///c:/Users/rogth/source/repos/rogthat7/rfe.auth.netcore/smithery.yaml) (Smithery.ai manifest)
- [`mcp-registry.json`](file:///c:/Users/rogth/source/repos/rogthat7/rfe.auth.netcore/mcp-registry.json) (Anthropic Official Registry manifest)

### Publishing to Smithery.ai (1-Click Install for Claude / Cursor / Windsurf)
Run the following command from the root directory:
```bash
npx smithery@latest publish
```
This registers your server on [smithery.ai](https://smithery.ai), enabling any Claude user to install it with:
```bash
npx smithery@latest install rfe-auth-api
```

### Submitting to Official `@modelcontextprotocol/registry`
1. Fork the [modelcontextprotocol/registry](https://github.com/modelcontextprotocol/registry) repo.
2. Add `mcp-registry.json` under `servers/rfe-auth-api.json`.
3. Open a Pull Request to merge your listing into the official Anthropic MCP registry.

---

## 3. Docker MCP Toolkit & Catalog

### Files Created
- [`Dockerfile.mcp`](file:///c:/Users/rogth/source/repos/rogthat7/rfe.auth.netcore/Dockerfile.mcp)
- [`docker-mcp.json`](file:///c:/Users/rogth/source/repos/rogthat7/rfe.auth.netcore/docker-mcp.json)
- [`docker-compose.mcp.yml`](file:///c:/Users/rogth/source/repos/rogthat7/rfe.auth.netcore/docker-compose.mcp.yml)

### Testing Locally
```powershell
docker compose -f docker-compose.mcp.yml up -d --build
```
Your MCP server will be live at `http://localhost:3000`.

### Submitting to Docker MCP Catalog
1. Build and push your Docker image to Docker Hub:
   ```bash
   docker build -f Dockerfile.mcp -t yourusername/rfe-auth-mcp:latest .
   docker push yourusername/rfe-auth-mcp:latest
   ```
2. Submit `docker-mcp.json` to the [Docker MCP Catalog](https://github.com/docker/mcp) repository as a pull request.
3. Once merged, users can install your MCP server from the **Docker Desktop MCP Extension Catalog** with a single click.
