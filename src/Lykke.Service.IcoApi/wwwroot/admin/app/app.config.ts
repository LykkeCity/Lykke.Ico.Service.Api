import { app, IAppRoute, AppEvent, AppToastType } from "./app.js";
import { AuthService, AuthUtils  } from "./auth/auth.js";

const appRoutes: IAppRoute[] = [
    { link: "campaign-info", icon: "info", name: "Info", template: "<campaign-info></campaign-info>" },
    { link: "campaign-settings", icon: "settings", name: "Settings", template: "<campaign-settings></campaign-settings>" },
    { link: "campaign-email-templates", icon: "email", name: "Email Templates", template: "<campaign-email-templates></campaign-email-templates>" }
];

// define app routes
app.constant("appRoutes", appRoutes);

// config app routes
app.config(($routeProvider: ng.route.IRouteProvider, $locationProvider: ng.ILocationProvider, appRoutes: IAppRoute[]) => {
    $locationProvider.html5Mode(true);
    $routeProvider.caseInsensitiveMatch = true;
    $routeProvider.otherwise({
        redirectTo: `/${appRoutes[0].link}`
    });
    appRoutes.forEach(route => {
        route.resolve = {
            auth: (authService: AuthService) => authService.authenticate()
        };
        $routeProvider.when(`/${route.link}`, route);
    });
});

// config interceptors
app.config(($httpProvider: ng.IHttpProvider) => {
    $httpProvider.interceptors.push(($q: ng.IQService, $rootScope: ng.IRootScopeService, authUtils: AuthUtils) => {
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
                } else if (!authUtils.isLoginForbidden(response)) {
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

// config Material Design theme
app.config(($mdThemingProvider: ng.material.IThemingProvider) => {
    $mdThemingProvider
        .theme("default")
    //.dark();
});
