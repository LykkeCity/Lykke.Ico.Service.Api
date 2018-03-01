import { app } from "../app.js";
export class Credentials {
}
class AuthDialogController {
    constructor($scope, authService, authUtils) {
        this.$scope = $scope;
        this.authService = authService;
        this.authUtils = authUtils;
        this.credentials = new Credentials();
        this.errors = {};
    }
    $onInit() {
        this.$scope.$watch(() => this.credentials, () => this.errors = {}, true);
    }
    login() {
        this.errors = {};
        this.authService.login(this.credentials)
            .catch(response => {
            this.errors.forbidden = this.authUtils.isLoginForbidden(response);
        });
    }
}
/**
 * Keeps auth data and provides auth utils
 * without dependency on any other service.
 */
export class AuthUtils {
    constructor() {
        this.authTokenStorageKey = "authToken";
        this.authUrl = "/api/admin/login";
    }
    get authToken() {
        return sessionStorage.getItem(this.authTokenStorageKey);
    }
    set authToken(value) {
        sessionStorage.setItem(this.authTokenStorageKey, value);
    }
    isAuthorized(response) {
        if (response.status == 401) {
            sessionStorage.removeItem(this.authTokenStorageKey);
            return false;
        }
        return true;
    }
    isLoginForbidden(response) {
        return response.config.url.startsWith(this.authUrl) && response.status == 403;
    }
}
export class AuthService {
    constructor($mdDialog, $q, $http, $timeout, authUtils) {
        this.$mdDialog = $mdDialog;
        this.$q = $q;
        this.$http = $http;
        this.$timeout = $timeout;
        this.authUtils = authUtils;
    }
    authenticate() {
        if (this.authUtils.authToken) {
            return this.$q.resolve();
        }
        // use $timeout for login dialog because it's shown on very app start, when layout 
        // can be not ready, so we give time for md-* directives to prepare layout
        return this.$timeout(() => this.$mdDialog.show({
            bindToController: true,
            controller: AuthDialogController,
            controllerAs: "$ctrl",
            templateUrl: "app/auth/auth.html",
            parent: angular.element(document.body),
            clickOutsideToClose: false,
            escapeToClose: false
        }));
    }
    login(credentials) {
        return this.$http
            .post(this.authUtils.authUrl, credentials)
            .then(response => {
            this.authUtils.authToken = response.data;
            this.$mdDialog.hide();
        });
    }
}
app.constant("authUtils", new AuthUtils());
app.service("authService", AuthService);
