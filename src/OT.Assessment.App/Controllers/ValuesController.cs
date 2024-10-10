using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OT.Assessment.App.Services;
using OT.Assessment.Common.Data.DTOs;

namespace OT.Assessment.App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        private readonly IPlayerWagerService _playerWagerService;
        public ValuesController(IPlayerWagerService playerWagerService)
        {
            _playerWagerService = playerWagerService;
        }
        //POST api/player/casinowager
        [HttpPost("casinowager")]
        public async Task<IActionResult> SubmitCasinoWager([FromBody] WagerDto wager)
        {
            if (wager == null)
                return BadRequest("Invalid wager details");

            //_playerWagerService.PublishWagerToQueue(wager);
            return Ok();
        }
    }
}
