// ----------------------------------------------------------------------------
// The MIT License
// LeopotamGroupLibrary https://github.com/Leopotam/LeopotamGroupLibraryUnity
// Copyright (c) 2012-2017 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System.Globalization;
using LeopotamGroup.Math;
using LeopotamGroup.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LeopotamGroup.SystemUi.Markup.Generators {
    static class UiNode {
        static readonly int HashedBase = "base".GetStableHashCode ();

        static readonly int HashedDragTreshold = "dragTreshold".GetStableHashCode ();
        /// <summary>
        /// Create "ui" node. If children supported - GameObject container for them should be returned.
        /// </summary>
        /// <param name="widget">Ui widget.</param>
        /// <param name="node">Xml node.</param>
        /// <param name="container">Markup container.</param>
        public static RectTransform Create (RectTransform widget, XmlNode node, MarkupContainer container) {
#if UNITY_EDITOR
            widget.name = "ui";
#endif
            var go = widget.gameObject;
            var canvas = go.AddComponent<Canvas> ();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            var pixelSize = 1f;
            var dragTreshold = 5;

            var scaler = go.AddComponent<CanvasScaler> ();
            var attrValue = node.GetAttribute (HashedBase);
            if (attrValue != null) {
                var refWidth = 1024;
                var refHeight = 768;
                var refBalance = 1f;
                try {
                    var parts = MarkupUtils.SplitAttrValue (attrValue);
                    var w = int.Parse (parts[0]);
                    var h = int.Parse (parts[1]);
                    var b = Mathf.Clamp01 (float.Parse (parts[2], NumberFormatInfo.InvariantInfo));
                    refWidth = w;
                    refHeight = h;
                    refBalance = b;
                } catch { }
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2 (refWidth, refHeight);
                scaler.matchWidthOrHeight = refBalance;
                if (Application.isPlaying) {
                    pixelSize = Mathf.Lerp (Screen.width / (float) refWidth, Screen.height / (float) refHeight, refBalance);
                } else {
                    pixelSize = 1f;
                }
            }

            attrValue = node.GetAttribute (HashedDragTreshold);
            if (attrValue != null) {
                if (int.TryParse (attrValue, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out dragTreshold)) {
                    dragTreshold = Mathf.Max (1, dragTreshold);
                }
            }

            container.PixelSize = pixelSize;
            container.DragTreshold = dragTreshold * pixelSize;

            go.AddComponent<GraphicRaycaster> ();

            if (Application.isPlaying) {
                var es = Object.FindObjectOfType<EventSystem> ();
                if ((object) es == null) {
                    es = new GameObject ("EventSystem").AddComponent<EventSystem> ();
                    es.gameObject.AddComponent<StandaloneInputModule> ();
                }
            }

            return widget;
        }
    }
}