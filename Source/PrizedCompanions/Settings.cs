using System.Collections.Generic;

using Verse;

namespace Prized_Companions
{
	public class Settings : ModSettings
	{
		public bool isActive = true;
		public bool isCounted = false;

		private bool wasActive = false;
		private bool wasCounted = false;

		public void Update()
        {
			if(isActive ^ wasActive)
            {
				wasActive = isActive;
				UpdateSlaughterers();

			}
			if(isCounted ^ wasCounted)
            {
				wasCounted = isCounted;
				UpdateSlaughterers();

			}
        }

		private void UpdateSlaughterers()
		{
			List<Map> SlaughteringGrounds = Find.Maps;
			if (!SlaughteringGrounds.NullOrEmpty())
			{
				foreach (Map map in SlaughteringGrounds)
				{
					map.autoSlaughterManager.Notify_ConfigChanged();
				}
			}
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref this.isActive, "PrizeCompanionsIsActive");
			Scribe_Values.Look(ref this.isCounted, "PrizeCompanionsAreCounted");

			base.ExposeData();
		}
	}
}
