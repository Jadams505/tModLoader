using Terraria.Map;

namespace Terraria.ModLoader;

public abstract class VanillaMapLayer : IMapLayer
{
	public bool Visible { get; set; } = true;

	public abstract void Draw(ref MapOverlayDrawContext context, ref string text);
}

public class GolfBallMapLayer : VanillaMapLayer
{
	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		Main.instance.DrawMapIcons_LastGolfballHit(ref context, ref text);
	}
}

public class PotionOfReturnMapLayer : VanillaMapLayer
{
	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		Main.instance.DrawMapIcons_PotionOfReturnHomePosition(ref context, ref text);
		Main.instance.DrawMapIcons_PotionOfReturnAppearAfterUsePosition(ref context, ref text);
	}
}
