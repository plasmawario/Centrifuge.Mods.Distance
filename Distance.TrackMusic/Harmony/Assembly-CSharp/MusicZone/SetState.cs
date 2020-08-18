﻿using Distance.TrackMusic.Models;
using HarmonyLib;
using System;
using UnityEngine;

namespace Distance.TrackMusic.Harmony
{
    [HarmonyPatch(typeof(MusicZone), "SetState")]
    internal static class MusicZone__SetState
    {
        [HarmonyPostfix]
        internal static void Postfix(MusicZone __instance, bool goingIn)
        {
            Mod mod = Mod.Instance;

            Debug.Log($"SetState {goingIn}");
            try
            {
                var previous = mod.Variables.CachedMusicZoneData.GetOrCreate(__instance, () => new MusicZoneData());
                if (goingIn)
                {
                    previous.PreviousTrackName = mod.Variables.CurrentTrackName;
                    mod.SoundPlayer.PlayTrack(mod.SoundPlayer.GetMusicChoiceValue(__instance.gameObject, "Zone"), 0f);
                }
                else
                {
                    mod.SoundPlayer.PlayTrack(previous.PreviousTrackName, 0f);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"SetState failed: {e}");
            }
        }
    }
}
