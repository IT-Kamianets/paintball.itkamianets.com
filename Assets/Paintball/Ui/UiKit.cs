using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ITKamianets.Paintball.Ui
{
    public static class UiKit
    {
        static Font _font;

        public static Font Font => _font != null ? _font : _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static RectTransform Canvas(string name, int sort)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sort;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return go.GetComponent<RectTransform>();
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 size, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return rect;
        }

        public static RectTransform Stretch(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static Image Panel(Transform parent, Color color)
        {
            var rect = Stretch(parent, "Panel");
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text Label(Transform parent, string name, string value, int size, Color color, Vector2 anchor, Vector2 box, Vector2 position, TextAnchor align = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var rect = Rect(parent, name, anchor, box, position);
            return TextOn(rect.gameObject, value, size, color, align, style);
        }

        public static Text FillLabel(Transform parent, string value, int size, Color color, FontStyle style = FontStyle.Normal)
        {
            var rect = Stretch(parent, "Label");
            return TextOn(rect.gameObject, value, size, color, TextAnchor.MiddleCenter, style);
        }

        public static Button Button(Transform parent, string label, Color color, Vector2 position, Vector2 size, UnityAction onClick)
        {
            var rect = Rect(parent, label, new Vector2(0.5f, 0.5f), size, position);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.28f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.22f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(color.r, color.g, color.b, 0.45f);
            button.colors = colors;
            button.onClick.AddListener(onClick);
            FillLabel(rect, label, 26, Color.white, FontStyle.Bold);
            return button;
        }

        static Text TextOn(GameObject go, string value, int size, Color color, TextAnchor align, FontStyle style)
        {
            var text = go.AddComponent<Text>();
            text.font = Font;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = align;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }
    }
}
