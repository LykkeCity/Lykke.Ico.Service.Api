import { app, AppToastType } from "../app.js";
import * as utils from "../utils.js";
class CampaignEmailTemplate {
}
class CampaignEmailTemplatesController {
    constructor($element, $http, $timeout, $mdTheming, $mdDialog, $q) {
        this.$element = $element;
        this.$http = $http;
        this.$timeout = $timeout;
        this.$mdTheming = $mdTheming;
        this.$mdDialog = $mdDialog;
        this.$q = $q;
        this.emailRegex = /^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/;
        this.sendPreviewEmailKey = "send_preview_email";
        this.templateDataKey = "template_data";
        this.emailUrl = "/api/admin/campaign/email";
        this.templatesUrl = "/api/admin/campaign/email/templates";
        this.customCommands = [
            { name: "Save", action: () => this.save() },
            { name: "Send", action: () => this.send(), isDisabled: () => this.selectedTemplate && this.selectedTemplate.isLayout }
        ];
    }
    $onInit() {
        // init code editor through $timeout service in order 
        // to give time for md-* directives to prepare layout
        this.loadTemplates()
            .then(() => this.$timeout(() => this.initEditors()))
            .then(() => {
            this.registerIntellisense();
            this.selectTemplate();
            this.shell.appendCustomCommands(this.customCommands);
        });
    }
    $onDestroy() {
        this.shell.deleteCustomCommands(this.customCommands);
        this.bodyEditor.dispose();
        this.dataEditor.dispose();
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
            let data = response.data || [];
            this.layouts = data.filter(t => t.isLayout);
            this.templates = data
                .filter(t => !t.isLayout)
                .map(t => {
                if (t.data) {
                    t.completionItems = Object.getOwnPropertyNames(t.data).map(p => {
                        return {
                            label: p,
                            kind: monaco.languages.CompletionItemKind.Property,
                            insertText: p
                        };
                    });
                    let storedDataJson = localStorage.getItem(`${t.campaignId}_${t.templateId}_${this.templateDataKey}`);
                    if (storedDataJson) {
                        try {
                            let storedData = JSON.parse(storedDataJson);
                            if (storedData) {
                                Object.getOwnPropertyNames(t.data).forEach(p => {
                                    t.data[p] = storedData[p];
                                });
                            }
                        }
                        catch (_a) {
                        }
                    }
                }
                return t;
            });
        });
    }
    initEditors() {
        this.bodyEditor = this.initStandaloneEditor("email-template-body-editor", "razor");
        this.dataEditor = this.initStandaloneEditor("email-template-data-editor", "json");
    }
    initStandaloneEditor(elementId, language) {
        return monaco.editor.create(document.getElementById(elementId), {
            automaticLayout: true,
            language: language,
            minimap: {
                enabled: false
            },
        });
    }
    registerIntellisense() {
        monaco.languages.registerCompletionItemProvider('razor', {
            provideCompletionItems: (model, position) => {
                // check if text until current cursor position ends with "@Model." 
                // or there are no html tags between "@" and "Model."
                let text = model.getValueInRange({ startLineNumber: 0, startColumn: 1, endLineNumber: position.lineNumber, endColumn: position.column });
                if (text.endsWith("@Model.") || (text.endsWith("Model.") && text.substring(text.lastIndexOf("@"), text.length - 6).match(/<(.|\n)*?>/g) == null)) {
                    return this.selectedTemplate && this.selectedTemplate.completionItems;
                }
                return [];
            },
            triggerCharacters: ["."]
        });
    }
    selectTemplate(template) {
        this.selectedTemplate = template || this.templates[0];
        let body = this.selectedTemplate && this.selectedTemplate.body ? this.selectedTemplate.body : "";
        let data = this.selectedTemplate && this.selectedTemplate.data ? JSON.stringify(this.selectedTemplate.data, null, 4) : "{}";
        this.bodyEditor.setValue(body);
        this.dataEditor.setValue(`// Set values and press SEND to send a preview of e-mail\n\r${data}`);
    }
    validate() {
        if (!this.selectedTemplate) {
            return this.$q.reject();
        }
        let errors = monaco.editor.getModelMarkers({})
            .filter(m => m.severity == monaco.Severity.Error)
            .map(m => `${m.message} - ${m.owner == "razor" ? "Body" : "Data Model"}, Line ${m.startLineNumber}`);
        if (!this.bodyEditor.getValue()) {
            errors.push("Body must not be empty");
        }
        if (errors.length) {
            errors.forEach(e => this.shell.toast({ message: e, type: AppToastType.Error }));
            return this.$q.reject();
        }
        return this.$q.resolve();
    }
    cacheDataModel() {
        if (!this.selectedTemplate) {
            return;
        }
        let json = utils.stripJsonComments(this.dataEditor.getValue());
        this.selectedTemplate.data = JSON.parse(json);
        localStorage.setItem(`${this.selectedTemplate.campaignId}_${this.selectedTemplate.templateId}_${this.templateDataKey}`, json);
    }
    save() {
        if (!this.selectedTemplate) {
            return;
        }
        this.validate()
            .then(() => {
            this.cacheDataModel();
            this.selectedTemplate.body = this.bodyEditor.getValue();
            this.$http
                .post(this.templatesUrl, this.selectedTemplate)
                .then(_ => this.shell.toast({ message: "Changes saved", type: AppToastType.Success }));
        });
    }
    send() {
        if (!this.selectedTemplate || this.selectedTemplate.isLayout) {
            return;
        }
        this.validate()
            .then(() => {
            return this.$mdDialog.show(this.$mdDialog.prompt()
                .title("SEND PREVIEW")
                .textContent("Please, provide an email address to send message to:")
                .placeholder("Email")
                .initialValue(localStorage.getItem(this.sendPreviewEmailKey))
                .required(true)
                .ok("Ok")
                .cancel("Cancel"));
        })
            .then((value) => {
            if (this.emailRegex.exec(value) == null) {
                this.shell.toast({ message: "Invalid email address", type: AppToastType.Error });
            }
            else {
                this.cacheDataModel();
                this.$http
                    .post(this.emailUrl, {
                    templateId: this.selectedTemplate.templateId,
                    data: this.selectedTemplate.data,
                    to: value
                })
                    .then(_ => {
                    this.shell.toast({ message: "E-mail sent", type: AppToastType.Success });
                    localStorage.setItem(this.sendPreviewEmailKey, value);
                });
            }
        });
    }
    listColors() {
        const hue = this.$mdTheming.THEMES.default.isDark ? "800" : "100";
        return {
            background: `background-${hue}`
        };
    }
    listItemColors(template) {
        if (template != this.selectedTemplate) {
            return this.listColors();
        }
        const hue = this.$mdTheming.THEMES.default.isDark ? "700" : "300";
        return {
            background: `background-${hue}`
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
