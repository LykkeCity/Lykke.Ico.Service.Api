using Lykke.Ico.Core.Helpers;
using Lykke.Service.IcoApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/qr")]
    public class QrController : Controller
    {
        /// <summary>
        /// Get QR image of provided address
        /// </summary>
        /// <remarks>
        /// Image is in png format
        /// </remarks>
        [HttpGet]
        [Route("{address}.png")]
        [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetQrImage([Required] string address)
        {
            return File(QRCodeHelper.GenerateQRPng(address), "image/png");
        }
    }
}
