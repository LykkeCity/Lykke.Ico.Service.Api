// to make monaco-editor types useful without bundler
/// <reference path="../../../node_modules/monaco-editor/monaco.d.ts" />

// to make angularjs types useful without bundler
declare global {
    const angular: ng.IAngularStatic;
}

export enum SysEvent {
    RouteChangeSuccess = "$routeChangeSuccess",
}

export enum AppEvent {
    ReloadRoute = "reloadRoute",
    Toast = "toast"
}

export enum AppToastType {
    Error = "error",
    Info = "info",
    Success = "success"
}

export class AppToast {
    message: string;
    type: AppToastType;
}

export interface IAppRoute extends ng.route.IRoute {
    link: string;
    icon: string;
    isActive?: boolean;
}

export class AppCommand {
    name: string;
    action: Function;
    isDisabled?: Function;
}

export class AppColorsService {
    constructor(private $mdTheming: ng.material.IThemingService) {
    }

    sidepanelColors() {
        const hue = this.$mdTheming.THEMES.default.isDark ? "800" : "100";
        return {
            background: `default-background-${hue}`
        };
    }

    selectionColors(isSelected: boolean) {
        if (!isSelected) {
            return this.sidepanelColors();
        }
        const hue = this.$mdTheming.THEMES.default.isDark ? "700" : "300";
        return {
            background: `default-background-${hue}`
        };
    }
}

const appRoutes: IAppRoute[] = [
    { link: "campaign-info", icon: "info", name: "Info", template: "<campaign-info></campaign-info>" },
    { link: "campaign-email-templates", icon: "email", name: "Email Templates", template: "<campaign-email-templates></campaign-email-templates>" },
    { link: "campaign-settings", icon: "settings", name: "Settings", template: "<campaign-settings></campaign-settings>" }
];

// angularJS application module is used in
// every other file to register components
export const app = angular.module("admin", ["ngRoute", "ngMaterial", "ngMessages"])
    .constant("appRoutes", appRoutes)
    .service("appColors", AppColorsService);
