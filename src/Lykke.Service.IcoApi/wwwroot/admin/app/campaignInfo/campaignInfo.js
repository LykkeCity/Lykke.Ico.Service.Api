import { app } from "../app.js";
class CampaignInfoController {
    constructor($element, $timeout) {
        this.$element = $element;
        this.$timeout = $timeout;
    }
    get info() {
        return this.shell && this.shell.campaignInfo;
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
    require: {
        shell: "^shell",
    }
});
