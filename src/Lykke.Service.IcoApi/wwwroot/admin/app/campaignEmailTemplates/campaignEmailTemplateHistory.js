export class CampaignEmailTemplateHistoryItem {
}
export class CampaignEmailTemplateHistoryController {
    constructor(templateId, history, $mdDialog) {
        this.templateId = templateId;
        this.history = history;
        this.$mdDialog = $mdDialog;
    }
    close() {
        this.$mdDialog.hide();
    }
}
