﻿// ----------------------------------------------------------------------------
// The MIT License
// LeopotamGroupLibrary https://github.com/Leopotam/LeopotamGroupLibraryUnity
// Copyright (c) 2012-2017 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using LeopotamGroup.Common;
using LeopotamGroup.Events;
using UnityEngine;
using UnityEngine.UI;

namespace LeopotamGroup.SystemUi.Actions {
    /// <summary>
    /// Event data of UiToggleAction.
    /// </summary>
    public struct UiToggleActionData {
        /// <summary>
        /// Logical group for filtering events.
        /// </summary>
        public int GroupId;

        /// <summary>
        /// Event sender.
        /// </summary>
        public Toggle Sender;

        /// <summary>
        /// New value.
        /// </summary>
        public bool Value;
    }

    /// <summary>
    /// Ui action for processing Toggle events.
    /// </summary>
    [RequireComponent (typeof (Toggle))]
    public sealed class UiToggleAction : UiActionBase {
        Toggle _toggle;

        protected override void Awake () {
            base.Awake ();
            _toggle = GetComponent<Toggle> ();
            _toggle.onValueChanged.AddListener (OnSliderValueChanged);
        }
        void OnSliderValueChanged (bool value) {
            if (Singleton.IsTypeRegistered<UnityEventBus> ()) {
                var action = new UiToggleActionData ();
                action.GroupId = GroupId;
                action.Sender = _toggle;
                action.Value = value;
                Singleton.Get<UnityEventBus> ().Publish<UiToggleActionData> (action);
            }
        }
    }
}