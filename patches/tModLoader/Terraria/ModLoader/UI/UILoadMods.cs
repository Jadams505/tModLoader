﻿using Microsoft.Xna.Framework;
using System;
using System.Threading;
using Terraria.Localization;
using Terraria.ModLoader.Engine;

namespace Terraria.ModLoader.UI
{
	internal class UILoadMods : UIProgress
	{
		public int modCount;

		private string stageText;

		private CancellationTokenSource _cts;

		public override void OnActivate() {
			base.OnActivate();
			_cts = new CancellationTokenSource();
			OnCancel += () => {
				SetLoadStage("Loading Cancelled");
				_cts.Cancel();
			};
			gotoMenu = 888; // ModLoader will redirect to the mods menu if there are no errors during cancel
			ModLoader.BeginLoad(_cts.Token);
		}

		public override void OnDeactivate() {
			base.OnDeactivate();
			_cts?.Dispose();
			_cts = null;
		}

		public void SetLoadStage(string stageText, int modCount = -1) {
			this.stageText = stageText;
			this.modCount = modCount;
			if (modCount < 0) SetProgressText(Language.GetTextValue(stageText));
			Progress = 0;
			SubProgressText = "";
		}

		private void SetProgressText(string text, string logText = null) {
			Logging.tML.Info(logText ?? text);
			if (Main.dedServ) Console.WriteLine(text);
			else DisplayText = text;
		}

		public void SetCurrentMod(int i, string modName, string displayName, Version version) {
			var display = $"{displayName} v{version}";
			var log = $"{modName} ({displayName}) v{version}";
			SetProgressText(Language.GetTextValue(stageText, display), Language.GetTextValue(stageText, log));
			Progress = i / (float)modCount;
		}

		public void SetCurrentMod(int i, Mod mod) => SetCurrentMod(i, mod.Name, mod.DisplayName, mod.Version);
	}
}
