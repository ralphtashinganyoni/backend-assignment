using Microsoft.AspNetCore.Mvc;
using OT.Assessment.App.Services;
using OT.Assessment.Common.Data.DTOs;
namespace OT.Assessment.App.Controllers
{
  
    [ApiController]
    [Route("api/player")]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerWagerService _playerWagerService;
        public PlayerController(IPlayerWagerService playerWagerService)
        {
            _playerWagerService = playerWagerService;  
        }
        //POST api/player/casinowager
        [HttpPost("casinowager")]
        public async Task<IActionResult> SubmitCasinoWager([FromBody] WagerDto wager)
        {
            if (wager == null)
                return BadRequest("Invalid wager details");

            _playerWagerService.PublishWagerToQueue(wager);
            return Ok();
        }

        //GET api/player/{playerId}/wagers
        [HttpGet("{playerId}/casino")]
        public async Task<IActionResult> GetPlayerCasinoWagers(Guid playerId, int pageSize = 10, int page = 1)
        {
            var wagers = await _playerWagerService.GetPlayerWagersAsync(playerId, pageSize, page);
            return Ok(wagers);
        }

        //GET api/player/topSpenders?count=10        
        [HttpGet("topSpenders")]
        public async Task<IActionResult> GetTopSpenders([FromQuery] int count = 10)
        {
            var topSpenders = await _playerWagerService.GetTopSpendersAsync(count);
            return Ok(topSpenders);
        }
    }
}
