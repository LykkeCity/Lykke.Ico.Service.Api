import { app, AppEvent, AppToastType, AppToast, AppCommand, IAppRoute, SysEvent } from "../app.js";

export class ShellController implements ng.IComponentController {

    constructor(private $element: ng.IRootElementService, private $rootScope: ng.IRootScopeService,
        private $location: ng.ILocationService, private $mdSidenav: ng.material.ISidenavService, private $mdToast: ng.material.IToastService,
        private $route: ng.route.IRouteService, public appRoutes: IAppRoute[]) {

        $rootScope.$on(AppEvent.Toast, (e: ng.IAngularEvent, toast: AppToast) => {
            this.toast(toast);
        });

        $rootScope.$on(AppEvent.ReloadRoute, () => {
            this.$route.reload();
        });

        $rootScope.$on(SysEvent.RouteChangeSuccess, () => {
            this.customCommands = []; // clean custom toolbar
        });
    }

    $postLink() {
        // there is no :host class for angular 1.x.x,
        // so style component root element manually
        this.$element
            .addClass("layout-fill") // fill parent
            .addClass("layout-column"); // define self layout
    }

    title = "Lykke ICO Platform";

    customCommands: AppCommand[] = [];

    navigateTo(route: IAppRoute) {
        this.$location.url(`/${route.link}`);
        this.toggleSidenav();
    }

    toggleSidenav() {
        this.$mdSidenav('sidenav').toggle();
    }

    toast(toast: AppToast) {
        let model = this.$mdToast.simple().textContent(toast.message).position("top right").toastClass(`md-toast-${toast.type}`);

        if (toast.type == AppToastType.Error) {
            model = model.hideDelay(false).action("Close");
        }

        this.$mdToast.show(model);
    }

    appendCustomCommands(commands: AppCommand[]) {
        commands.forEach(command => {
            if (this.customCommands.indexOf(command) < 0) {
                this.customCommands.push(command);
            }
        });
    }

    deleteCustomCommands(commands: AppCommand[]) {
        commands.forEach(command => {
            this.customCommands.splice(this.customCommands.indexOf(command, 1));
        });
    }
}

app.component("shell", {
    controller: ShellController,
    templateUrl: "app/shell/shell.html",
});










