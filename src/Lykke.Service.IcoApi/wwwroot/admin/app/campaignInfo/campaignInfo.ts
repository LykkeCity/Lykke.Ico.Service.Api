import { app } from "../app.js";

class CampaignInfoController implements ng.IComponentController {
    constructor(private $http: ng.IHttpService) {
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
