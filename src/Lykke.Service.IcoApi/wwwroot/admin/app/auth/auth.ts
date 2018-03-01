import { app } from "../app.js";

export class Credentials {
    username: string;
    password: string;
}

class AuthDialogController implements ng.IController{
    constructor(private $scope: ng.IScope, private authService: AuthService, private authUtils: AuthUtils) {
    }

    credentials: Credentials = new Credentials();

    errors: any = {};

    $onInit() {
        this.$scope.$watch(
            () => this.credentials,
            () => this.errors = {},
            true);
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

    private authTokenStorageKey = "authToken";

    get authToken(): string {
        return sessionStorage.getItem(this.authTokenStorageKey);
    }
    set authToken(value: string) {
        sessionStorage.setItem(this.authTokenStorageKey, value);
    }

    authUrl = "/api/admin/login";

    isAuthorized(response: ng.IHttpResponse<any>) {
        if (response.status == 401) {
            sessionStorage.removeItem(this.authTokenStorageKey);
            return false;
        }
        return true;
    }

    isLoginForbidden(response: ng.IHttpResponse<any>) {
        return response.config.url.startsWith(this.authUrl) && response.status == 403;
    }
}

export class AuthService {
    constructor(private $mdDialog: ng.material.IDialogService, private $q: ng.IQService, private $http: ng.IHttpService, private $timeout: ng.ITimeoutService, private authUtils: AuthUtils) {
    }

    authenticate(): ng.IPromise<void> {
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

    login(credentials: Credentials): ng.IPromise<void> {
        return this.$http
            .post<string>(this.authUtils.authUrl, credentials)
            .then(response => {
                this.authUtils.authToken = response.data;
                this.$mdDialog.hide();
            });
    }
}

app.constant("authUtils", new AuthUtils());

app.service("authService", AuthService);