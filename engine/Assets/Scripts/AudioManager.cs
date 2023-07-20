using System;
using Modes.MatchMode;
using SynthesisAPI.EventBus;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    private AudioSource source;
    private EventBus.EventCallback startCallback;
    private EventBus.EventCallback endCallback;

    public void Start() {
        source = gameObject.GetComponent<AudioSource>();

        startCallback = e => {
            MatchStateMachine.OnStateStarted onStateStarted = (MatchStateMachine.OnStateStarted) e;
            Type stateType                                  = onStateStarted.state.GetType();
            if (stateType == typeof(MatchStateMachine.Auto)) {
                source.clip = SynthesisAssetCollection.GetAudioClip("Start_Auto");
                source.Play();
            } else if (stateType == typeof(MatchStateMachine.Teleop)) {
                source.clip = SynthesisAssetCollection.GetAudioClip("Start_Teleop");
                source.Play();
            } else if (stateType == typeof(MatchStateMachine.Endgame)) {
                source.clip = SynthesisAssetCollection.GetAudioClip("Start_of_End_Game");
                source.Play();
            }
        };

        endCallback = e => {
            MatchStateMachine.OnStateEnded onStateEnded = (MatchStateMachine.OnStateEnded) e;
            Type stateType                              = onStateEnded.state.GetType();
            if (stateType == typeof(MatchStateMachine.Endgame)) {
                source.clip = SynthesisAssetCollection.GetAudioClip("Match_End");
                source.Play();
            }
        };

        EventBus.NewTypeListener<MatchStateMachine.OnStateStarted>(startCallback);
        EventBus.NewTypeListener<MatchStateMachine.OnStateEnded>(endCallback);
    }

    public void OnDestroy() {
        EventBus.RemoveTypeListener<MatchStateMachine.OnStateStarted>(startCallback);
        EventBus.RemoveTypeListener<MatchStateMachine.OnStateEnded>(endCallback);
    }
}