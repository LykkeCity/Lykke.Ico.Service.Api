import { app, AppEvent, AppToastType, SysEvent } from "../app.js";
export class ShellController {
    constructor($element, $rootScope, $location, $mdSidenav, $mdToast, $route, appRoutes) {
        this.$element = $element;
        this.$rootScope = $rootScope;
        this.$location = $location;
        this.$mdSidenav = $mdSidenav;
        this.$mdToast = $mdToast;
        this.$route = $route;
        this.appRoutes = appRoutes;
        this.title = "Lykke ICO Platform";
        this.customCommands = [];
        $rootScope.$on(AppEvent.Toast, (e, toast) => {
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
    navigateTo(route) {
        this.$location.url(`/${route.link}`);
        this.toggleSidenav();
    }
    toggleSidenav() {
        this.$mdSidenav('sidenav').toggle();
    }
    toast(toast) {
        let model = this.$mdToast.simple().textContent(toast.message).position("top right").toastClass(`md-toast-${toast.type}`);
        if (toast.type == AppToastType.Error) {
            model = model.hideDelay(false).action("Close");
        }
        this.$mdToast.show(model);
    }
    appendCustomCommands(commands) {
        commands.forEach(command => {
            if (this.customCommands.indexOf(command) < 0) {
                this.customCommands.push(command);
            }
        });
    }
    deleteCustomCommands(commands) {
        commands.forEach(command => {
            this.customCommands.splice(this.customCommands.indexOf(command, 1));
        });
    }
}
app.component("shell", {
    controller: ShellController,
    templateUrl: "app/shell/shell.html",
});
