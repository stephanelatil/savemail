using MailKit;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend.Utils;

public class UniqueIdConverter : ValueConverter<UniqueId, ulong>
{
    public UniqueIdConverter()
        : base(
            uniqueId => ((ulong)uniqueId.Validity) << 32 | uniqueId.Id,
            combined => new UniqueId((uint)(combined >> 32), (uint)(combined & 0xFFFFFFFF)))
    {}
}
