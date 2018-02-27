import { app } from "../app.js";

class Credentials {
    username: string;
    password: string;
}

class AuthDialogController implements ng.IController{
    constructor(private $scope: ng.IScope, private authService: AuthService) {
    }

    credentials: Credentials = new Credentials();

    errors: any = {};

    login() {
        this.authService.login(this.credentials)
            .catch(response => {
                this.errors.forbidden = AuthService.isAuthForbidden(response);
            });
    }

    $onInit() {
        this.$scope.$watch(
            () => this.credentials,
            () => this.errors = {},
            true);
    }
}

export class AuthService {
    constructor(private $mdDialog: ng.material.IDialogService, private $q: ng.IQService, private $http: ng.IHttpService) {
    }

    static AuthToken: string;

    static AuthUrl = "/api/admin/login";

    static isAuthForbidden(response: ng.IHttpResponse<any>) {
        return response.config.url.indexOf(AuthService.AuthUrl) > -1 && response.status == 403;
    }

    authenticate(): ng.IPromise<any> {
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

    login(credentials: Credentials) {
        return this.$http
            .post<string>(AuthService.AuthUrl, credentials)
            .then(response => {
                this.$mdDialog.hide();
                AuthService.AuthToken = response.data;
            });
    }
}

app.service("authService", AuthService);