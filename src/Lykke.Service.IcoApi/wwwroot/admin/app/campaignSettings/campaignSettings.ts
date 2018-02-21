import { app } from "../app.js";

class CampaignSettingsController implements ng.IComponentController {
    constructor(private $http: ng.IHttpService) {
    }

    $onInit() {
    }
}

app.component("campaignSettings", {
    bindings: {},
    controller: CampaignSettingsController,
    controllerAs: "vm",
    templateUrl: "app/campaignSettings/campaignSettings.html",
});