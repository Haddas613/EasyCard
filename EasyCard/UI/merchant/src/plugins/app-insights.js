import { ApplicationInsights } from '@microsoft/applicationinsights-web'
import cfg from "../app.config";

const appInsights = new ApplicationInsights({ config: {
    instrumentationKey: cfg.VUE_APP_APPLICATION_INSIGHTS_KEY
    /* ...Other Configuration Options... */
} });
appInsights.loadAppInsights();
appInsights.trackPageView();

export default appInsights;