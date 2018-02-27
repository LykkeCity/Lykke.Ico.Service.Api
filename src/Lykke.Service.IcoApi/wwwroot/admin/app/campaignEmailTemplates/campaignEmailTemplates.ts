/// <reference path="../../node_modules/monaco-editor/monaco.d.ts" />

import { app, IAppCommand, AppToastType } from "../app.js";
import { ShellController } from "../shell.js";

class CampaignEmailTemplate {
    campaignId: string;
    templateId: string;
    subject: string;
    body: string;
}

class CampaignEmailTemplatesController implements ng.IComponentController {

    private templatesUrl = "/api/admin/campaign/email/templates";
    private shell: ShellController;
    private editor: monaco.editor.IStandaloneCodeEditor;

    constructor(private $element: ng.IRootElementService, private $http: ng.IHttpService, private $timeout: ng.ITimeoutService, private $mdTheming: ng.material.IThemingService) {
    }

    customCommands: IAppCommand[] = [{ name: "Save", action: () => this.save() }];
    templates: CampaignEmailTemplate[];
    selectedTemplate: CampaignEmailTemplate;

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

    loadTemplates(): ng.IPromise<void> {
        return this.$http.get<CampaignEmailTemplate[]>(this.templatesUrl)
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

    selectTemplate(template?: CampaignEmailTemplate) {
        this.selectedTemplate = template || this.templates[0];
        this.editor.setValue(this.selectedTemplate && this.selectedTemplate.body ? this.selectedTemplate.body : "");
    };

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
    };

    listColors(template: CampaignEmailTemplate) {
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
