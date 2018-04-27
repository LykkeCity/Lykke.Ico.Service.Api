import { app } from "../app.js";
export class CampaignInfo {
}
class CampaignInfoController {
    constructor($element, $http) {
        this.$element = $element;
        this.$http = $http;
        this.infoUrl = "/api/admin/campaign/info";
    }
    $onInit() {
        this.$http
            .get(this.infoUrl)
            .then(response => {
            this.info = response.data || new CampaignInfo();
        });
    }
    $postLink() {
        // there is no :host class for angular 1.x.x,
        // so style component root element manually
        this.$element
            .addClass("layout-fill") // fill parent
            .addClass("layout-column") // define self layout
            .addClass("layout-align-start-center"); // center children horizontally
    }
}
app.component("campaignInfo", {
    controller: CampaignInfoController,
    templateUrl: "app/campaignInfo/campaignInfo.html",
});
