using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace GrowPeat.Behaviors;

public class BlockEntityPeatMix : BlockEntity
{
    private float cureTimeDays;
    private float waterloggedTimeDays;
    private bool waterSaturated;

    internal static Random rand = new Random();
    internal Block peatBlock;

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        cureTimeDays = Block.Attributes["peatCureTimeDays"].AsFloat(60);

        if (api is ICoreServerAPI)
        {
            peatBlock = Api.World.GetBlock(new AssetLocation("game:peat-none"));
            RegisterGameTickListener(Update, 10000 + rand.Next(500));
        }
    }

    private void Update(float dt)
    {
        var sapi = Api as ICoreServerAPI;
        if (sapi == null || !sapi.World.IsFullyLoadedChunk(Pos))
            return;

        var isWaterlogged = GetNearbyWaterDistance();
        if (!waterSaturated && isWaterlogged)
        {
            waterSaturated = isWaterlogged;
            waterloggedTimeDays = (float)Api.World.Calendar.ElapsedDays;
            MarkDirty();
        }
        else if (!isWaterlogged)
        {
            waterSaturated = isWaterlogged;
            MarkDirty();
        }

        if (waterSaturated && (float)Api.World.Calendar.ElapsedDays > waterloggedTimeDays + cureTimeDays)
        {
            Api.World.BlockAccessor.SetBlock(peatBlock.Id, Pos);
        }
    }

    public override void OnBlockPlaced(ItemStack byItemStack = null)
    {
        Update(0);
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
    {
        if (waterSaturated)
        {
            dsc.AppendLine("Water Saturated: Yes");

            var currentDay = (float)Api.World.Calendar.ElapsedDays;
            float timeRemaining = GameMath.Clamp((waterloggedTimeDays + cureTimeDays) - currentDay, 0, cureTimeDays);
            dsc.AppendLine($"Days till converted: {timeRemaining:0.0}");
        }
        else
        {
            dsc.AppendLine("Water Saturated: No");
        }

        dsc.ToString();
    }

    protected bool GetNearbyWaterDistance()
    {
        var foundWater = false;

        Api.World.BlockAccessor.SearchFluidBlocks(new BlockPos(Pos.X - 2, Pos.Y, Pos.Z - 2), new BlockPos(Pos.X + 2, Pos.Y + 2, Pos.Z + 2), delegate (Block block, BlockPos pos)
        {
            if (block.LiquidCode == "water")
                foundWater = true;

            return true;
        });

        return foundWater;
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        waterloggedTimeDays = tree.GetFloat("waterloggedTimeDays");
        waterSaturated = tree.GetBool("waterSaturated");
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetFloat("waterloggedTimeDays", waterloggedTimeDays);
        tree.SetBool("waterSaturated", waterSaturated);
    }
}
