namespace InsuranceAgentAPI.DTOs
{
    // DTO para lectura / respuesta
    public class PolicyResponseDto
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public int ClientId { get; set; }
        public Guid ClientGuid { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string InsuredFirstName { get; set; } = string.Empty;
        public string InsuredLastName { get; set; } = string.Empty;
        public DateTime? InsuredBirthDate { get; set; }
        public string PolicyType { get; set; } = string.Empty;
        public int PolicyTypeId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string PaymentFrequency { get; set; } = string.Empty;
        public int PaymentFrequencyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal NetPremium { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal CommissionPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO para creación individual
    public class CreatePolicyDto
    {
        public int? ClientId { get; set; }
        public Guid? ClientGuid { get; set; }
        public string InsuredFirstName { get; set; } = string.Empty;
        public string InsuredLastName { get; set; } = string.Empty;
        public DateTime? InsuredBirthDate { get; set; }
        public int PolicyTypeId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public int PaymentFrequencyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal NetPremium { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal CommissionPercentage { get; set; }
    }

    // DTO para creación masiva (desde el modal del cliente)
    public class CreateClientPoliciesDto
    {
        public int? ClientId { get; set; }
        public Guid? ClientGuid { get; set; }
        public List<CreatePolicyDto> Policies { get; set; } = new();
    }

    // DTO para actualización
    public class UpdatePolicyDto
    {
        public string InsuredFirstName { get; set; } = string.Empty;
        public string InsuredLastName { get; set; } = string.Empty;
        public DateTime? InsuredBirthDate { get; set; }
        public int PolicyTypeId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public int PaymentFrequencyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal NetPremium { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal CommissionPercentage { get; set; }
    }
}