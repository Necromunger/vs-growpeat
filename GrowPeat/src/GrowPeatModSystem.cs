using GrowPeat.Behaviors;
using GrowPeat.Entities;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace GrowPeat;

public class GrowPeatModSystem : ModSystem
{
    public override void Start(ICoreAPI api)
    {
        api.RegisterBlockEntityClass("PeatMix", typeof(BlockEntityPeatMix));
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        api.Network.RegisterChannel(BlockEntityPeatMix.ChannelName).RegisterMessageType<BlockPeatMixPacket>();
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.Network.RegisterChannel(BlockEntityPeatMix.ChannelName).RegisterMessageType<BlockPeatMixPacket>();
    }
}
