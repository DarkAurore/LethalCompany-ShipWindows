﻿using System;
using System.Linq;
using ShipWindows.Components;
using ShipWindows.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace ShipWindows.Networking;

[Serializable]
internal class WindowState {
    [FormerlySerializedAs("WindowsClosed")]
    public bool windowsClosed;

    [FormerlySerializedAs("WindowsLocked")]
    public bool windowsLocked;

    [FormerlySerializedAs("VolumeActive")]
    public bool volumeActive = true;

    [FormerlySerializedAs("VolumeRotation")]
    public float volumeRotation;


    private System.Random _random = new System.Random();

    public WindowState() => Instance = this;

    public static WindowState Instance { get; set; } = null!;

    public void SetWindowState(bool closed, bool locked, bool playVoiceLine = true) {
        if (!WindowConfig.enableShutter.Value) return;


        var windows = Object.FindObjectsByType<ShipWindow>(FindObjectsSortMode.None);

        if (!WindowConfig.enableCustomRandomShutterVoiceLines.Value)
        {
            if (playVoiceLine && windows.Length > 0) PlayVoiceLine(closed ? 1 : 0);
        }
        else
        {
            var randomIndex = _random.Next(0, SoundLoader.VoiceLines.Count);
            PlayVoiceLine(randomIndex);
        }

        foreach (var w in windows)
            w.SetClosed(closed);

        windowsClosed = closed;
        windowsLocked = locked;
    }

    public static void PlayVoiceLine(int clipIndex) {
        if (!WindowConfig.enableShutterVoiceLines.Value || SoundLoader.VoiceLines.Count < clipIndex)
        {
            return;
        }

        ShipWindows.Logger.LogDebug("Playing clip: " + clipIndex);

        var audioClip = SoundLoader.VoiceLines[clipIndex];
        if (audioClip != null)
        {
            var speakerAudioSource = StartOfRound.Instance.speakerAudioSource;

            speakerAudioSource.PlayOneShot(StartOfRound.Instance.disableSpeakerSFX);

            speakerAudioSource.clip = audioClip;
            speakerAudioSource.Play();
        }
    }

    public void SetVolumeState(bool active) {
        var outsideSkybox = ShipWindows.outsideSkybox;
        outsideSkybox?.SetActive(active);

        volumeActive = active;
    }

    public void SetVolumeRotation(float rotation) {
        SpaceSkybox.Instance?.SetRotation(rotation);
        volumeRotation = rotation;
    }

    public void ReceiveSync() {
        // By this point the Instance has already been replaced, so we can just update the actual objects
        // with what the values should be.

        ShipWindows.Logger.LogInfo("Receiving window sync message...");

        //TODO: Check if this causes issues.
        SetWindowState(windowsClosed, windowsLocked, false);
        SetVolumeState(volumeActive);
        SetVolumeRotation(volumeRotation);
    }
}