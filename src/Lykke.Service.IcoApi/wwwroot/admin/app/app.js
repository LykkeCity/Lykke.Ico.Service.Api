const routes = [
    { link: "campaign-info", icon: "info", name: "Info", template: "<campaign-info class=\"flex layout-column\"></campaign-info>" },
    { link: "campaign-settings", icon: "settings", name: "Settings", template: "<campaign-settings class=\"flex layout-column\"></campaign-settings>" },
    { link: "campaign-email-templates", icon: "email", name: "Email Templates", template: "<campaign-email-templates class=\"flex layout-column\"></campaign-email-templates>" }
];
class AppController {
    constructor($rootScope, $location, $route) {
        this.$rootScope = $rootScope;
        this.$location = $location;
        this.$route = $route;
        this.routes = routes;
        this.sidenav = document.getElementById("sidenav");
        $rootScope.$on("$routeChangeStart", (e, next, current) => {
        });
        $rootScope.$on("$routeChangeSuccess", (e, current, previous) => {
            this.updateActiveRoutes(current);
        });
    }
    updateActiveRoutes(current) {
        this.routes.forEach(route => {
            route.isActive = current.name == route.name;
        });
    }
    $onInit() {
        this.updateActiveRoutes(this.$route.current);
    }
    toggleSidenav() {
        this.sidenav.classList.toggle("expanded");
    }
}
export const app = angular.module("admin", ["ngRoute", "ngMaterial"]);
// config app routes
app.config(($routeProvider, $locationProvider) => {
    $locationProvider.html5Mode(true);
    $routeProvider.caseInsensitiveMatch = true;
    $routeProvider.when("/login", { template: "<login></login>" });
    $routeProvider.otherwise({
        redirectTo: `/${routes[0].link}`
    });
    routes.forEach(route => $routeProvider.when(`/${route.link}`, route));
});
// config AMD theme
app.config(($mdThemingProvider) => {
    $mdThemingProvider
        .theme("default")
        .primaryPalette("grey", { default: "200" })
        .accentPalette("grey", { default: "700" });
});
// define app component
app.component("app", {
    bindings: {},
    controller: AppController,
    controllerAs: "vm",
    templateUrl: "app/app.html",
});
