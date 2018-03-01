import { app, AppToastType } from "../app.js";
class CampaignSettings {
}
class CampaignSettingsController {
    constructor($element, $http) {
        this.$element = $element;
        this.$http = $http;
        this.settingsUrl = "/api/admin/campaign/settings";
        this.customCommands = [{
                name: "Save",
                action: () => this.save()
            }];
    }
    $onInit() {
        this.$http
            .get(this.settingsUrl)
            .then(response => this.settings = response.data || new CampaignSettings());
        this.shell.appendCustomCommands(this.customCommands);
    }
    $onDestroy() {
        this.shell.deleteCustomCommands(this.customCommands);
    }
    $postLink() {
        // there is no :host class for angular 1.x.x,
        // so style component root element manually
        this.$element
            .addClass("layout-fill") // fill parent
            .addClass("layout-column") // define self layout
            .addClass("layout-align-start-center"); // center children horizontally
    }
    save() {
        if (this.settings) {
            this.$http
                .post(this.settingsUrl, this.settings)
                .then(_ => this.shell.toast({ message: "Changes saved", type: AppToastType.Success }));
        }
    }
}
app.component("campaignSettings", {
    controller: CampaignSettingsController,
    templateUrl: "app/campaignSettings/campaignSettings.html",
    require: {
        shell: "^shell",
    }
});
