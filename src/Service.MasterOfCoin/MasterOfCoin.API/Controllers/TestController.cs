using System.Diagnostics;
using Lib.Logger.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transactions.Contracts.Interfaces;

namespace MasterOfCoin.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("test")]
public class TestController(ILogger _logger, ITransactionsApi _transactionsApi) : Controller
{
    private ActivitySource _activitySource = new("Tests");
    
    [HttpGet]
    public async Task<IActionResult> HelloWorld()
    {
        var acitivty = _activitySource.StartActivity("test.activity");
        _logger.Critical("Test Critical log", new { ciritcal = true, test = 123 });
        var result = await _transactionsApi.TestMethod(new(16));
        acitivty?.AddEvent(new(nameof(HelloWorld)));
        return Ok(result.Message);
    }
}