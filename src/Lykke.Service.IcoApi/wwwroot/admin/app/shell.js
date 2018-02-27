import { app, AppEvent, AppToastType } from "./app.js";
export class ShellController {
    constructor($element, $rootScope, $location, $mdSidenav, $mdToast, appRoutes) {
        this.$element = $element;
        this.$rootScope = $rootScope;
        this.$location = $location;
        this.$mdSidenav = $mdSidenav;
        this.$mdToast = $mdToast;
        this.appRoutes = appRoutes;
        this.title = "Lykke ICO Platform";
        this.customCommands = [];
        $rootScope.$on(AppEvent.Toast, (e, toast) => {
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
    navigateTo(route) {
        this.$location.url(`/${route.link}`);
        this.toggleSidenav();
    }
    toggleSidenav() {
        this.$mdSidenav('sidenav').toggle();
    }
    toast(toast) {
        let model = this.$mdToast.simple().textContent(toast.message).position("bottom right").toastClass(`md-toast-${toast.type}`);
        if (toast.type == AppToastType.Error) {
            model = model.hideDelay(false).action("Close");
        }
        this.$mdToast.show(model);
    }
    appendCustomCommands(commands) {
        commands.forEach(command => {
            if (this.customCommands.find(c => c.name == command.name) == null) {
                this.customCommands.push(command);
            }
        });
    }
    deleteCustomCommands(commands) {
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
