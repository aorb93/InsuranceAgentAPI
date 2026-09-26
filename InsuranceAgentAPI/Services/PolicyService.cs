using InsuranceAgentAPI.Data;
using InsuranceAgentAPI.DTOs;
using InsuranceAgentAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgentAPI.Services
{
    public interface IPolicyService
    {
        Task<IEnumerable<PolicyResponseDto>> GetAllAsync(string agentId);
        Task<PolicyResponseDto?> GetByGuidAsync(Guid guid, string agentId);
        Task<IEnumerable<PolicyResponseDto>> GetByClientGuidAsync(Guid clientGuid, string agentId);
        Task<PolicyResponseDto?> CreateSinglePolicyAsync(CreatePolicyDto dto, string agentId);
        Task<bool> CreatePoliciesAsync(CreateClientPoliciesDto dto, string agentId);
        Task<bool> UpdatePolicyByGuidAsync(Guid guid, UpdatePolicyDto dto, string agentId);
        Task<bool> DeletePolicyByGuidAsync(Guid guid, string agentId);
    }

    public class PolicyService : IPolicyService
    {
        private readonly ApplicationDbContext _context;

        public PolicyService(ApplicationDbContext context)
        {
            _context = context;
        }

        // READ: Obtener todas las pólizas del agente autenticado
        public async Task<IEnumerable<PolicyResponseDto>> GetAllAsync(string agentId)
        {
            return await _context.Policies
                .Include(p => p.Client)
                .Include(p => p.PolicyType)
                .Include(p => p.PaymentFrequency)
                .Where(p => p.Client != null && p.Client.AgentId == agentId)
                .Select(p => MapToResponseDto(p))
                .ToListAsync();
        }

        // READ: Obtener póliza por GUID validando pertenencia al agente
        public async Task<PolicyResponseDto?> GetByGuidAsync(Guid guid, string agentId)
        {
            var policy = await _context.Policies
                .Include(p => p.Client)
                .Include(p => p.PolicyType)
                .Include(p => p.PaymentFrequency)
                .FirstOrDefaultAsync(p => p.Guid == guid && p.Client != null && p.Client.AgentId == agentId);

            return policy == null ? null : MapToResponseDto(policy);
        }

        // READ: Obtener pólizas por GUID de cliente del agente autenticado
        public async Task<IEnumerable<PolicyResponseDto>> GetByClientGuidAsync(Guid clientGuid, string agentId)
        {
            return await _context.Policies
                .Include(p => p.Client)
                .Include(p => p.PolicyType)
                .Include(p => p.PaymentFrequency)
                .Where(p => p.Client != null && p.Client.Guid == clientGuid && p.Client.AgentId == agentId)
                .Select(p => MapToResponseDto(p))
                .ToListAsync();
        }

        // CREATE: Crear una sola póliza verificando que el cliente pertenezca al agente
        public async Task<PolicyResponseDto?> CreateSinglePolicyAsync(CreatePolicyDto dto, string agentId)
        {
            Client? client = null;
            if (dto.ClientGuid.HasValue && dto.ClientGuid.Value != Guid.Empty)
            {
                client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Guid == dto.ClientGuid.Value && c.AgentId == agentId);
            }
            else if (dto.ClientId.HasValue)
            {
                client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Id == dto.ClientId.Value && c.AgentId == agentId);
            }

            if (client == null) return null;

            var entity = new Policy
            {
                Guid = Guid.NewGuid(),
                ClientId = client.Id,
                InsuredFirstName = dto.InsuredFirstName,
                InsuredLastName = dto.InsuredLastName,
                InsuredBirthDate = dto.InsuredBirthDate,
                PolicyTypeId = dto.PolicyTypeId,
                PolicyNumber = dto.PolicyNumber,
                Company = dto.Company,
                PaymentFrequencyId = dto.PaymentFrequencyId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                NetPremium = dto.NetPremium,
                TotalPremium = dto.TotalPremium,
                CommissionPercentage = dto.CommissionPercentage,
                CreatedAt = DateTime.UtcNow
            };

            _context.Policies.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByGuidAsync(entity.Guid, agentId);
        }

        // CREATE: Crear pólizas en lote (Bulk) verificando que el cliente pertenezca al agente
        public async Task<bool> CreatePoliciesAsync(CreateClientPoliciesDto dto, string agentId)
        {
            Client? client = null;
            if (dto.ClientGuid.HasValue && dto.ClientGuid.Value != Guid.Empty)
            {
                client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Guid == dto.ClientGuid.Value && c.AgentId == agentId);
            }
            else if (dto.ClientId.HasValue)
            {
                client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Id == dto.ClientId.Value && c.AgentId == agentId);
            }

            if (client == null) return false;

            var entities = dto.Policies.Select(p => new Policy
            {
                Guid = Guid.NewGuid(),
                ClientId = client.Id,
                InsuredFirstName = p.InsuredFirstName,
                InsuredLastName = p.InsuredLastName,
                InsuredBirthDate = p.InsuredBirthDate,
                PolicyTypeId = p.PolicyTypeId,
                PolicyNumber = p.PolicyNumber,
                Company = p.Company,
                PaymentFrequencyId = p.PaymentFrequencyId,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                NetPremium = p.NetPremium,
                TotalPremium = p.TotalPremium,
                CommissionPercentage = p.CommissionPercentage,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _context.Policies.AddRangeAsync(entities);
            return await _context.SaveChangesAsync() > 0;
        }

        // UPDATE: Actualizar póliza existente por GUID verificando pertenencia al agente
        public async Task<bool> UpdatePolicyByGuidAsync(Guid guid, UpdatePolicyDto dto, string agentId)
        {
            var policy = await _context.Policies
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Guid == guid && p.Client != null && p.Client.AgentId == agentId);
            if (policy == null) return false;

            policy.InsuredFirstName = dto.InsuredFirstName;
            policy.InsuredLastName = dto.InsuredLastName;
            policy.InsuredBirthDate = dto.InsuredBirthDate;
            policy.PolicyTypeId = dto.PolicyTypeId;
            policy.PolicyNumber = dto.PolicyNumber;
            policy.Company = dto.Company;
            policy.PaymentFrequencyId = dto.PaymentFrequencyId;
            policy.StartDate = dto.StartDate;
            policy.EndDate = dto.EndDate;
            policy.NetPremium = dto.NetPremium;
            policy.TotalPremium = dto.TotalPremium;
            policy.CommissionPercentage = dto.CommissionPercentage;

            return await _context.SaveChangesAsync() > 0;
        }

        // DELETE: Eliminar póliza por GUID verificando pertenencia al agente
        public async Task<bool> DeletePolicyByGuidAsync(Guid guid, string agentId)
        {
            var policy = await _context.Policies
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Guid == guid && p.Client != null && p.Client.AgentId == agentId);
            if (policy == null) return false;

            _context.Policies.Remove(policy);
            return await _context.SaveChangesAsync() > 0;
        }

        // Helper para mapear entidad a DTO de respuesta
        private static PolicyResponseDto MapToResponseDto(Policy p) => new()
        {
            Id = p.Id,
            Guid = p.Guid,
            ClientId = p.ClientId,
            ClientGuid = p.Client != null ? p.Client.Guid : Guid.Empty,
            ClientName = p.Client != null ? $"{p.Client.FirstName} {p.Client.LastName}" : string.Empty,
            InsuredFirstName = p.InsuredFirstName,
            InsuredLastName = p.InsuredLastName,
            InsuredBirthDate = p.InsuredBirthDate,
            PolicyType = p.PolicyType != null ? p.PolicyType.Name : string.Empty,
            PolicyTypeId = p.PolicyTypeId,
            PolicyNumber = p.PolicyNumber,
            Company = p.Company,
            PaymentFrequency = p.PaymentFrequency != null ? p.PaymentFrequency.Name : string.Empty,
            PaymentFrequencyId = p.PaymentFrequencyId,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            NetPremium = p.NetPremium,
            TotalPremium = p.TotalPremium,
            CommissionPercentage = p.CommissionPercentage,
            CreatedAt = p.CreatedAt
        };
    }
}