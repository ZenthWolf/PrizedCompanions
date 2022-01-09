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
			if(this.settings.isActive)
				_ = listingCompanion.Label("Named Companions are Prized and will not be autoslaughtered.");
			else
				_ = listingCompanion.Label("Named animals are food, like the rest of the stock.");

			listingCompanion.CheckboxLabeled("Prized Companions is Active", ref this.settings.isActive, "Enable/Disable Mod");
			if (!this.settings.isActive)
				_ = listingCompanion.Label("");
			else if (this.settings.isAlternate)
				_ = listingCompanion.Label("Named Companions are counted in total animal population (if not bonded)");
			else
				_ = listingCompanion.Label("Prized Companions are not counted amongst livestock.");
			listingCompanion.CheckboxLabeled("Count Prized Companions on the autoslaughter tab (they will still be protected!)", ref this.settings.isAlternate, "Alternate method if you want total count easily.", !this.settings.isActive);
			listingCompanion.End();

			this.settings.Update();

			base.DoSettingsWindowContents(rect);
		}

		public override string SettingsCategory() => "Prized Companions";
	}
}
