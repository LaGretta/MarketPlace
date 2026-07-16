/* ============================================================================
 *  MarketPlace — Frontend configuration
 *  --------------------------------------------------------------------------
 *  Switch between your local API and the deployed API by changing ONE line.
 *  No protocol trailing slash needed — endpoints are appended with a leading "/".
 * ==========================================================================*/

// 👉 Local development (default launch profile: http://localhost:5222)
const API_BASE = "http://localhost:5222";

// 👉 When deployed, comment the line above and use your public URL instead:
// const API_BASE = "https://your-marketplace-api.azurewebsites.net";

/* Exposed for the rest of the app. */
window.APP_CONFIG = { API_BASE };
