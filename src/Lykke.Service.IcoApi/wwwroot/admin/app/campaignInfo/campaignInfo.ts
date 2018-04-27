import { app } from "../app.js";

export class CampaignInfo {
    addressPoolCurrentSize: string;
    addressPoolTotalSize: string;

    bctNetwork: string;
    ethNetwork: string;

    investorsConfirmed: string;
    investorsFilledIn: string;
    investorsKycPassed: string;
    investorsRegistered: string;

    phase: string;
    phaseTokenPriceUsd: string;
    phaseTokenAmount: string;
    phaseTokenAmountAvailable: string;
    phaseTokenAmountTotal: string;

    amountPreSaleInvestedBtc: string;
    amountPreSaleInvestedEth: string;
    amountPreSaleInvestedFiat: string;
    amountPreSaleInvestedUsd: string;
    amountPreSaleInvestedToken: string;

    amountCrowdSaleInvestedBtc: string;
    amountCrowdSaleInvestedEth: string;
    amountCrowdSaleInvestedFiat: string;
    amountCrowdSaleInvestedUsd: string;
    amountCrowdSaleInvestedToken: string;

    campaignId: string;
    tokenName: string;
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
