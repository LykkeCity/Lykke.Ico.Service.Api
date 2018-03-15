import * as utils from "../utils.js";

export class CampaignEmailTemplateHistoryItem {
    campaignId: string;
    templateId: string;
    subject: string;
    body: string;
    isLayout: boolean;
    username: string;
    changedUtc: Date;
}

export class CampaignEmailTemplateHistoryController implements ng.IController {
    constructor(public templateId: string, public history: CampaignEmailTemplateHistoryItem[], private $mdDialog: ng.material.IDialogService) {
    }

    close() {
        this.$mdDialog.hide();
    }
}