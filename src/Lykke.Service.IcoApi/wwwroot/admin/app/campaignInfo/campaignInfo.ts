import { app } from "../app.js";

class CampaignInfo {
    addressPoolCurrentSize: string;
    addressPoolTotalSize: string;

    bctNetwork: string;
    ethNetwork: string;

    investorsConfirmed: string;
    investorsFilledIn: string;
    investorsKycPassed: string;
    investorsRegistered: string;

    smarcPhase: string;
    smarcPhaseTokenPriceUsd: string;
    smarcPhaseTokenAmount: string;
    smarcPhaseTokenAmountAvailable: string;
    smarcPhaseTokenAmountTotal: string;

    logiPhase: string;
    logiPhaseTokenPriceUsd: string;
    logiPhaseTokenAmount: string;
    logiPhaseTokenAmountAvailable: string;
    logiPhaseTokenAmountTotal: string;

    amountPreSaleInvestedBtc: string;
    amountPreSaleInvestedEth: string;
    amountPreSaleInvestedFiat: string;
    amountPreSaleInvestedUsd: string;
    amountPreSaleInvestedSmarcToken: string;
    amountPreSaleInvestedLogiToken: string;

    amountCrowdSaleInvestedBtc: string;
    amountCrowdSaleInvestedEth: string;
    amountCrowdSaleInvestedFiat: string;
    amountCrowdSaleInvestedUsd: string;
    amountCrowdSaleInvestedSmarcToken: string;
    amountCrowdSaleInvestedLogiToken: string;
}

class CampaignInfoController implements ng.IComponentController {

    private infoUrl = "/api/admin/campaign/info";

    constructor(private $element: ng.IRootElementService, private $http: ng.IHttpService) {
    }

    info: CampaignInfo;

    $onInit() {
        this.$http
            .get<CampaignInfo>(this.infoUrl)
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
            .addClass("layout-align-start-center") // center children horizontally
    }
}

app.component("campaignInfo", {
    controller: CampaignInfoController,
    templateUrl: "app/campaignInfo/campaignInfo.html",
});
