using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAgentAPI.Entities
{
    public class Policy
    {
        public int Id { get; set; }

        public Guid Guid { get; set; } = Guid.NewGuid();

        public int ClientId { get; set; }

        [Required]
        [MaxLength(100)]
        public string InsuredFirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string InsuredLastName { get; set; } = string.Empty;

        public DateTime? InsuredBirthDate { get; set; }

        public int PolicyTypeId { get; set; }
        public PolicyType? PolicyType { get; set; }

        [Required]
        [MaxLength(50)]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Company { get; set; } = string.Empty;

        public int PaymentFrequencyId { get; set; }
        public PaymentFrequency? PaymentFrequency { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetPremium { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPremium { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal CommissionPercentage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propiedad de navegación
        public Client? Client { get; set; }
    }
}