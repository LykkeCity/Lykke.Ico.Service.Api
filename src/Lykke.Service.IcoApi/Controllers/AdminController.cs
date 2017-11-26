using Common.Log;
using CsvHelper;
using Lykke.Ico.Core.Repositories.AddressPool;
using Lykke.Ico.Core.Repositories.EmailHistory;
using Lykke.Ico.Core.Repositories.InvestorHistory;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Infrastructure.Auth;
using Lykke.Service.IcoApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/admin")]
    [Produces("application/json")]
    public class AdminController : Controller
    {
        private readonly ILog _log;
        private readonly IInvestorService _investorService;
        private readonly IBtcService _btcService;
        private readonly IEthService _ethService;
        private readonly IAddressPoolRepository _addressPoolRepository;
        private readonly IEmailHistoryRepository _emailHistoryRepository;
        private readonly IInvestorHistoryRepository _investorHistoryRepository;

        public AdminController(ILog log, IInvestorService investorService, IBtcService btcService, IEthService ethService,
            IAddressPoolRepository addressPoolRepository, IEmailHistoryRepository emailHistoryRepository,
            IInvestorHistoryRepository investorHistoryRepository)
        {
            _log = log;
            _investorService = investorService;
            _btcService = btcService;
            _ethService = ethService;
            _addressPoolRepository = addressPoolRepository;
            _emailHistoryRepository = emailHistoryRepository;
            _investorHistoryRepository = investorHistoryRepository;
        }

        [AdminAuth]
        [HttpGet("investors/{email}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInvestor(string email)
        {
            var investor = await _investorService.GetAsync(email);
            if (investor == null)
            {
                return NotFound(ErrorResponse.Create("Investor not found"));
            }

            return Ok(FullInvestorResponse.Create(investor));
        }

        [AdminAuth]
        [HttpDelete("investors/{email}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteInvestor(string email)
        {
            await _investorService.DeleteAsync(email);

            return NoContent();
        }

        [AdminAuth]
        [HttpGet("investors/{email}/changeHistory")]
        [ProducesResponseType(typeof(InvestorHistoryResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInvestorHistory(string email)
        {
            var items = await _investorHistoryRepository.GetAsync(email);

            return Ok(InvestorHistoryResponse.Create(items));
        }

        [AdminAuth]
        [HttpGet("investors/{email}/sentEmails")]
        [ProducesResponseType(typeof(InvestorEmailsResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetInvestorEmailsHistory(string email)
        {
            var items = await _emailHistoryRepository.GetAsync(email);

            return Ok(InvestorEmailsResponse.Create(items));
        }

        [AdminAuth]
        [HttpGet("addresses/random/eth")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetRandomEthAddress()
        {
            var key = _ethService.GeneratePublicKey();
            var address = _ethService.GetAddressByPublicKey(key);

            return Ok(new AddressResponse { Address = address } );
        }

        [AdminAuth]
        [HttpGet("addresses/random/btc")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetRandomBtcAddress()
        {
            var key = _btcService.GeneratePublicKey();
            var address = _btcService.GetAddressByPublicKey(key);

            return Ok(new AddressResponse { Address = address });
        }

        /// <remarks>
        /// Imports the scv file with keys
        /// </remarks>
        [AdminAuth]
        [HttpPost("addresses/pool/import")]
        [DisableRequestSizeLimit]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ImportKeys([FromForm] IFormFile file)
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                await _log.WriteInfoAsync(nameof(AdminController), nameof(ImportKeys), $"Start of public keys import");

                var list = new List<IAddressPoolItem>();
                var csv = new CsvReader(reader);
                var counter = 0;

                csv.Configuration.Delimiter = ";";
                csv.Configuration.Encoding = Encoding.ASCII;
                csv.Configuration.HasHeaderRecord = true;

                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var record = csv.GetRecord<PublicKeysModel>();
                    counter++;

                    if (counter % 500 == 0)
                    {
                        await _addressPoolRepository.AddBatchAsync(list);

                        list = new List<IAddressPoolItem>();
                    }

                    list.Add(new AddressPoolItem
                    {
                        Id = counter,
                        BtcPublicKey = record.btcPublic,
                        EthPublicKey = record.ethPublic
                    });

                    if (counter % 10000 == 0)
                    {
                        await _log.WriteInfoAsync(nameof(AdminController), nameof(ImportKeys), $"{counter} imported keys");
                    }
                }

                if (list.Count > 0)
                {
                    await _addressPoolRepository.AddBatchAsync(list);
                }

                await _log.WriteInfoAsync(nameof(AdminController), nameof(ImportKeys), $"{counter} imported keys");
                await _log.WriteInfoAsync(nameof(AdminController), nameof(ImportKeys), $"End of public keys import");

                return Ok(counter);
            }
        }
    }
}
