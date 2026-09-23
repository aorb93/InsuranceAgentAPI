using Microsoft.AspNetCore.Mvc;
using InsuranceAgentAPI.DTOs;
using InsuranceAgentAPI.Services;

namespace InsuranceAgentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PoliciesController : ControllerBase
    {
        private readonly IPolicyService _policyService;

        public PoliciesController(IPolicyService policyService)
        {
            _policyService = policyService;
        }

        // GET: api/policies
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var policies = await _policyService.GetAllAsync();
            return Ok(policies);
        }

        // GET: api/policies/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var policy = await _policyService.GetByIdAsync(id);
            if (policy == null)
                return NotFound(new { message = $"No se encontró la póliza con ID {id}." });

            return Ok(policy);
        }

        // GET: api/policies/client/10
        [HttpGet("client/{clientId:int}")]
        public async Task<IActionResult> GetByClientId(int clientId)
        {
            var policies = await _policyService.GetByClientIdAsync(clientId);
            return Ok(policies);
        }

        // POST: api/policies
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePolicyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdPolicy = await _policyService.CreateSinglePolicyAsync(dto);
            if (createdPolicy == null)
            {
                return BadRequest(new { message = "No se pudo crear la póliza. Verifique que el cliente exista." });
            }

            return CreatedAtAction(nameof(GetById), new { id = createdPolicy.Id }, createdPolicy);
        }

        // POST: api/policies/bulk
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] CreateClientPoliciesDto dto)
        {
            if (!ModelState.IsValid || dto.Policies == null || !dto.Policies.Any())
            {
                return BadRequest(new { message = "La solicitud contiene datos no válidos o la lista de pólizas está vacía." });
            }

            var result = await _policyService.CreatePoliciesAsync(dto);
            if (!result)
            {
                return BadRequest(new { message = "No se pudieron registrar las pólizas. Verifique que el cliente exista." });
            }

            return Ok(new { message = "Póliza(s) guardada(s) exitosamente." });
        }

        // PUT: api/policies/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePolicyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _policyService.UpdatePolicyAsync(id, dto);
            if (!result)
                return NotFound(new { message = $"No se encontró la póliza con ID {id} para actualizar." });

            return Ok(new { message = "Póliza actualizada exitosamente." });
        }

        // DELETE: api/policies/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _policyService.DeletePolicyAsync(id);
            if (!result)
                return NotFound(new { message = $"No se encontró la póliza con ID {id} para eliminar." });

            return Ok(new { message = "Póliza eliminada exitosamente." });
        }
    }
}