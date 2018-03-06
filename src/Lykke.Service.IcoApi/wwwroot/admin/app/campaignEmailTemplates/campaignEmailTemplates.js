import { app, AppToastType } from "../app.js";
import * as utils from "../utils.js";
class CampaignEmailTemplate {
}
class CampaignEmailTemplatesController {
    constructor($element, $http, $timeout, $mdTheming, $mdDialog) {
        this.$element = $element;
        this.$http = $http;
        this.$timeout = $timeout;
        this.$mdTheming = $mdTheming;
        this.$mdDialog = $mdDialog;
        this.emailUrl = "/api/admin/campaign/email";
        this.templatesUrl = "/api/admin/campaign/email/templates";
        this.customCommands = [
            { name: "Save", action: () => this.save() },
            { name: "Send", action: () => this.send() }
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
            this.templates.forEach(t => {
                t.completionItems = Object.getOwnPropertyNames(t.data || {}).map(p => {
                    return {
                        label: p,
                        kind: monaco.languages.CompletionItemKind.Property,
                        insertText: p
                    };
                });
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
    save() {
        if (!this.selectedTemplate) {
            return;
        }
        this.selectedTemplate.body = this.bodyEditor.getValue();
        if (!this.selectedTemplate.body) {
            alert("Body must not be null or empty");
            return;
        }
        this.$http
            .post(this.templatesUrl, this.selectedTemplate)
            .then(_ => this.shell.toast({ message: "Changes saved", type: AppToastType.Success }));
    }
    send() {
        if (!this.selectedTemplate) {
            return;
        }
        let prompt = this.$mdDialog.prompt()
            .title("SEND PREVIEW")
            .textContent("Please, provide an email for sending message to:")
            .placeholder("Email")
            .required(true)
            .ok("Ok")
            .cancel("Cancel");
        this.$mdDialog.show(prompt)
            .then(value => {
            this.$http
                .post(this.emailUrl, {
                templateId: this.selectedTemplate.templateId,
                data: JSON.parse(utils.stripJsonComments(this.dataEditor.getValue())),
                to: value
            })
                .then(_ => this.shell.toast({ message: "E-mail sent", type: AppToastType.Success }));
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
