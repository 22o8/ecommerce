namespace Ecommerce.Api.Domain.Entities;

public class ServiceRequirementTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    // سؤال يظهر للزبون عند طلب الخدمة
    public string Question { get; set; } = "";

    // هل السؤال إلزامي؟
    public bool IsRequired { get; set; } = true;

    // ترتيب الظهور
    public int Order { get; set; } = 1;
}
