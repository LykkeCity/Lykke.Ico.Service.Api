export var AppEvent;
(function (AppEvent) {
    AppEvent["Toast"] = "toast";
})(AppEvent || (AppEvent = {}));
export var AppToastType;
(function (AppToastType) {
    AppToastType["Error"] = "error";
    AppToastType["Info"] = "info";
    AppToastType["Success"] = "success";
})(AppToastType || (AppToastType = {}));
// angularJS application module is used in every other file to register components/services
export const app = angular.module("admin", ["ngRoute", "ngMaterial", "ngMessages"]);
