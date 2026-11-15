using UnityEngine;

namespace DigitalSignatureLab.Presentation
{
    public static class GuiPanelDrawer
    {
        public static Vector2 BeginPanel(Rect area, string title, Vector2 scrollPosition)
        {
            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("<b>" + title + "</b>");
            return GUILayout.BeginScrollView(scrollPosition);
        }

        public static void EndPanel()
        {
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        public static void DrawReadOnlyField(string label, string value)
        {
            GUILayout.Label(label);
            GUILayout.TextArea(value);
        }
    }
}
