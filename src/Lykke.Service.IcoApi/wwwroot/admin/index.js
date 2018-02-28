// in most cases components are independent ES6 modules (not imported by any other module)
// so we import all of them explicitly at the application entry point
import "./app/app.js";
import "./app/app.config.js";
import "./app/auth/auth.js";
import "./app/campaignEmailTemplates/campaignEmailTemplates.js";
import "./app/campaignInfo/campaignInfo.js";
import "./app/campaignSettings/campaignSettings.js";
import "./app/shell/shell.js";

