import { app } from "../app.js";
class CampaignInfoController {
    constructor($element) {
        this.$element = $element;
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
app.component("campaignInfo", {
    controller: CampaignInfoController,
    templateUrl: "app/campaignInfo/campaignInfo.html",
});
