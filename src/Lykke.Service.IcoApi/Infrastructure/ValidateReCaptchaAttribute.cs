using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Lykke.Service.IcoApi.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Infrastructure
{
    public class ValidateReCaptchaAttribute : TypeFilterAttribute
    {
        public ValidateReCaptchaAttribute() : base(typeof(ValidateReCaptchaAttributeImpl))
        {

        }

        class ValidateReCaptchaAttributeImpl : ActionFilterAttribute
        {
            public const string _reCaptchaModelErrorKey = "ReCaptcha";
            private const string _recaptchaResponseTokenKey = "g-recaptcha-response";
            private const string _apiVerificationEndpoint = "https://www.google.com/recaptcha/api/siteverify";
            private readonly ICampaignService _campaignService;

            public ValidateReCaptchaAttributeImpl(ICampaignService campaignService)
            {
                _campaignService = campaignService;
            }

            public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var campaignSettings = await _campaignService.GetCampaignSettings();

                if (campaignSettings.CaptchaEnable)
                {
                    await DoReCaptchaValidation(context, campaignSettings);
                }

                await base.OnActionExecutionAsync(context, next);
            }

            private async Task DoReCaptchaValidation(ActionExecutingContext context, ICampaignSettings campaignSettings)
            {
                if (!context.HttpContext.Request.Headers.ContainsKey(_recaptchaResponseTokenKey))
                {
                    AddModelError(context, "No reCaptcha Token Found");
                    return;
                }

                string token = context.HttpContext.Request.Headers[_recaptchaResponseTokenKey];
                if (string.IsNullOrWhiteSpace(token))
                {
                    AddModelError(context, "No reCaptcha Token Found");
                }
                else
                {
                    await ValidateRecaptcha(context, token, campaignSettings);
                }
            }

            private void AddModelError(ActionExecutingContext context, string error)
            {
                context.ModelState.AddModelError(_reCaptchaModelErrorKey, error.ToString());
            }

            private async Task ValidateRecaptcha(ActionExecutingContext context, string token, ICampaignSettings campaignSettings)
            {
                using (var webClient = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new[] {
                        new KeyValuePair<string, string>("secret", campaignSettings.CaptchaSecret),
                        new KeyValuePair<string, string>("response", token)
                    });

                    var response = await webClient.PostAsync(_apiVerificationEndpoint, content);
                    var json = await response.Content.ReadAsStringAsync();
                    var reCaptchaResponse = JsonConvert.DeserializeObject<ReCaptchaResponse>(json);

                    if (reCaptchaResponse == null)
                    {
                        AddModelError(context, "Unable To Read Response From Server");
                    }
                    else if (!reCaptchaResponse.Success)
                    {
                        AddModelError(context, "Invalid Captcha");
                    }
                }
            }
        }

        class ReCaptchaResponse
        {
            public bool Success { get; set; }
        }
    }    
}
