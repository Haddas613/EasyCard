import { ApplicationInsights } from '@microsoft/applicationinsights-web'

const appInsights = new ApplicationInsights({ config: {
    instrumentationKey: process.env.VUE_APP_APPLICATION_INSIGHTS_KEY
    /* ...Other Configuration Options... */
} });
appInsights.loadAppInsights();
appInsights.trackPageView();

export default appInsights;