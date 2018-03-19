import { AppColorsService } from "../app.js";
import * as utils from "../utils.js";

export class CampaignSettingsHistoryItem {

    username: string;
    settings: string;
    changedUtc: Date;
}

export class CampaignSettingsHistoryController implements ng.IController {
    private editor: monaco.editor.IStandaloneCodeEditor;

    constructor(public history: CampaignSettingsHistoryItem[], public appColors: AppColorsService,
        private $mdDialog: ng.material.IDialogService, private $timeout: ng.ITimeoutService) {
        // order history by change time desc
        history.sort((a, b) => b.changedUtc.getTime() - a.changedUtc.getTime());
    }

    selectedItem: CampaignSettingsHistoryItem;

    $onInit() {
        this.$timeout(() => this.initEditor()).then(() => this.selectHistoryItem());
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

    selectHistoryItem(item?: CampaignSettingsHistoryItem) {
        this.selectedItem = item || this.history[0];
        let value = JSON.stringify(JSON.parse((this.selectedItem && this.selectedItem.settings) || ""), null, 4);
        this.editor.setValue(value);
    }
}