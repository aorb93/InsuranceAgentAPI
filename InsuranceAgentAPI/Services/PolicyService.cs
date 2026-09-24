using InsuranceAgentAPI.Data;
using InsuranceAgentAPI.DTOs;
using InsuranceAgentAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgentAPI.Services
{
    public interface IPolicyService
    {
        Task<IEnumerable<PolicyResponseDto>> GetAllAsync();
        Task<PolicyResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<PolicyResponseDto>> GetByClientIdAsync(int clientId);
        Task<PolicyResponseDto?> CreateSinglePolicyAsync(CreatePolicyDto dto);
        Task<bool> CreatePoliciesAsync(CreateClientPoliciesDto dto);
        Task<bool> UpdatePolicyAsync(int id, UpdatePolicyDto dto);
        Task<bool> DeletePolicyAsync(int id);
        Task<IEnumerable<PolicyResponseDto>> GetByClientGuidAsync(Guid clientGuid);
    }

    public class PolicyService : IPolicyService
    {
        private readonly ApplicationDbContext _context;

        public PolicyService(ApplicationDbContext context)
        {
            _context = context;
        }

        // READ: Obtener todas las pólizas
        public async Task<IEnumerable<PolicyResponseDto>> GetAllAsync()
        {
            return await _context.Policies
                .Include(p => p.Client)
                .Select(p => MapToResponseDto(p))
                .ToListAsync();
        }

        // READ: Obtener póliza por ID
        public async Task<PolicyResponseDto?> GetByIdAsync(int id)
        {
            var policy = await _context.Policies
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == id);

            return policy == null ? null : MapToResponseDto(policy);
        }

        // READ: Obtener pólizas por ID de cliente
        public async Task<IEnumerable<PolicyResponseDto>> GetByClientIdAsync(int clientId)
        {
            return await _context.Policies
                .Include(p => p.Client)
                .Where(p => p.ClientId == clientId)
                .Select(p => MapToResponseDto(p))
                .ToListAsync();
        }

        // CREATE: Crear una sola póliza
        public async Task<PolicyResponseDto?> CreateSinglePolicyAsync(CreatePolicyDto dto)
        {
            var clientExists = await _context.Clients.AnyAsync(c => c.Id == dto.ClientId);
            if (!clientExists) return null;

            var entity = new Policy
            {
                ClientId = dto.ClientId,
                InsuredFirstName = dto.InsuredFirstName,
                InsuredLastName = dto.InsuredLastName,
                InsuredBirthDate = dto.InsuredBirthDate,
                PolicyType = dto.PolicyType,
                PolicyNumber = dto.PolicyNumber,
                Company = dto.Company,
                PaymentFrequency = dto.PaymentFrequency,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                NetPremium = dto.NetPremium,
                TotalPremium = dto.TotalPremium,
                CommissionPercentage = dto.CommissionPercentage,
                CreatedAt = DateTime.UtcNow
            };

            _context.Policies.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id);
        }

        // CREATE: Crear pólizas en lote (Bulk)
        public async Task<bool> CreatePoliciesAsync(CreateClientPoliciesDto dto)
        {
            var clientExists = await _context.Clients.AnyAsync(c => c.Id == dto.ClientId);
            if (!clientExists) return false;

            var entities = dto.Policies.Select(p => new Policy
            {
                ClientId = dto.ClientId,
                InsuredFirstName = p.InsuredFirstName,
                InsuredLastName = p.InsuredLastName,
                InsuredBirthDate = p.InsuredBirthDate,
                PolicyType = p.PolicyType,
                PolicyNumber = p.PolicyNumber,
                Company = p.Company,
                PaymentFrequency = p.PaymentFrequency,
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

        // UPDATE: Actualizar póliza existente
        public async Task<bool> UpdatePolicyAsync(int id, UpdatePolicyDto dto)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null) return false;

            policy.InsuredFirstName = dto.InsuredFirstName;
            policy.InsuredLastName = dto.InsuredLastName;
            policy.InsuredBirthDate = dto.InsuredBirthDate;
            policy.PolicyType = dto.PolicyType;
            policy.PolicyNumber = dto.PolicyNumber;
            policy.Company = dto.Company;
            policy.PaymentFrequency = dto.PaymentFrequency;
            policy.StartDate = dto.StartDate;
            policy.EndDate = dto.EndDate;
            policy.NetPremium = dto.NetPremium;
            policy.TotalPremium = dto.TotalPremium;
            policy.CommissionPercentage = dto.CommissionPercentage;

            return await _context.SaveChangesAsync() > 0;
        }

        // DELETE: Eliminar póliza
        public async Task<bool> DeletePolicyAsync(int id)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null) return false;

            _context.Policies.Remove(policy);
            return await _context.SaveChangesAsync() > 0;
        }

        // Helper para mapear entidad a DTO de respuesta
        private static PolicyResponseDto MapToResponseDto(Policy p) => new()
        {
            Id = p.Id,
            ClientId = p.ClientId,
            ClientName = p.Client != null ? $"{p.Client.FirstName} {p.Client.LastName}" : string.Empty,
            InsuredFirstName = p.InsuredFirstName,
            InsuredLastName = p.InsuredLastName,
            InsuredBirthDate = p.InsuredBirthDate,
            PolicyType = p.PolicyType,
            PolicyNumber = p.PolicyNumber,
            Company = p.Company,
            PaymentFrequency = p.PaymentFrequency,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            NetPremium = p.NetPremium,
            TotalPremium = p.TotalPremium,
            CommissionPercentage = p.CommissionPercentage,
            CreatedAt = p.CreatedAt
        };

        public async Task<IEnumerable<PolicyResponseDto>> GetByClientGuidAsync(Guid clientGuid)
        {
            return await _context.Policies
                .Include(p => p.Client)
                .Where(p => p.Client != null && p.Client.Guid == clientGuid)
                .Select(p => MapToResponseDto(p))
                .ToListAsync();
        }
    }
}