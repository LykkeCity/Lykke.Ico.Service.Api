import { AppColorsService } from "../app.js";
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
    private bodyEditor: monaco.editor.IStandaloneCodeEditor;

    constructor(public templateId: string, public history: CampaignEmailTemplateHistoryItem[], public appColors: AppColorsService,
        private $mdDialog: ng.material.IDialogService, private $timeout: ng.ITimeoutService) {
        // order history by change time desc
        history.sort((a, b) => b.changedUtc.getTime() - a.changedUtc.getTime());
    }

    selectedItem: CampaignEmailTemplateHistoryItem;

    $onInit() {
        this.$timeout(() => this.initEditor()).then(() => this.selectHistoryItem());
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
            readOnly: true,
            renderLineHighlight: "none",
            hideCursorInOverviewRuler: true,
        });
    }

    selectHistoryItem(item?: CampaignEmailTemplateHistoryItem) {
        this.selectedItem = item || this.history[0];
        let body = this.selectedItem && this.selectedItem.body ? this.selectedItem.body : "";
        this.bodyEditor.setValue(body);
    }
}