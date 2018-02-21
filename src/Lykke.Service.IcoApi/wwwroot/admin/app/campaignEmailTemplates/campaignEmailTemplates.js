import { app } from "../app.js";
export class CampaignEmailTemplate {
}
export class CampaignEmailTemplatesController {
    constructor($http) {
        this.$http = $http;
    }
    $onInit() {
        this.loadTemplates();
    }
    loadTemplates() {
        this.$http.get("/api/admin/campaign/email/templates", { headers: { adminAuthToken: "smarcAdm2@" } })
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
