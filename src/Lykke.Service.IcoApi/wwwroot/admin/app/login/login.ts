import { app } from "../app.js";

class LoginController implements ng.IComponentController {
    constructor(private $mdDialog: ng.material.IDialogService) {
    }

    $onInit() {
        //this.$mdDialog.show()
    }
}

app.component("login", {
    bindings: {},
    controller: LoginController,
    controllerAs: "vm",
    templateUrl: "app/login/login.html",
});