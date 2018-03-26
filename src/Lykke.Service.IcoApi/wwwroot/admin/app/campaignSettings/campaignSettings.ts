import { app, AppCommand, AppToast, AppToastType } from "../app.js";
import { ShellController } from "../shell/shell.js";
import { CampaignSettingsHistoryController, CampaignSettingsHistoryItem } from "./campaignSettingsHistory.js";

class CampaignSettings {
    preSaleStartDateTimeUtc: Date = new Date();
    preSaleEndDateTimeUtc: Date = new Date();
    preSaleTokenAmount: number;
    preSaleTokenPriceUsd: number;

    crowdSaleStartDateTimeUtc: Date = new Date();
    crowdSaleEndDateTimeUtc: Date = new Date();
    crowdSale1stTierTokenPriceUsd: number;
    crowdSale1stTierTokenAmount: number;
    crowdSale2ndTierTokenPriceUsd: number;
    crowdSale2ndTierTokenAmount: number;
    crowdSale3rdTierTokenPriceUsd: number;
    crowdSale3rdTierTokenAmount: number;

    rowndDownTokenDecimals: number;
    minInvestAmountUsd: number;
    enableFrontEnd: boolean;

    kycEnableRequestSending: boolean;
    kycCampaignId: string;
    kycLinkTemplate: string;
    kycServiceEncriptionKey: string;
    kycServiceEncriptionIv: string;

    captchaEnable: boolean;
    captchaSecret: string;

    commonSettings: CommonCampaignSettings = new CommonCampaignSettings();
}

class CommonCampaignSettings {
    transactionQueueSasUrl: string;
    smtp: SmtpSettings;
}

class SmtpSettings {
    host: string;
    port: number;
    localDomain: string;
    login: string;
    password: string;
    displayName: string
    from: string;
}

class CampaignSettingsController implements ng.IComponentController {

    private transactionQueueSasGenerationUrl = "/api/admin/transactions/sas";
    private settingsUrl = "/api/admin/campaign/settings";
    private shell: ShellController;
    private settingsOriginal: CampaignSettings;
    private customCommands: AppCommand[] = [
        { name: "Save", action: () => this.save() },
        { name: "History", action: () => this.showHistory() }
    ];

    private extractDateFromSas() {
        try {
            this.transactionQueueSasExpiryTime =
                new Date(decodeURIComponent(/se=(.*)&/g.exec(this.settings.commonSettings.transactionQueueSasUrl)[1]));
        } catch {
            this.transactionQueueSasExpiryTime = null;
        }
    }

    constructor(private $element: ng.IRootElementService, private $http: ng.IHttpService, private $mdDialog: ng.material.IDialogService) {
    }

    transactionQueueSasExpiryTime: Date;

    settings: CampaignSettings;

    $onInit() {
        this.$http
            .get<CampaignSettings>(this.settingsUrl)
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
            .addClass("layout-align-start-center") // center children horizontally
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
                history: () =>
                    this.$http.get<CampaignSettingsHistoryItem[]>(`${this.settingsUrl}/history`)
                        .then(res => res.data)
            }
        });
    }

    generateTransactionQueueSasUrl() {
        this.$http
            .post<string>(this.transactionQueueSasGenerationUrl, { expiryTime: this.transactionQueueSasExpiryTime })
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