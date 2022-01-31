using UnityEngine;
using Verse;

namespace Prized_Companions
{
    public class PrizedCompanions : Mod
	{
		public static PrizedCompanions Instance;
		public Settings settings;

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
			_=listingCompanion.SectionLabel((string)"Prized_Companions_GeneralSettings".Translate());
			if (this.settings.isActive)
				_ = listingCompanion.Label((string)"Prized_Companions_Active".Translate());
			else
				_ = listingCompanion.Label((string)"Prized_Companions_Inactive".Translate());

			listingCompanion.CheckboxLabeled((string)"Prized_Companions_ActivateCheckBox".Translate(), ref this.settings.isActive, (string)"Prized_Companions_Activator_TTip".Translate());
			if (!this.settings.isActive)
				_ = listingCompanion.Label("");
			else if (this.settings.isCounted)
				_ = listingCompanion.Label((string)"Prized_Companions_Counted".Translate());
			else
				_ = listingCompanion.Label((string)"Prized_Companions_NotCounted".Translate());
			listingCompanion.CheckboxLabeled((string)"Prized_Companions_CountCheckBox".Translate(), ref this.settings.isCounted, (string)"Prized_Companions_Counter_TTip".Translate(), !this.settings.isActive);
			listingCompanion.End();

			this.settings.Update();

			base.DoSettingsWindowContents(rect);
		}

		public override string SettingsCategory() => (string) "Prized_Companions_Title".Translate();
	}
}
