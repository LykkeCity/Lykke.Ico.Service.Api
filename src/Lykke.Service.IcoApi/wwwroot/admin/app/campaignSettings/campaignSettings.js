import { app, AppToastType } from "../app.js";
import { CampaignSettingsHistoryController } from "./campaignSettingsHistory.js";
class CampaignSettings {
    constructor() {
        this.preSaleStartDateTimeUtc = new Date();
        this.preSaleEndDateTimeUtc = new Date();
        this.crowdSaleStartDateTimeUtc = new Date();
        this.crowdSaleEndDateTimeUtc = new Date();
        this.commonSettings = new CommonCampaignSettings();
    }
}
class CommonCampaignSettings {
}
class SmtpSettings {
}
class CampaignSettingsController {
    constructor($element, $http, $mdDialog) {
        this.$element = $element;
        this.$http = $http;
        this.$mdDialog = $mdDialog;
        this.transactionQueueSasGenerationUrl = "/api/admin/transactions/sas";
        this.settingsUrl = "/api/admin/campaign/settings";
        this.customCommands = [
            { name: "Save", action: () => this.save() },
            { name: "History", action: () => this.showHistory() }
        ];
    }
    extractDateFromSas() {
        try {
            this.transactionQueueSasExpiryTime =
                new Date(decodeURIComponent(/se=(.*)&/g.exec(this.settings.commonSettings.transactionQueueSasUrl)[1]));
        }
        catch (_a) {
            this.transactionQueueSasExpiryTime = null;
        }
    }
    $onInit() {
        this.$http
            .get(this.settingsUrl)
            .then(response => {
            this.settings = response.data || new CampaignSettings();
            this.settingsOriginal = angular.copy(this.settings);
            this.extractDateFromSas();
        });
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
        if (!this.settings) {
            return;
        }
        if (angular.equals(this.settings, this.settingsOriginal)) {
            this.shell.toast({ message: "There are no changes to save", type: AppToastType.Info });
            return;
        }
        if (this.settings) {
            this.$http
                .post(this.settingsUrl, this.settings)
                .then(_ => {
                this.shell.toast({ message: "Changes saved", type: AppToastType.Success });
                this.settingsOriginal = angular.copy(this.settings);
            });
        }
    }
    showHistory() {
        this.$mdDialog.show({
            bindToController: true,
            controller: CampaignSettingsHistoryController,
            controllerAs: "$ctrl",
            templateUrl: "app/campaignSettings/campaignSettingsHistory.html",
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            resolve: {
                history: () => this.$http.get(`${this.settingsUrl}/history`)
                    .then(res => res.data)
            }
        });
    }
    generateTransactionQueueSasUrl() {
        this.$http
            .post(this.transactionQueueSasGenerationUrl, { expiryTime: this.transactionQueueSasExpiryTime })
            .then(resp => {
            this.settings.commonSettings.transactionQueueSasUrl = resp.data;
            this.extractDateFromSas();
        });
    }
    overrideSmtp() {
        this.settings.commonSettings.smtp = new SmtpSettings();
    }
}
app.component("campaignSettings", {
    controller: CampaignSettingsController,
    templateUrl: "app/campaignSettings/campaignSettings.html",
    require: {
        shell: "^shell",
    }
});
