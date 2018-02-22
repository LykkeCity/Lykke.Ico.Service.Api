declare global {
    const angular: ng.IAngularStatic;
}

interface IAppRoute extends ng.route.IRoute {
    link: string;
    icon: string;
    isActive?: boolean;
}

const routes: IAppRoute[] = [
    { link: "campaign-info", icon: "information", name: "Info", template: "<campaign-info flex layout=\"column\"></campaign-info>" },
    { link: "campaign-settings", icon: "settings", name: "Settings", template: "<campaign-settings flex layout=\"column\"></campaign-settings>" },
    { link: "campaign-email-templates", icon: "email", name: "Email Templates", template: "<campaign-email-templates flex layout=\"column\"></campaign-email-templates>" },
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
    $routeProvider.otherwise({
        redirectTo: routes[0].link
    });
    routes.forEach(route => $routeProvider.when(`/${route.link}`, route));
})

// config AMD theme
app.config(($mdThemingProvider: ng.material.IThemingProvider) => {
    $mdThemingProvider.theme("default")
        .primaryPalette("grey", { default: "200" })
        .accentPalette("deep-purple");
});

// define app component
app.component("app", {
    bindings: {},
    controller: AppController,
    controllerAs: "vm",
    templateUrl: "app/app.html",
});