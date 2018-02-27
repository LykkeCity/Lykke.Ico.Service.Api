import { app } from "../app.js";

class CampaignSettingsController implements ng.IComponentController {
    constructor(private $element: ng.IRootElementService, private $http: ng.IHttpService) {
    }

    $onInit() {
    }

    $postLink() {
        // there is no :host class for angular 1.x.x,
        // so style component root element manually
        this.$element
            .addClass("layout-fill") // fill parent
            .addClass("layout-column"); // define self layout
    }
}

app.component("campaignSettings", {
    controller: CampaignSettingsController,
    templateUrl: "app/campaignSettings/campaignSettings.html"
});