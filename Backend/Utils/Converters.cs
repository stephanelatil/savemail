using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend.Utils;

public class UniqueIdConverter : ValueConverter<MailKit.UniqueId, ulong>
{
    public UniqueIdConverter()
        : base(
            uniqueId => ((ulong)uniqueId.Validity) << 32 | uniqueId.Id,
            combined => new MailKit.UniqueId((uint)(combined >> 32), (uint)(combined & 0xFFFFFFFF)))
    {}
}


public class DateTimeConverter : ValueConverter<DateTime, long>
{
    public DateTimeConverter()
        : base(
            datetime => datetime.ToUniversalTime().Ticks,
            ticks => new DateTime(ticks, DateTimeKind.Utc)
        )
    {}
}
