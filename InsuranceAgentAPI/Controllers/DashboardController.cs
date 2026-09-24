using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InsuranceAgentAPI.Data;
using InsuranceAgentAPI.DTOs;

namespace InsuranceAgentAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string GetCurrentAgentId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
        if (claim != null && !string.IsNullOrEmpty(claim.Value))
        {
            return claim.Value;
        }
        throw new UnauthorizedAccessException("Usuario no autenticado o token no válido.");
    }

    // GET: api/dashboard/stats
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardMetricsDto>> GetStats()
    {
        string agentId = GetCurrentAgentId();
        var today = DateTime.UtcNow.Date;
        var nextMonth = today.AddDays(30);

        // 1. Asegurados Activos (Clientes activos con ClientType == 1)
        var totalActiveClients = await _context.Clients
            .Where(c => c.AgentId == agentId && c.IsActive && c.ClientType == 1)
            .CountAsync();

        // 2. Pólizas Vigentes (Fecha de fin mayor o igual a hoy)
        var activePolicies = await _context.Policies
            .Where(p => p.Client != null && p.Client.AgentId == agentId && p.EndDate >= today)
            .CountAsync();

        // 3. Pólizas por Vencer (Vencimiento entre hoy y los próximos 30 días)
        var expiringPolicies = await _context.Policies
            .Where(p => p.Client != null && p.Client.AgentId == agentId && p.EndDate >= today && p.EndDate <= nextMonth)
            .CountAsync();

        return Ok(new DashboardMetricsDto
        {
            TotalActiveClients = totalActiveClients,
            ActivePolicies = activePolicies,
            ExpiringPolicies = expiringPolicies
        });
    }
}