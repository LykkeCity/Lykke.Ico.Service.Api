import { app } from "../app.js";
class CampaignInfoController {
    constructor($http) {
        this.$http = $http;
    }
    $onInit() {
    }
    loadTemplates() {
    }
}
app.component("campaignInfo", {
    bindings: {},
    controller: CampaignInfoController,
    controllerAs: "vm",
    templateUrl: "app/campaignInfo/campaignInfo.html",
});
