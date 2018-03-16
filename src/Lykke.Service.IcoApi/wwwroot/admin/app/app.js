// to make monaco-editor types useful without bundler
/// <reference path="../../../node_modules/monaco-editor/monaco.d.ts" />
export var SysEvent;
(function (SysEvent) {
    SysEvent["RouteChangeSuccess"] = "$routeChangeSuccess";
})(SysEvent || (SysEvent = {}));
export var AppEvent;
(function (AppEvent) {
    AppEvent["ReloadRoute"] = "reloadRoute";
    AppEvent["Toast"] = "toast";
})(AppEvent || (AppEvent = {}));
export var AppToastType;
(function (AppToastType) {
    AppToastType["Error"] = "error";
    AppToastType["Info"] = "info";
    AppToastType["Success"] = "success";
})(AppToastType || (AppToastType = {}));
export class AppToast {
}
export class AppCommand {
}
export class AppColorsService {
    constructor($mdTheming) {
        this.$mdTheming = $mdTheming;
    }
    sidepanelColors() {
        const hue = this.$mdTheming.THEMES.default.isDark ? "800" : "100";
        return {
            background: `default-background-${hue}`
        };
    }
    selectionColors(isSelected) {
        if (!isSelected) {
            return this.sidepanelColors();
        }
        const hue = this.$mdTheming.THEMES.default.isDark ? "700" : "300";
        return {
            background: `default-background-${hue}`
        };
    }
}
const appRoutes = [
    { link: "campaign-info", icon: "info", name: "Info", template: "<campaign-info></campaign-info>" },
    { link: "campaign-email-templates", icon: "email", name: "Email Templates", template: "<campaign-email-templates></campaign-email-templates>" },
    { link: "campaign-settings", icon: "settings", name: "Settings", template: "<campaign-settings></campaign-settings>" }
];
// angularJS application module is used in
// every other file to register components
export const app = angular.module("admin", ["ngRoute", "ngMaterial", "ngMessages"])
    .constant("appRoutes", appRoutes)
    .service("appColors", AppColorsService);
