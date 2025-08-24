using Property.Domain.Entities;
using PropertyEntity = Property.Domain.Entities.Property;

namespace Property.Domain.Models;

public class PropertySearchResult
{
    public IEnumerable<PropertyEntity> Properties { get; set; } = Enumerable.Empty<PropertyEntity>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
