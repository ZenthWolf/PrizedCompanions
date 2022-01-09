using System.Collections.Generic;

using Verse;

namespace Prized_Companions
{
	public class Settings : ModSettings
	{
		public bool isActive = true;
		public bool isAlternate = false;

		private bool wasActive = false;
		private bool wasAlternate = false;

		public void Update()
        {
			if(isActive ^ wasActive)
            {
				wasActive = isActive;
				UpdateSlaughterers();

			}
			if(isAlternate ^ wasAlternate)
            {
				wasAlternate = isAlternate;
				UpdateSlaughterers();

			}
        }

		private void UpdateSlaughterers()
        {
			List<Map> SlaughteringGrounds = Current.Game.Maps;
			foreach(Map map in SlaughteringGrounds)
            {
				map.autoSlaughterManager.Notify_ConfigChanged();
            }
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref this.isActive, "PrizeCompanionsIsActive");
			Scribe_Values.Look(ref this.isAlternate, "PrizeCompanionsCountCompanions");

			base.ExposeData();
		}
	}
}
