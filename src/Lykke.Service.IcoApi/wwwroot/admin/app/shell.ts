import { app, AppEvent, AppToastType, IAppToast, IAppCommand, IAppRoute } from "./app.js";

export class ShellController implements ng.IComponentController {

    constructor(private $element: ng.IRootElementService, private $rootScope: ng.IRootScopeService,
        private $location: ng.ILocationService, private $mdSidenav: ng.material.ISidenavService, private $mdToast: ng.material.IToastService,
        readonly appRoutes: IAppRoute[]) {

        $rootScope.$on(AppEvent.Toast, (e: ng.IAngularEvent, toast: IAppToast) => {
            this.toast(toast);
        });

        $rootScope.$on("$routeChangeSuccess", () => {
            this.customCommands = []; // clean children's toolbar
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

    customCommands: IAppCommand[] = [];

    navigateTo(route: IAppRoute) {
        this.$location.url(`/${route.link}`);
        this.toggleSidenav();
    }

    toggleSidenav() {
        this.$mdSidenav('sidenav').toggle();
    }

    toast(toast: IAppToast) {
        let model = this.$mdToast.simple().textContent(toast.message).position("bottom right").toastClass(`md-toast-${toast.type}`);

        if (toast.type == AppToastType.Error) {
            model = model.hideDelay(false).action("Close");
        }

        this.$mdToast.show(model);
    }

    appendCustomCommands(commands: IAppCommand[]) {
        commands.forEach(command => {
            if (this.customCommands.find(c => c.name == command.name) == null) {
                this.customCommands.push(command);
            }
        });
    }

    deleteCustomCommands(commands: IAppCommand[]) {
        commands.forEach(command => {
            let index = this.customCommands.findIndex(c => c.name == command.name);
            if (index >= 0) {
                this.customCommands.splice(index, 1);
            }
        });
    }
}

app.component("shell", {
    controller: ShellController,
    templateUrl: "app/shell.html",
});










