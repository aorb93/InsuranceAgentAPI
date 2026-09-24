using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsuranceAgentAPI.DTOs;
using InsuranceAgentAPI.Services;

namespace InsuranceAgentAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PoliciesController : ControllerBase
    {
        private readonly IPolicyService _policyService;

        public PoliciesController(IPolicyService policyService)
        {
            _policyService = policyService;
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

        // GET: api/policies
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            string agentId = GetCurrentAgentId();
            var policies = await _policyService.GetAllAsync(agentId);
            return Ok(policies);
        }

        // GET: api/policies/{guid}
        [HttpGet("{guid:guid}")]
        public async Task<IActionResult> GetByGuid(Guid guid)
        {
            string agentId = GetCurrentAgentId();
            var policy = await _policyService.GetByGuidAsync(guid, agentId);
            if (policy == null)
                return NotFound(new { message = $"No se encontró la póliza con GUID {guid} o no tiene autorización para acceder a ella." });

            return Ok(policy);
        }

        // GET: api/policies/client/{clientGuid}
        [HttpGet("client/{clientGuid:guid}")]
        [HttpGet("client/guid/{clientGuid:guid}")]
        public async Task<IActionResult> GetByClientGuid(Guid clientGuid)
        {
            string agentId = GetCurrentAgentId();
            var policies = await _policyService.GetByClientGuidAsync(clientGuid, agentId);
            return Ok(policies);
        }

        // POST: api/policies
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePolicyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string agentId = GetCurrentAgentId();
            var createdPolicy = await _policyService.CreateSinglePolicyAsync(dto, agentId);
            if (createdPolicy == null)
            {
                return BadRequest(new { message = "No se pudo crear la póliza. Verifique que el cliente exista y pertenezca a su cuenta." });
            }

            return CreatedAtAction(nameof(GetByGuid), new { guid = createdPolicy.Guid }, createdPolicy);
        }

        // POST: api/policies/bulk
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] CreateClientPoliciesDto dto)
        {
            if (!ModelState.IsValid || dto.Policies == null || !dto.Policies.Any())
            {
                return BadRequest(new { message = "La solicitud contiene datos no válidos o la lista de pólizas está vacía." });
            }

            string agentId = GetCurrentAgentId();
            var result = await _policyService.CreatePoliciesAsync(dto, agentId);
            if (!result)
            {
                return BadRequest(new { message = "No se pudieron registrar las pólizas. Verifique que el cliente exista y pertenezca a su cuenta." });
            }

            return Ok(new { message = "Póliza(s) guardada(s) exitosamente." });
        }

        // PUT: api/policies/{guid}
        [HttpPut("{guid:guid}")]
        public async Task<IActionResult> Update(Guid guid, [FromBody] UpdatePolicyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string agentId = GetCurrentAgentId();
            var result = await _policyService.UpdatePolicyByGuidAsync(guid, dto, agentId);
            if (!result)
                return NotFound(new { message = $"No se encontró la póliza con GUID {guid} para actualizar o no tiene autorización." });

            return Ok(new { message = "Póliza actualizada exitosamente." });
        }

        // DELETE: api/policies/{guid}
        [HttpDelete("{guid:guid}")]
        public async Task<IActionResult> Delete(Guid guid)
        {
            string agentId = GetCurrentAgentId();
            var result = await _policyService.DeletePolicyByGuidAsync(guid, agentId);
            if (!result)
                return NotFound(new { message = $"No se encontró la póliza con GUID {guid} para eliminar o no tiene autorización." });

            return Ok(new { message = "Póliza eliminada exitosamente." });
        }
    }
}