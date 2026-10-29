using System;
using ITKamianets.Paintball.Ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ITKamianets.Paintball.Menus
{
    public class PauseMenu : MonoBehaviour
    {
        Button _resume;

        public event Action Resume;
        public event Action Leave;

        public static PauseMenu Create()
        {
            var canvas = UiKit.Canvas("Pause Menu", 30);
            var menu = canvas.gameObject.AddComponent<PauseMenu>();
            menu.Build(canvas);
            menu.gameObject.SetActive(false);
            return menu;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (EventSystem.current != null && _resume != null)
            {
                EventSystem.current.SetSelectedGameObject(_resume.gameObject);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        void Build(RectTransform canvas)
        {
            UiKit.Panel(canvas, new Color(0.03f, 0.04f, 0.06f, 0.55f));
            UiKit.Label(canvas, "Title", "Пауза", 56, Color.white, new Vector2(0.5f, 0.5f), new Vector2(400f, 70f), new Vector2(0f, 120f), TextAnchor.MiddleCenter, FontStyle.Bold);
            _resume = UiKit.Button(canvas, "Продовжити", new Color(0.22f, 0.48f, 0.32f), new Vector2(0f, 10f), new Vector2(360f, 68f), () => Resume?.Invoke());
            UiKit.Button(canvas, "До меню", new Color(0.35f, 0.28f, 0.24f), new Vector2(0f, -80f), new Vector2(360f, 68f), () => Leave?.Invoke());
        }
    }
}
