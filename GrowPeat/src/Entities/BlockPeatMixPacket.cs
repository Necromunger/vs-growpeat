using ProtoBuf;

namespace GrowPeat.Entities;

[ProtoContract]
public class BlockPeatMixPacket
{
    [ProtoMember(1)]
    public bool WaterSaturated;
}
