using MemoryPack;

namespace AspTutorial1.Models;

[MemoryPackable]
public partial class MemTestDTO
{
    public string Street { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
}