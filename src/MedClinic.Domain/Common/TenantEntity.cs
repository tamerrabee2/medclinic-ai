namespace MedClinic.Domain.Common;

public abstract class TenantEntity : BaseEntity
{
    public Guid ClinicId { get; set; }
}
