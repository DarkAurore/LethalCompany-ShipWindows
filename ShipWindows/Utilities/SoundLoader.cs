using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Debug = System.Diagnostics.Debug;
using System.Collections.Generic;

namespace ShipWindows.Utilities;

public static class SoundLoader
{
    public static readonly AudioClip[] CommonSellCounterLines = new AudioClip[1];
    public static readonly AudioClip[] RareSellCounterLines = new AudioClip[1];
    public static readonly List<AudioClip> VoiceLines = new List<AudioClip>();

    public static IEnumerator LoadAudioClips()
    {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        Debug.Assert(assemblyDirectory != null, nameof(assemblyDirectory) + " != null");
        var audioPath = Path.Combine(assemblyDirectory, "sounds");

        audioPath = Directory.Exists(audioPath) ? audioPath : Path.Combine(assemblyDirectory);


        var voiceLinesAudioPath = Path.Combine(audioPath, "voicelines");

        voiceLinesAudioPath = Directory.Exists(voiceLinesAudioPath) ? voiceLinesAudioPath : Path.Combine(audioPath);

        if (WindowConfig.enableShutter.Value)
        {
            if (!WindowConfig.enableCustomRandomShutterVoiceLines.Value)
            {
                ShipWindows.Logger.LogInfo("Loading Wesley voice lines...");
                LoadShutterCloseClip(voiceLinesAudioPath);
                LoadShutterOpenClip(voiceLinesAudioPath);
            }
            else
            {
                ShipWindows.Logger.LogInfo("Loading Custom Random voice lines...");
                LoadCustomAudioClips(voiceLinesAudioPath);
            }
        }
        if (WindowConfig.enableWesleySellAudio.Value)
        {
            LoadSellCounterClips(voiceLinesAudioPath);
        }
        yield break;
    }


    private static void LoadCustomAudioClips(string voiceLinesAudioPath)
    {
        var customFolderPath = Path.Combine(voiceLinesAudioPath, "Custom");
        if (!Directory.Exists(customFolderPath))
        {
            Directory.CreateDirectory(customFolderPath);
        }
        var customFiles = Directory.GetFiles(customFolderPath);
        foreach (var customFile in customFiles)
        {
            var customClip = LoadAudioClipFromFile(customFile);
            if (customClip != null)
            {
                VoiceLines.Add(customClip);
            }
        }
    }

    private static void LoadShutterOpenClip(string voiceLinesAudioPath)
    {
        var shutterOpenFilePath = Path.Combine(voiceLinesAudioPath, "ShutterOpen.wav");
        var shutterOpenVoiceLineAudioClip = LoadAudioClipFromFile(shutterOpenFilePath);

        if (shutterOpenVoiceLineAudioClip != null)
        {
            VoiceLines[0] = shutterOpenVoiceLineAudioClip;
        }
    }

    private static void LoadShutterCloseClip(string voiceLinesAudioPath)
    {
        var shutterCloseFilePath = Path.Combine(voiceLinesAudioPath, "ShutterClose.wav");
        var shutterCloseVoiceLineAudioClip = LoadAudioClipFromFile(shutterCloseFilePath);
        if (shutterCloseVoiceLineAudioClip != null)
        {
            VoiceLines[1] = shutterCloseVoiceLineAudioClip;
        }
    }

    private static void LoadSellCounterClips(string voiceLinesAudioPath)
    {
        var sellCounterFilePath = Path.Combine(voiceLinesAudioPath, "SellCounter1.wav");
        var sellCounterAudioClip = LoadAudioClipFromFile(sellCounterFilePath);

        if (sellCounterAudioClip != null)
        {
            if (WindowConfig.makeWesleySellAudioRare.Value)
            {
                RareSellCounterLines[0] = sellCounterAudioClip;
            }
            else
            {
                CommonSellCounterLines[0] = sellCounterAudioClip;
            }
        }
    }

    private static AudioClip? LoadAudioClipFromFile(string filePath)
    {
        var audioType = AudioType.UNKNOWN;
        var fileNameWithExt = Path.GetFileName(filePath);
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath).Substring(1);
        switch (extension.ToLower())
        {
            case "wav":
                audioType = AudioType.WAV;
                break;
            case "mp3":
                audioType = AudioType.MPEG;
                break;
        }

        using var unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(filePath, audioType);

        var asyncOperation = unityWebRequest.SendWebRequest();

        while (!asyncOperation.isDone)
            Thread.Sleep(100);

        if (unityWebRequest.result != UnityWebRequest.Result.Success)
        {
            ShipWindows.Logger.LogError($"Failed to load AudioClip file '{filePath}': {unityWebRequest.error}");
            return null;
        }

        var clip = DownloadHandlerAudioClip.GetContent(unityWebRequest);

        clip.name = fileNameWithoutExt;
        ShipWindows.Logger.LogInfo($"Loaded AudioClip '{fileNameWithExt}'");

        return clip;
    }
}