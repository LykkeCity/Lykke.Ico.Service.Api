declare global {
    const angular: ng.IAngularStatic;
}

interface IAppRoute extends ng.route.IRoute {
    link: string;
    icon: string;
    isActive?: boolean;
}

const routes: IAppRoute[] = [
    { link: "campaign-info", icon: "info", name: "Info", template: "<campaign-info class=\"flex layout-column\"></campaign-info>" },
    { link: "campaign-settings", icon: "settings", name: "Settings", template: "<campaign-settings class=\"flex layout-column\"></campaign-settings>" },
    { link: "campaign-email-templates", icon: "email", name: "Email Templates", template: "<campaign-email-templates class=\"flex layout-column\"></campaign-email-templates>" }
];

class AppController implements ng.IComponentController {

    constructor(private $rootScope: ng.IRootScopeService, private $location: ng.ILocationService, private $route: ng.route.IRouteService) {
        this.sidenav = document.getElementById("sidenav");

        $rootScope.$on("$routeChangeStart", (e: ng.IAngularEvent, next: ng.route.IRoute, current: ng.route.IRoute) => {

        });

        $rootScope.$on("$routeChangeSuccess", (e: ng.IAngularEvent, current: ng.route.IRoute, previous: ng.route.IRoute) => {
            this.updateActiveRoutes(current);
        });
    }

    private sidenav: HTMLElement;

    private user;

    private updateActiveRoutes(current: ng.route.IRoute) {
        this.routes.forEach(route => {
            route.isActive = current.name == route.name;
        });
    }

    routes: IAppRoute[] = routes;

    $onInit() {
        this.updateActiveRoutes(this.$route.current);
    }

    toggleSidenav() {
        this.sidenav.classList.toggle("expanded");
    }
}

export const app = angular.module("admin", ["ngRoute", "ngMaterial"]);

// config app routes
app.config(($routeProvider: ng.route.IRouteProvider, $locationProvider: ng.ILocationProvider) => {
    $locationProvider.html5Mode(true);
    $routeProvider.caseInsensitiveMatch = true;
    $routeProvider.when("/login", { template: "<login></login>" });
    $routeProvider.otherwise({
        redirectTo: `/${routes[0].link}`
    });
    routes.forEach(route => $routeProvider.when(`/${route.link}`, route));
})

// config AMD theme
app.config(($mdThemingProvider: ng.material.IThemingProvider) => {
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