/// <reference path="../../node_modules/monaco-editor/monaco.d.ts" />
import { app } from "../app.js";
class CampaignEmailTemplate {
}
class CampaignEmailTemplatesController {
    constructor($element, $http, $timeout, $mdTheming) {
        this.$element = $element;
        this.$http = $http;
        this.$timeout = $timeout;
        this.$mdTheming = $mdTheming;
        this.templatesUrl = "/api/admin/campaign/email/templates";
        this.customCommands = [{ name: "Save", action: () => this.save() }];
    }
    $onInit() {
        // init code editor through $timeout service in order 
        // to give time for md-* directives to prepare layout
        this.loadTemplates()
            .then(() => this.$timeout(() => this.initEditor()))
            .then(() => this.selectTemplate());
        // append "save" command to the application toolbar
        this.shell.appendCustomCommands(this.customCommands);
    }
    $onDestroy() {
        // delete "save" command to the application toolbar
        this.shell.deleteCustomCommands(this.customCommands);
    }
    $postLink() {
        // there is no :host class for angular 1.x.x,
        // so style component root element manually
        this.$element
            .addClass("layout-fill") // fill parent
            .addClass("layout-row"); // define self layout
    }
    loadTemplates() {
        return this.$http.get(this.templatesUrl)
            .then(response => {
            this.templates = response.data || [];
        });
    }
    initEditor() {
        this.editor = monaco.editor.create(document.getElementById('email-template-editor'), {
            language: 'razor',
            minimap: { enabled: false },
            renderIndentGuides: true,
            theme: this.$mdTheming.THEMES.default.isDark
                ? "vs-dark"
                : "vs"
        });
    }
    selectTemplate(template) {
        this.selectedTemplate = template || this.templates[0];
        this.editor.setValue(this.selectedTemplate && this.selectedTemplate.body ? this.selectedTemplate.body : "");
    }
    ;
    save() {
        if (!this.selectedTemplate) {
            return;
        }
        this.selectedTemplate.body = this.editor.getValue();
        if (!this.selectedTemplate.body) {
            alert("Body must not be null or empty");
            return;
        }
        this.$http
            .post(this.templatesUrl, this.selectedTemplate)
            .then(response => alert("Changes saved!"), response => alert(response.data.errorMessage));
    }
    ;
    listColors(template) {
        const hue = this.$mdTheming.THEMES.default.isDark ? "800" : "200";
        return {
            background: template == this.selectedTemplate ? `background-${hue}` : 'background'
        };
    }
}
app.component("campaignEmailTemplates", {
    controller: CampaignEmailTemplatesController,
    templateUrl: "app/campaignEmailTemplates/campaignEmailTemplates.html",
    require: {
        shell: "^shell",
    }
});
