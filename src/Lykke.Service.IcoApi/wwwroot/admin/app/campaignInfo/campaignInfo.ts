import { app } from "../app.js";

class CampaignInfo {
    addressPoolCurrentSize: number;
    addressPoolTotalSize: number;
    amountInvestedBtc: number;
    amountInvestedEth: number;
    amountInvestedFiat: number;
    amountInvestedToken: number;
    amountInvestedUsd: number;
    investorsConfirmed: number;
    investorsFilledIn: number;
    investorsKycPassed: number;
    investorsRegistered: number;
    lastProcessedBlockBtc: number;
    lastProcessedBlockEth: number;
    lastProcessedBlockEthInfura: number;
    bctNetwork: string;
    ethNetwork: string;
    tokenPriceUsd: number;
    phase: string;
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
