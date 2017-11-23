using Lykke.Ico.Core.Repositories.AddressPool;
using Lykke.Ico.Core.Repositories.EmailHistory;
using Lykke.Ico.Core.Repositories.InvestorHistory;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Infrastructure.Auth;
using Lykke.Service.IcoApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Controllers
{
    [Route("api/admin")]
    [Produces("application/json")]
    public class AdminController : Controller
    {
        private readonly IInvestorService _investorService;
        private readonly IBtcService _btcService;
        private readonly IEthService _ethService;
        private readonly IAddressPoolRepository _addressPoolRepository;
        private readonly IEmailHistoryRepository _emailHistoryRepository;
        private readonly IInvestorHistoryRepository _investorHistoryRepository;

        public AdminController(IInvestorService investorService, IBtcService btcService, IEthService ethService,
            IAddressPoolRepository addressPoolRepository, IEmailHistoryRepository emailHistoryRepository,
            IInvestorHistoryRepository investorHistoryRepository)
        {
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

        [AdminAuth]
        [HttpPost("addresses/pool/add/{count}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddRandomAddressesToPool([Required] int count)
        {
            for (var i = 0; i < count; i++)
            {
                var btcKey = _btcService.GeneratePublicKey();
                var ethKey = _ethService.GeneratePublicKey();

                await _addressPoolRepository.AddAsync(ethKey, btcKey);
            }

            return Ok();
        }
    }
}
