using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InsuranceAgentAPI.Data;
using InsuranceAgentAPI.DTOs;
using InsuranceAgentAPI.Entities;

namespace InsuranceAgentAPI.Controllers;

[Authorize] // Asegura que las peticiones requieran estar autenticadas
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Método auxiliar para obtener el ID del agente autenticado desde el JWT
    private string GetCurrentAgentId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
        if (claim != null && !string.IsNullOrEmpty(claim.Value))
        {
            return claim.Value;
        }
        throw new UnauthorizedAccessException("Usuario no autenticado o token no válido.");
    }

    // GET: api/clients (Obtiene solo los clientes del agente autenticado)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
    {
        string agentId = GetCurrentAgentId();

        var clients = await _context.Clients
            .Where(c => c.AgentId == agentId) // <-- Filtro por Agente
            .OrderByDescending(c => c.Id)
            .Select(c => new ClientDto
            {
                Id = c.Id,
                AgentId = c.AgentId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                IdentificationNumber = c.IdentificationNumber,
                BirthDate = c.BirthDate,
                City = c.City,
                ClientType = c.ClientType, // Guarda 1 o 2 directamente
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(clients);
    }

    // GET: api/clients/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetClient(int id)
    {
        string agentId = GetCurrentAgentId();

        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == id && c.AgentId == agentId);

        if (client == null)
            return NotFound(new { message = "Asegurado no encontrado o no autorizado" });

        return Ok(new ClientDto
        {
            Id = client.Id,
            AgentId = client.AgentId,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Phone = client.Phone,
            IdentificationNumber = client.IdentificationNumber,
            BirthDate = client.BirthDate,
            City = client.City,
            ClientType = client.ClientType,
            IsActive = client.IsActive,
            CreatedAt = client.CreatedAt
        });
    }

    // POST: api/clients
    [HttpPost]
    public async Task<ActionResult<ClientDto>> CreateClient([FromBody] CreateClientDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string agentId = GetCurrentAgentId();

        var client = new Client
        {
            AgentId = agentId, // <-- Asigna automáticamente el agente logueado
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            IdentificationNumber = dto.IdentificationNumber,
            BirthDate = dto.BirthDate,
            City = dto.City,
            ClientType = dto.ClientType, // Guarda 1 o 2 directamente
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        var clientDto = new ClientDto
        {
            Id = client.Id,
            AgentId = client.AgentId,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Phone = client.Phone,
            IdentificationNumber = client.IdentificationNumber,
            BirthDate = client.BirthDate,
            City = client.City,
            ClientType = client.ClientType,
            IsActive = client.IsActive,
            CreatedAt = client.CreatedAt
        };

        return CreatedAtAction(nameof(GetClient), new { id = client.Id }, clientDto);
    }

    // PUT: api/clients/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClient(int id, [FromBody] UpdateClientDto dto)
    {
        string agentId = GetCurrentAgentId();

        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == id && c.AgentId == agentId);

        if (client == null)
            return NotFound(new { message = "Asegurado no encontrado o no autorizado" });

        client.FirstName = dto.FirstName;
        client.LastName = dto.LastName;
        client.Email = dto.Email;
        client.Phone = dto.Phone;
        client.IdentificationNumber = dto.IdentificationNumber;
        client.BirthDate = dto.BirthDate;
        client.City = dto.City;
        client.ClientType = dto.ClientType;
        client.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/clients/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        string agentId = GetCurrentAgentId();

        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == id && c.AgentId == agentId);

        if (client == null)
            return NotFound(new { message = "Asegurado no encontrado o no autorizado" });

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}