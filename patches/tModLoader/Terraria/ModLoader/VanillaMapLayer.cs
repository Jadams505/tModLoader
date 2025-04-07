using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
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

public class RevengeMarkerLayer : VanillaMapLayer
{
	private CoinLossRevengeSystem.RevengeMarker _revengeMarker = null;

	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		var spriteBatch = Main.spriteBatch;
		var uIScaleMatrix = Main.UIScaleMatrix;
		_revengeMarker = NPC.RevengeManager.DrawMapIcons(spriteBatch, context.MapPosition, context.MapOffset, context.ClippingRectangle, context.MapScale, context.DrawScale, ref text);

		// Skip hover text on the minimap since it is handled later in Main.DrawMap()
		if (!Main.mapFullscreen && Main.mapStyle == 1)
			return;

		if (_revengeMarker != null) {
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, uIScaleMatrix);
			_revengeMarker.UseMouseOver(spriteBatch, ref text, context.DrawScale);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		}
	}

	// Call this at the end of minimap drawing in Main.DrawMap() to ensure the text is drawn after the minimap frame.
	internal void UseMouseOver(SpriteBatch spriteBatch, ref string text, float scale = 1)
	{
		if (Visible)
			_revengeMarker?.UseMouseOver(spriteBatch, ref text, scale);
	}
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
		Main.instance.DrawMapNPCHeads(ref context, ref text);
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

public class PlayerHeadsMapLayer : VanillaMapLayer
{
	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		bool unityHover = false;
		foreach (var player in Main.ActivePlayers) {
			if ((Main.LocalPlayer.hostile || player.hostile) && (Main.LocalPlayer.team != player.team || player.team == 0) && player.whoAmI != Main.myPlayer)
				continue;

			if (player.dead)
				continue;

			var playerTilePosition = (player.position + new Vector2(0, player.gfxOffY) + player.Size / 2) / 16;
			var position = (playerTilePosition - context.MapPosition) * context.MapScale;
			position += context.MapOffset;

			float vanillaYOffset = 2f - context.MapScale / 5f * 2f;
			position.Y -= vanillaYOffset;
			position.X -= 6f;

			// the minimap has a different offset for some reason
			if (Main.mapFullscreen || Main.mapStyle == 2)
				position.Y -= 2f;
			else if (Main.mapStyle == 1)
				position.Y -= 6f;

			var borderColor = Main.GetPlayerHeadBordersColor(player);
			float alpha = 1f;
			if (Main.mapStyle == 1)
				alpha = Main.mapMinimapAlpha;
			else if (Main.mapStyle == 2)
				alpha = Main.mapOverlayAlpha;

			Main.MapPlayerRenderer.DrawPlayerHead(Main.Camera, player, position, alpha, context.DrawScale, borderColor);

			float hoverTopLeftX = position.X + 4f - 14f * context.DrawScale;
			float hoverTopLeftY = position.Y + 2 - 14f * context.DrawScale;
			float hoverSize = 28f * context.DrawScale;
			if (!Utils.FloatIntersect(hoverTopLeftX, hoverTopLeftY, hoverSize, hoverSize, Main.mouseX, Main.mouseY, 0f, 0f))
				continue;

			text = player.name;
			if (player.whoAmI != Main.myPlayer && Main.LocalPlayer.team > 0 && Main.LocalPlayer.team == player.team && Main.netMode == 1 && Main.LocalPlayer.HasUnityPotion() && !unityHover && !Main.cancelWormHole) {
				unityHover = true;
				if (!Main.instance.unityMouseOver)
					SoundEngine.PlaySound(12);

				Main.instance.unityMouseOver = true;
				borderColor = Main.OurFavoriteColor;
				Main.MapPlayerRenderer.DrawPlayerHead(Main.Camera, player, position, 1f, context.DrawScale + 0.5f, borderColor);
				text = Language.GetTextValue("Game.TeleportTo", player.name);
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					Main.mouseLeftRelease = false;
					Main.mapFullscreen = false;
					Main.LocalPlayer.UnityTeleport(player.position);
					Main.LocalPlayer.TakeUnityPotion();
				}
			}
		}
		Main.cancelWormHole = false;
		if (!unityHover && Main.instance.unityMouseOver)
			Main.instance.unityMouseOver = false;
	}
}
