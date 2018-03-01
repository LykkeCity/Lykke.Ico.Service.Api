import { app, AppEvent, AppToastType } from "./app.js";
const appRoutes = [
    { link: "campaign-info", icon: "info", name: "Info", template: "<campaign-info></campaign-info>" },
    { link: "campaign-email-templates", icon: "email", name: "Email Templates", template: "<campaign-email-templates></campaign-email-templates>" },
    { link: "campaign-settings", icon: "settings", name: "Settings", template: "<campaign-settings></campaign-settings>" },
];
// define app routes
app.constant("appRoutes", appRoutes);
// config app routes
app.config(($routeProvider, $locationProvider, appRoutes) => {
    $locationProvider.html5Mode(true);
    $routeProvider.caseInsensitiveMatch = true;
    $routeProvider.otherwise({
        redirectTo: `/${appRoutes[0].link}`
    });
    appRoutes.forEach(route => {
        route.resolve = {
            auth: (authService) => authService.authenticate()
        };
        $routeProvider.when(`/${route.link}`, route);
    });
});
// config interceptors
app.config(($httpProvider) => {
    $httpProvider.interceptors.push(($q, $rootScope, authUtils) => {
        return {
            request: request => {
                if (!!authUtils.authToken) {
                    request.headers["adminAuthToken"] = authUtils.authToken;
                }
                return request;
            },
            responseError: response => {
                if (!authUtils.isAuthorized(response)) {
                    $rootScope.$emit(AppEvent.ReloadRoute);
                }
                else if (!authUtils.isLoginForbidden(response)) {
                    $rootScope.$emit(AppEvent.Toast, {
                        message: (response.data && response.data.Message) || response.statusText || "Technical problem",
                        type: AppToastType.Error
                    });
                }
                return $q.reject(response);
            }
        };
    });
});
// config Material Design
app.config(($mdThemingProvider, $mdAriaProvider) => {
    $mdAriaProvider.disableWarnings();
    // uncomment to turn off the light:
    //$mdThemingProvider
    //    .theme("default")
    //    .dark();    
});
