using GrowPeat.Behaviors;
using Vintagestory.API.Common;

namespace GrowPeat;

public class GrowPeatModSystem : ModSystem
{
    public override void Start(ICoreAPI api)
    {
        api.RegisterBlockEntityClass("PeatMix", typeof(BlockEntityPeatMix));
    }
}
