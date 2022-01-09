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

        public Rect SectionLabel(string label, float maxHeight = -1f, string tooltip = null)
        {
            Text.Font = GameFont.Medium;
            Rect rect = this.Label(label, maxHeight, tooltip);
            Text.Font = GameFont.Small;
            this.GapLine(1f);
            ++rect.height;
            this.Gap(11f);
            rect.height += 11f;
            return rect;
        }
    }
}