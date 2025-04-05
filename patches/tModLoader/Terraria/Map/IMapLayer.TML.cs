using Terraria.ModLoader;

namespace Terraria.Map;

public partial interface IMapLayer
{
	public static IMapLayer GolfBall { get; private set; } = new GolfBallMapLayer();
	public static IMapLayer PotionOfReturn { get; private set; } = new PotionOfReturnMapLayer();
	public static IMapLayer NPCHeads { get; private set; } = new NPCHeadsMapLayer();
	public static IMapLayer Spawn { get; private set; } = new SpawnMapLayer();
	public static IMapLayer Pylons { get; private set; } = new TeleportPylonsMapLayer();
	public static IMapLayer Pings { get; private set; } = new PingMapLayer();

	bool Visible { get; internal set; }

	void Hide() => Visible = false;
}
