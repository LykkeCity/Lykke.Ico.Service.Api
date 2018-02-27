// to make angularjs types useful without bundler
declare global {
    const angular: ng.IAngularStatic;
}

export enum AppEvent {
    Toast = "toast"
}

export enum AppToastType {
    Error = "error",
    Info = "info",
    Success = "success"
}

export interface IAppToast {
    message: string;
    type: AppToastType;
}

export interface IAppRoute extends ng.route.IRoute {
    link: string;
    icon: string;
    isActive?: boolean;
}

export interface IAppCommand {
    name: string;
    action: Function;
}

// angularJS application module is used in every other file to register components/services
export const app = angular.module("admin", ["ngRoute", "ngMaterial", "ngMessages"]);