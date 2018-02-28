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
}

// angularJS application module is used in every other file to register components/services
export const app = angular.module("admin", ["ngRoute", "ngMaterial", "ngMessages"]);