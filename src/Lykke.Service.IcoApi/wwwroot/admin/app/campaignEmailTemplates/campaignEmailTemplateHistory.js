export class CampaignEmailTemplateHistoryItem {
}
export class CampaignEmailTemplateHistoryController {
    constructor(templateId, history, appColors, $mdDialog, $timeout) {
        this.templateId = templateId;
        this.history = history;
        this.appColors = appColors;
        this.$mdDialog = $mdDialog;
        this.$timeout = $timeout;
        // order history by change time desc
        history.sort((a, b) => b.changedUtc.getTime() - a.changedUtc.getTime());
    }
    $onInit() {
        if (this.history && this.history.length) {
            this.$timeout(() => this.initEditor()).then(() => this.selectHistoryItem());
        }
    }
    $onDestroy() {
        if (this.bodyEditor) {
            this.bodyEditor.dispose();
        }
    }
    close() {
        this.$mdDialog.hide();
    }
    initEditor() {
        this.bodyEditor = monaco.editor.create(document.getElementById("email-template-history-body-editor"), {
            automaticLayout: true,
            language: "razor",
            minimap: {
                enabled: false
            },
            readOnly: true
        });
    }
    selectHistoryItem(item) {
        this.selectedItem = item || this.history[0];
        let body = this.selectedItem && this.selectedItem.body ? this.selectedItem.body : "";
        this.bodyEditor.setValue(body);
    }
}
