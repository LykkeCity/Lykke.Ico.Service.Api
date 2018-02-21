import { app } from "../app.js";
class CampaignSettingsController {
    constructor($http) {
        this.$http = $http;
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
