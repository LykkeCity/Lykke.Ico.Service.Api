import { app, AppEvent, AppToastType } from "./app.js";
const regexIso8601 = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*)?)Z$/;
function transformDateStringsToDates(input) {
    if (typeof input !== "object") {
        return input;
    }
    for (var key in input) {
        if (!input.hasOwnProperty(key)) {
            continue;
        }
        var value = input[key];
        var match;
        if (typeof value === "string" && (match = value.match(regexIso8601))) {
            var milliseconds = Date.parse(match[0]);
            if (!isNaN(milliseconds)) {
                input[key] = new Date(milliseconds);
            }
        }
        else if (typeof value === "object") {
            input[key] = transformDateStringsToDates(value);
        }
    }
    return input;
}
function interceptAuthTokensAndHttpErrors($q, $rootScope, authUtils) {
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
}
const appRoutes = [
    { link: "campaign-info", icon: "info", name: "Info", template: "<campaign-info></campaign-info>" },
    { link: "campaign-email-templates", icon: "email", name: "Email Templates", template: "<campaign-email-templates></campaign-email-templates>" },
    { link: "campaign-settings", icon: "settings", name: "Settings", template: "<campaign-settings></campaign-settings>" }
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
// config interceptors and transformers
app.config(($httpProvider) => {
    let transformers = angular.isArray($httpProvider.defaults.transformResponse) ? $httpProvider.defaults.transformResponse : [$httpProvider.defaults.transformResponse];
    $httpProvider.defaults.transformResponse = transformers.concat(transformDateStringsToDates);
    $httpProvider.interceptors.push(interceptAuthTokensAndHttpErrors);
});
// config Material Design and monaco-editor
app.config(($mdThemingProvider, $mdAriaProvider, $mdColorPalette) => {
    $mdAriaProvider.disableWarnings();
    let isDark = false;
    $mdThemingProvider.theme("default").dark(isDark);
    monaco.editor.defineTheme('material', {
        base: isDark ? "vs-dark" : "vs",
        inherit: true,
        rules: [],
        colors: {
            'editor.background': isDark ? $mdColorPalette.grey["A400"] : $mdColorPalette.grey["50"]
        }
    });
    monaco.editor.setTheme("material");
});
