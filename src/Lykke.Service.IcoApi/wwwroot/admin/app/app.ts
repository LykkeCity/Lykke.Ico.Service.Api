declare global {
    const angular: ng.IAngularStatic;
}

function routes($routeProvider: ng.route.IRouteProvider, $locationProvider: ng.ILocationProvider) {
    $routeProvider.caseInsensitiveMatch = true;
    $routeProvider
        .when("/campaign-email-templates", { template: "<campaign-email-templates></campaign-email-templates>" })
        .when("/campaign-info", { template: "<campaign-info></campaign-info>" })
        .when("/campaign-settings", { template: "<campaign-settings></campaign-settings>" })
        .otherwise({ redirectTo: "/campaign-settings" })
    $locationProvider.html5Mode(true);
}

export const app = angular
    .module("admin", ["ngRoute", "ngMaterial"])
    .config(routes);