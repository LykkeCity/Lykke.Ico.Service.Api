// to make monaco-editor types useful without bundler
/// <reference path="../../../node_modules/monaco-editor/monaco.d.ts" />
// angularJS application module is used in every other file to register components/services
export const app = angular.module("admin", ["ngRoute", "ngMaterial", "ngMessages"]);
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
