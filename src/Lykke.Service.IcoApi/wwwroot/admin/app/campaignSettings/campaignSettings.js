import { app } from "../app.js";
class CampaignSettingsController {
    constructor($element, $http) {
        this.$element = $element;
        this.$http = $http;
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
