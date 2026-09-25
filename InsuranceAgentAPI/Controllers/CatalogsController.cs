using InsuranceAgentAPI.Data;
using InsuranceAgentAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgentAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CatalogsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("policy-types")]
    public async Task<ActionResult<IEnumerable<CatalogDto>>> GetPolicyTypes()
    {
        var result = await _context.PolicyTypes
            .Where(x => x.IsActive)
            .Select(x => new CatalogDto { Id = x.Id, Name = x.Name })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("payment-frequencies")]
    public async Task<ActionResult<IEnumerable<CatalogDto>>> GetPaymentFrequencies()
    {
        var result = await _context.PaymentFrequencies
            .Where(x => x.IsActive)
            .Select(x => new CatalogDto { Id = x.Id, Name = x.Name })
            .ToListAsync();

        return Ok(result);
    }
}