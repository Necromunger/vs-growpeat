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
    private float waterSaturatedDays;
    private bool isWaterSaturated;

    internal static Random rand = new Random();
    internal Block peatBlock;

    internal float pastDays;
    internal float lastElapsedDays;

    internal ICoreServerAPI Sapi;

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        cureTimeDays = Block.Attributes["peatCureTimeDays"].AsFloat(60);

        if (api is ICoreServerAPI sapi)
        {
            Sapi = sapi;
            peatBlock = Api.World.GetBlock(new AssetLocation("game:peat-none"));
            RegisterGameTickListener(ServerUpdate, 10000 + rand.Next(500));
        }
    }

    private void ServerUpdate(float dt)
    {
        if (Sapi == null || !Sapi.World.IsFullyLoadedChunk(Pos))
            return;

        var elapsedDays = (float)Api.World.Calendar.ElapsedDays;

        if (lastElapsedDays == 0)
            lastElapsedDays = elapsedDays;

        // If world clock changed, handle invalid time
        if (Api.World.Calendar.ElapsedDays < lastElapsedDays)
            lastElapsedDays = elapsedDays;

        var daysDelta = elapsedDays - lastElapsedDays;
        HandleWaterSaturation(daysDelta);

        lastElapsedDays = elapsedDays;
    }

    private void HandleWaterSaturation(float daysDt)
    {
        isWaterSaturated = GetNearbyWaterDistance();

        if (isWaterSaturated)
            waterSaturatedDays += daysDt;
        else
            waterSaturatedDays = 0;

        // Swap block to peat after cure time
        if (waterSaturatedDays >= cureTimeDays)
            Api.World.BlockAccessor.SetBlock(peatBlock.Id, Pos);

        MarkDirty();
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
    {
        if (isWaterSaturated)
        {
            dsc.AppendLine("Water Saturated: Yes");

            float timeRemaining = GameMath.Clamp(cureTimeDays - waterSaturatedDays, 0, cureTimeDays);
            dsc.AppendLine($"Days until converted: {timeRemaining:0.0}");
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
        waterSaturatedDays = tree.GetFloat("waterSaturatedDays");
        isWaterSaturated = tree.GetBool("isWaterSaturated");
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetFloat("waterSaturatedDays", waterSaturatedDays);
        tree.SetBool("isWaterSaturated", isWaterSaturated);
    }
}
