import { app } from "../app.js";
class Credentials {
}
class AuthDialogController {
    constructor($scope, authService) {
        this.$scope = $scope;
        this.authService = authService;
        this.credentials = new Credentials();
        this.errors = {};
    }
    login() {
        this.authService.login(this.credentials)
            .catch(response => {
            this.errors.forbidden = AuthService.isAuthForbidden(response);
        });
    }
    $onInit() {
        this.$scope.$watch(() => this.credentials, () => this.errors = {}, true);
    }
}
export class AuthService {
    constructor($mdDialog, $q, $http) {
        this.$mdDialog = $mdDialog;
        this.$q = $q;
        this.$http = $http;
    }
    static isAuthForbidden(response) {
        return response.config.url.indexOf(AuthService.AuthUrl) > -1 && response.status == 403;
    }
    authenticate() {
        if (!!AuthService.AuthToken) {
            return this.$q.resolve();
        }
        return this.$mdDialog
            .show({
            bindToController: true,
            controller: AuthDialogController,
            controllerAs: "$ctrl",
            templateUrl: "app/auth/auth.html",
            parent: angular.element(document.body),
            clickOutsideToClose: false,
            escapeToClose: false
        })
            .then(value => {
            AuthService.AuthToken = value;
        });
    }
    login(credentials) {
        return this.$http
            .post(AuthService.AuthUrl, credentials)
            .then(response => {
            this.$mdDialog.hide();
            AuthService.AuthToken = response.data;
        });
    }
}
AuthService.AuthUrl = "/api/admin/login";
app.service("authService", AuthService);
