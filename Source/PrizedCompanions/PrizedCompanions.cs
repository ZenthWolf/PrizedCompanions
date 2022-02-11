using UnityEngine;
using Verse;

namespace Prized_Companions
{
    public class PrizedCompanions : Mod
	{
		public static PrizedCompanions Instance;
		public Settings settings;

		private bool heavyHighlight = false;

		public PrizedCompanions(ModContentPack content) : base(content)
		{
			if(PrizedCompanions.Instance == null)
            {
				PrizedCompanions.Instance = this;
            }

			this.settings = this.GetSettings<Settings>();
		}

		public override void DoSettingsWindowContents(Rect rect)
		{
			Companion_Listing listingCompanion = new Companion_Listing();
			listingCompanion.Begin(rect);
			Rect highlightRect;

			//GeneralSettings
			heavyHighlight = false;
			_ =listingCompanion.SectionLabel((string)"Prized_Companions_GeneralSettings".Translate());
			if (settings.isActive)
				highlightRect = listingCompanion.Label((string)"Prized_Companions_Active".Translate());
			else
				highlightRect = listingCompanion.Label((string)"Prized_Companions_Inactive".Translate());
			DoHighlight(highlightRect);
			listingCompanion.CheckboxLabeled((string)"Prized_Companions_ActivateCheckBox".Translate(), ref this.settings.isActive, (string)"Prized_Companions_Activator_TTip".Translate());
			
			if(settings.IsLiteMode)
			{
				GUI.color = Color.red;
				highlightRect = listingCompanion.Label("Cannot be active in Lite mode");
				GUI.color = Color.white;
			}
			else if (!settings.isActive)
				highlightRect = listingCompanion.Label("");
			else if (settings.isCounted)
				highlightRect = listingCompanion.Label((string)"Prized_Companions_Counted".Translate());
			else
				highlightRect = listingCompanion.Label((string)"Prized_Companions_NotCounted".Translate());
			listingCompanion.CheckboxLabeled((string)"Prized_Companions_CountCheckBox".Translate(), ref this.settings.isCounted, (string)"Prized_Companions_Counter_TTip".Translate(), !settings.isActive || settings.IsLiteMode);
			DoHighlight(highlightRect);

			//Patch Options
			heavyHighlight = false;
			_ = listingCompanion.SectionLabel((string)"Prized_Companions_PatchOptions".Translate());
			_ = listingCompanion.Label("Prized_Companions_Patch_Descriptor".Translate());

			if (settings.IsLiteMode ^ settings.liteModeSetter)
			{
				GUI.color = Color.red;
				highlightRect = listingCompanion.Label("Prized_Companions_RestartNecessary".Translate());
				GUI.color = Color.white;
			}
			else if (!settings.IsLiteMode)
				highlightRect = listingCompanion.Label("Prized_Companions_LiteModeOff".Translate());
			else
				highlightRect = listingCompanion.Label("Prized_Companions_LiteModeOn".Translate());
			listingCompanion.CheckboxLabeled((string)"Prized_Companions_LiteMode".Translate(), ref this.settings.liteModeSetter, (string)"Prized_Companions_LiteMode_TTip".Translate());
			DoHighlight(highlightRect);

			if (settings.IsDoGUI ^ settings.doGUISetter)
			{
				GUI.color = Color.red;
				highlightRect = listingCompanion.Label("Prized_Companions_RestartNecessary".Translate());
				GUI.color = Color.white;
			}
			else if (settings.IsDoGUI)
				highlightRect = listingCompanion.Label("Prized_Companions_GUIAdded".Translate());
			else
				highlightRect = listingCompanion.Label("");
			listingCompanion.CheckboxLabeled((string)"Prized_Companions_DoGUI".Translate(), ref this.settings.doGUISetter, (string)"Prized_Companions_DoGUI_TTip".Translate());
			DoHighlight(highlightRect);
			listingCompanion.End();

			this.settings.Update();
			base.DoSettingsWindowContents(rect);
		}
		private void DoHighlight(Rect rect)
        {
			rect.height += 24f;
			Widgets.DrawLightHighlight(rect);
			if(heavyHighlight)
            {
				Widgets.DrawLightHighlight(rect);
			}

			heavyHighlight = !heavyHighlight;
		}

		public override string SettingsCategory() => (string) "Prized_Companions_Title".Translate();
	}
}
