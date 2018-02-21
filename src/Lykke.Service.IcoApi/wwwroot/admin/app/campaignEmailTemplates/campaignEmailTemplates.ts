import { app } from "../app.js";

export class CampaignEmailTemplate {
    campaignId: string;
    templateId: string;
    subject: string;
    body: string;
}

export class CampaignEmailTemplatesController implements ng.IComponentController {

    constructor(private $http: ng.IHttpService) {
    }

    templates: CampaignEmailTemplate[];

    $onInit() {
        this.loadTemplates();
    }

    loadTemplates() {
        this.$http.get<CampaignEmailTemplate[]>("/api/admin/campaign/email/templates", { headers: { adminAuthToken: "smarcAdm2@" } })
            .then(r => {
                this.templates = r.data;
            });
    }
}

app.component("campaignEmailTemplates", {
    bindings: {},
    controller: CampaignEmailTemplatesController,
    controllerAs: "vm",
    templateUrl: "app/campaignEmailTemplates/campaignEmailTemplates.html",
});
