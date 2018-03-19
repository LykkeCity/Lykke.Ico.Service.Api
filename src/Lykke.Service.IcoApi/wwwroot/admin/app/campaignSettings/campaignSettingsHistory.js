export class CampaignSettingsHistoryItem {
}
export class CampaignSettingsHistoryController {
    constructor(history, appColors, $mdDialog, $timeout) {
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
        if (this.editor) {
            this.editor.dispose();
        }
    }
    close() {
        this.$mdDialog.hide();
    }
    initEditor() {
        this.editor = monaco.editor.create(document.getElementById("email-template-history-body-editor"), {
            automaticLayout: true,
            language: "json",
            minimap: {
                enabled: false
            },
            readOnly: true,
            renderIndentGuides: false,
            renderLineHighlight: "none",
            hideCursorInOverviewRuler: true
        });
    }
    selectHistoryItem(item) {
        this.selectedItem = item || this.history[0];
        let value = JSON.stringify(JSON.parse((this.selectedItem && this.selectedItem.settings) || "{}"), null, 4);
        this.editor.setValue(value);
    }
}
