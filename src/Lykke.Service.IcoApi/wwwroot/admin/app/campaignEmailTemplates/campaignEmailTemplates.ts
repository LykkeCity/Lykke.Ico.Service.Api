/// <reference path="../../node_modules/monaco-editor/monaco.d.ts" />

import { app } from "../app.js";

export class CampaignEmailTemplate {
    campaignId: string;
    templateId: string;
    subject: string;
    body: string;
}

export class CampaignEmailTemplatesController implements ng.IComponentController {

    private editor: monaco.editor.IStandaloneCodeEditor;
    private templatesUrl = "/api/admin/campaign/email/templates";

    constructor(private $http: ng.IHttpService, private $timeout: ng.ITimeoutService) {
    }

    templates: CampaignEmailTemplate[];
    selectedTemplate: CampaignEmailTemplate;

    $onInit() {
        // init code editor by timeout because at the moment of component
        // initialization page layout may not be fully ready
        this.$timeout(() => this.initEditor()).then(() => this.loadTemplates());
    }

    loadTemplates() {
        this.$http.get<CampaignEmailTemplate[]>(this.templatesUrl, { headers: { adminAuthToken: "smarcAdm2@" } })
            .then(response => {
                this.templates = response.data || [];
                this.selectTemplate();
            });
    }

    initEditor() {
        this.editor = monaco.editor.create(document.getElementById('email-template-editor'), {
            language: 'razor',
            minimap: {
                enabled: false
            },
            renderIndentGuides: true,
            theme: "vs",
        });
    }

    selectTemplate(template?: CampaignEmailTemplate) {
        this.selectedTemplate = template || this.templates[0];
        this.editor.setValue(this.selectedTemplate && this.selectedTemplate.body ? this.selectedTemplate.body : "");
    };

    save() {
        if (!this.selectTemplate) {
            return;
        }

        this.selectedTemplate.body = this.editor.getValue();

        if (!this.selectedTemplate.body) {
            alert("Body must not be null or empty");
            return;
        }

        this.$http.post(this.templatesUrl, this.selectedTemplate, { headers: { adminAuthToken: "smarcAdm2@" } })
            .then(response => alert("Changes saved!"), response => alert(response.data.errorMessage));
    };
}

app.component("campaignEmailTemplates", {
    bindings: {},
    controller: CampaignEmailTemplatesController,
    controllerAs: "vm",
    templateUrl: "app/campaignEmailTemplates/campaignEmailTemplates.html",
});
