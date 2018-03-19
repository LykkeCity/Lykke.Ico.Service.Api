import { app, IAppRoute, AppEvent, AppToastType } from "./app.js";
import { AuthService, AuthUtils  } from "./auth/auth.js";

const regexIso8601 = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})/;

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

        if (typeof value === "string" && regexIso8601.test(value)) {
            var milliseconds = Date.parse(value)
            if (!isNaN(milliseconds)) {
                input[key] = new Date(milliseconds);
            }
        } else if (typeof value === "object") {
            input[key] = transformDateStringsToDates(value);
        }
    }

    return input;
}

function interceptAuthTokensAndHttpErrors($q: ng.IQService, $rootScope: ng.IRootScopeService, authUtils: AuthUtils) {
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
}

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

// config interceptors and transformers
app.config(($httpProvider: ng.IHttpProvider) => {
    let transformers = angular.isArray($httpProvider.defaults.transformResponse) ? $httpProvider.defaults.transformResponse : [$httpProvider.defaults.transformResponse];
    $httpProvider.defaults.transformResponse = transformers.concat(transformDateStringsToDates);
    $httpProvider.interceptors.push(interceptAuthTokensAndHttpErrors);
});

// config Material Design and monaco-editor
app.config(($mdThemingProvider: ng.material.IThemingProvider, $mdAriaProvider: ng.material.IAriaProvider, $mdColorPalette: ng.material.IColorPalette) => {
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
