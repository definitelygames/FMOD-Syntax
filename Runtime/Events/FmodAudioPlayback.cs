using System;
using System.IO;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using RoyTheunissen.FMODSyntax.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace RoyTheunissen.FMODSyntax
{
    /// <summary>
    /// Non-generic base class for AudioFmodPlayback to apply as a type constraint.
    /// </summary>
    public abstract class FmodAudioPlaybackBase : FmodPlayablePlaybackBase
    {
        private bool isOneshot = false;
        public bool IsOneshot
        {
            get => isOneshot;
            protected set => isOneshot = value;
        }
    }
    
    /// <summary>
    /// Playback for a playable FMOD audio event. Allows you to update its parameters.
    /// Produced by calling Play() on an AudioFmodConfig, which are specified in FmodEvents.AudioEvents
    /// </summary>
    public abstract class FmodAudioPlayback : FmodAudioPlaybackBase, IAudioPlayback
    {
        public void Play(EventDescription eventDescription, Vector3 source)
        {
            Instance = CreateInstance(eventDescription);

            if (source != null)
            {
                Instance.set3DAttributes(RuntimeUtils.To3DAttributes(source));
            }

            Play();
        }

        public void Play(EventDescription eventDescription, GameObject source)
        {
            Instance = CreateInstance(eventDescription);

            if (source != null)
            {
                Instance.set3DAttributes(RuntimeUtils.To3DAttributes(source));
                RuntimeManager.AttachInstanceToGameObject(Instance, source);
            }

            Play();
        }

        [Obsolete("Deprecated. Pass a GameObject or Vector3 instead.")]
        public void Play(EventDescription eventDescription, Transform source)
        {
            Instance = CreateInstance(eventDescription);

            if (source != null)
            {
                Instance.set3DAttributes(RuntimeUtils.To3DAttributes(source));
                RuntimeManager.AttachInstanceToGameObject(Instance, source);
            }

            Play();
        }

        private void Play()
        {
            EventDescription.isOneshot(out bool isOneshotResult);
            IsOneshot = isOneshotResult;

            InitializeParameters();

            Instance.start();

            FmodSyntaxSystem.RegisterActiveEventPlayback(this);
        }

        public void Stop()
        {
            if (Instance.isValid())
                Instance.stop(STOP_MODE.ALLOWFADEOUT);
        }

        public override void Cleanup()
        {
            if (Instance.isValid())
            {
                Instance.stop(STOP_MODE.IMMEDIATE);
                
                RuntimeManager.DetachInstanceFromGameObject(Instance);
                if (EventDescription.isValid())
                {
                    Instance.release();
                    Instance.clearHandle();
                }
            }

            FmodSyntaxSystem.UnregisterActiveEventPlayback(this);
        }
    }
}
