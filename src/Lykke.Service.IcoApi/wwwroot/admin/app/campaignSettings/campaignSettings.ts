﻿import { app, AppCommand, AppToast, AppToastType } from "../app.js";
import { ShellController } from "../shell/shell.js";
import { CampaignSettingsHistoryController, CampaignSettingsHistoryItem } from "./campaignSettingsHistory.js";

class CampaignSettings {
    preSaleMinInvestAmountUsd: number;
    preSaleStartDateTimeUtc: Date;
    preSaleEndDateTimeUtc: Date;
    preSaleSmarcAmount: number;
    preSaleLogiAmount: number;
    preSaleSmarcPriceUsd: number;
    preSaleLogiPriceUsd: number;

    crowdSaleMinInvestAmountUsd: number;
    crowdSaleStartDateTimeUtc: Date;
    crowdSaleEndDateTimeUtc: Date;
    crowdSale1stTierSmarcPriceUsd: number;
    crowdSale1stTierSmarcAmount: number;
    crowdSale1stTierLogiPriceUsd: number;
    crowdSale1stTierLogiAmount: number;
    crowdSale2ndTierSmarcPriceUsd: number;
    crowdSale2ndTierSmarcAmount: number;
    crowdSale2ndTierLogiPriceUsd: number;
    crowdSale2ndTierLogiAmount: number;
    crowdSale3rdTierSmarcPriceUsd: number;
    crowdSale3rdTierSmarcAmount: number;
    crowdSale3rdTierLogiPriceUsd: number;
    crowdSale3rdTierLogiAmount: number;

    rowndDownTokenDecimals: number;
    enableFrontEnd: boolean;
    minEthExchangeRate: number;
    minBtcExchangeRate: number;

    kycEnableRequestSending: boolean;
    kycCampaignId: string;
    kycLinkTemplate: string;
    kycServiceEncriptionKey: string;
    kycServiceEncriptionIv: string;

    captchaEnable: boolean;
    captchaSecret: string;    
}

class CampaignSettingsController implements ng.IComponentController {

    private settingsUrl = "/api/admin/campaign/settings";
    private shell: ShellController;
    private customCommands: AppCommand[] = [
        { name: "Save", action: () => this.save() },
        { name: "History", action: () => this.showHistory() }
    ];

    constructor(private $element: ng.IRootElementService, private $http: ng.IHttpService, private $mdDialog: ng.material.IDialogService) {
    }

    settings: CampaignSettings;

    $onInit() {
        this.$http
            .get<CampaignSettings>(this.settingsUrl)
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
            .addClass("layout-align-start-center") // center children horizontally
    }

    save() {
        if (this.settings) {
            this.$http
                .post(this.settingsUrl, this.settings)
                .then(_ => this.shell.toast({ message: "Changes saved", type: AppToastType.Success }));
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
}

app.component("campaignSettings", {
    controller: CampaignSettingsController,
    templateUrl: "app/campaignSettings/campaignSettings.html",
    require: {
        shell: "^shell",
    }
});