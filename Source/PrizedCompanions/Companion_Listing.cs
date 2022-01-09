using UnityEngine;
using Verse;

namespace Prized_Companions
{
    public class Companion_Listing : Listing_Standard
    {
        public void CheckboxLabeled(string label, ref bool checkOn, string tooltip = null, bool disabled = false)
        {
            Rect rect = this.GetRect(Text.LineHeight);
            if (!this.BoundingRectCached.HasValue || rect.Overlaps(this.BoundingRectCached.Value))
            {
                if (!tooltip.NullOrEmpty())
                {
                    if (Mouse.IsOver(rect))
                        Widgets.DrawHighlight(rect);
                    TooltipHandler.TipRegion(rect, (TipSignal)tooltip);
                }
                Widgets.CheckboxLabeled(rect, label, ref checkOn, disabled);
            }
            this.Gap(this.verticalSpacing);
        }
    }
}