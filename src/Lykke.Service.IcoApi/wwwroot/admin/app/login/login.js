import { app } from "../app.js";
class LoginController {
    constructor($mdDialog) {
        this.$mdDialog = $mdDialog;
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
