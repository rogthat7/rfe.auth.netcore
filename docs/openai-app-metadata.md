# Publishing RFE Auth Service to OpenAI / ChatGPT Plugin & App Directory

To list your MCP server as a searchable OpenAI App / Plugin in ChatGPT:

## 1. Prerequisites
- **Public Domain**: Host `RFE.Auth.Netcore` on a publicly accessible domain with HTTPS (e.g., `https://auth.yourdomain.com`).
- **OpenAPI Schema**: Ensure `/swagger/v1/swagger.json` or `/scalar/v1` is publicly accessible.
- **OpenAI Verification Token**: Insert your OpenAI developer verification token into `ai-plugin.json`.

## 2. Registering OpenAI App in Developer Dashboard
1. Go to the [OpenAI Platform Dashboard](https://platform.openai.com/).
2. Navigate to **Apps & Integrations** (or **GPT Actions** / **Plugin Submission**).
3. Select **Add New Plugin / Custom Action**.
4. Enter your plugin domain (`https://auth.yourdomain.com`). OpenAI will automatically fetch `https://auth.yourdomain.com/.well-known/ai-plugin.json`.

## 3. Configuring OAuth 2.1 authentication
1. When prompted for Authentication Type, select **OAuth**.
2. Set Client ID and Client Secret (generate these in `RFE.Auth.Netcore` DB using OpenIddict application seeding or SQL insert for client name `openai-chatgpt`).
3. Set Authorization URL: `https://auth.yourdomain.com/connect/authorize`
4. Set Token URL: `https://auth.yourdomain.com/connect/token`
5. Copy OpenAI's Redirect URI (e.g. `https://chatgpt.com/aip/g-.../oauth/callback`) and ensure it is added to `RedirectUris` in `OpenIddictApplications`.

## 4. Submitting for Review
1. Provide Privacy Policy URL (`https://yourdomain.com/privacy`).
2. Provide Terms of Service URL (`https://yourdomain.com/terms`).
3. Upload high-res App Icon (PNG/SVG, 512x512).
4. Click **Submit for Review**. Once approved by OpenAI, the plugin will become searchable in ChatGPT's Plugin Directory / App Store.
