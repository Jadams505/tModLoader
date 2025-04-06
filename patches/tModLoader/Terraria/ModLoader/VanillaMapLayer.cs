using System;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.Map;
using Terraria.UI;

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

public class NPCHeadsMapLayer : VanillaMapLayer
{
	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		if (Main.mapFullscreen)
			Main.instance.DrawMap_FullscreenMapNPCHeads(ref context, ref text);
		else if (Main.mapStyle == 1)
			Main.instance.DrawMap_MiniMapNPCHeads(ref context, ref text);
		else if (Main.mapStyle == 2)
			Main.instance.DrawMap_OverlayNPCHeads(ref context, ref text);
	}
}

public class DeathMarkersMapLayer : VanillaMapLayer
{
	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		foreach (var player in Main.ActivePlayers) {
			if ((Main.LocalPlayer.hostile || player.hostile) && (Main.LocalPlayer.team != player.team || player.team == 0) && player.whoAmI != Main.myPlayer)
				continue;

			if (!player.showLastDeath)
				continue;

			var position = player.lastDeathPostion / 16f;

			// offset taken from Main.DrawPlayerDeathMarker()
			float vanillaYOffset = 2f - context.MapScale / 5f * 2f;
			position.Y -= vanillaYOffset / context.MapScale;

			var frame = new SpriteFrame(1, 1);
			bool hovering = context.Draw(TextureAssets.MapDeath.Value, position, frame, Alignment.Center).IsMouseOver;

			if (hovering) {
				TimeSpan timeSpan = DateTime.Now - player.lastDeathTime;
				text = Language.GetTextValue("Game.PlayerDeathTime", player.name, Lang.LocalizedDuration(timeSpan, abbreviated: false, showAllAvailableUnits: false));
			}
		}
	}
}
