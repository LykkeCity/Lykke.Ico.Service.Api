/// <reference path="../../node_modules/monaco-editor/monaco.d.ts" />
import { app } from "../app.js";
export class CampaignEmailTemplate {
}
export class CampaignEmailTemplatesController {
    constructor($http, $timeout) {
        this.$http = $http;
        this.$timeout = $timeout;
        this.templatesUrl = "/api/admin/campaign/email/templates";
    }
    $onInit() {
        // init code editor by timeout because at the moment of component
        // initialization page layout may not be fully ready
        this.$timeout(() => this.initEditor()).then(() => this.loadTemplates());
    }
    loadTemplates() {
        this.$http.get(this.templatesUrl, { headers: { adminAuthToken: "smarcAdm2@" } })
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
    selectTemplate(template) {
        this.selectedTemplate = template || this.templates[0];
        this.editor.setValue(this.selectedTemplate && this.selectedTemplate.body ? this.selectedTemplate.body : "");
    }
    ;
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
    }
    ;
}
app.component("campaignEmailTemplates", {
    bindings: {},
    controller: CampaignEmailTemplatesController,
    controllerAs: "vm",
    templateUrl: "app/campaignEmailTemplates/campaignEmailTemplates.html",
});
