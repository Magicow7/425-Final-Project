using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static SoundManager;

// References Sound Manager by Code Monkey: https://www.youtube.com/watch?v=QL29aTa7J5Q
public class SoundManager : MonoBehaviour
{
    public enum Sound       // TO DO: 1) FIND + IMPLEMENT SOUNDS FOR LASER WEAPON
                            //        2) FIND PERMANANT SOUNDS FOR ALL TEMP/MISSING SOUNDS
    {
        Walking,            // FUNCTIONAL (WALKING/SPRINTING)
        AirborneMovement,   // FUNCTIONAL (SCALES WITH SPEED)
        Jumping,            // FUNCTIONAL
        PlayerHit,          // FUNCTIONAL
        PlayerLowHp,        // FUNCTIONAL
        PlayerDeath,        // FUNCTIONAL
        LowMana,            // FUNCTIONAL
        FireSpellStart,     // FUNCTIONAL
        FireSpellStop,      // FUNCTIONAL
        MobHit,             // FUNCTIONAL
        MobNoise1,          // FUNCTIONAL
        MobNoise2,          // FUNCTIONAL
        MobNoise3,          // FUNCTIONAL
        MobDeath,           // FUNCTIONAL
        StoodGroundSuccess, // FUNCTIONAL
        StoodGroundFail,    // FUNCTIONAL
        OpenChest,          // FUNCTIONAL
        ShotSpellCast,      // FUNCTIONAL
        NormalBackground,   // FUNCTIONAL
        LowHpBackground,    // FUNCTIONAL
        MenuButtonMove,     // Menu Buttons not implemented (death screen)
        MenuButtonPress,    // Menu Buttons not implemented (death screen)
        LaserShoot,         // FUNCTIONAL
        LaserBlast,         // FUNCTIONAL
        GrenadeThrow,       // FUNCTIONAL
        GrenadeBoom,        // FUNCTIONAL
        SpendGold,          // Gold unimplemented in this version 
    }

    [System.Serializable]
    public class SoundComponents
    {
        [FormerlySerializedAs("sound")] public Sound _sound;
        [FormerlySerializedAs("clip")] public AudioClip _clip;
        [FormerlySerializedAs("volume")] public float _volume = 1;
        [FormerlySerializedAs("maxDistance")] public float _maxDistance = 10;
        [FormerlySerializedAs("wait")] public float _wait = -1;  // set to -1 to be set to the length of the audioclip
    }

    [FormerlySerializedAs("soundComponents")] public SoundComponents[] _soundComponents;

    private static GameObject _player;
    private static Sound _currentBackground;
    private static AudioSource _backgroundMusic;
    private static readonly Dictionary<Sound, AudioClip> _sounds = new Dictionary<Sound, AudioClip>();
    private static readonly Dictionary<Sound, float> _waits = new Dictionary<Sound, float>();
    private static readonly Dictionary<Sound, float> _vols = new Dictionary<Sound, float>();
    private static readonly Dictionary<Sound, float> _dists = new Dictionary<Sound, float>();
    private static readonly Dictionary<Sound, float> _lastPlayed = new Dictionary<Sound, float>();

    private void Start()
    {
        _player = GameObject.Find("PlayerModel");
        foreach(SoundComponents soundComponent in _soundComponents)
        {
            _sounds.Add(soundComponent._sound, soundComponent._clip);
            _waits.Add(soundComponent._sound, soundComponent._wait != -1 ? soundComponent._wait : soundComponent._clip.length);
            _vols.Add(soundComponent._sound, soundComponent._volume);
            _dists.Add(soundComponent._sound, soundComponent._maxDistance);
            _lastPlayed.Add(soundComponent._sound, 0f);
        }
        _backgroundMusic = _player.AddComponent<AudioSource>();
        _backgroundMusic.loop = true;
        ChangeBackgroundMusic(SoundManager.Sound.NormalBackground);
    }

    // Plays a Sound on the Player
    public static void PlaySound(Sound sound)
    {
        PlaySound(sound, _player);
    }

    // Plays a Sound on the given GameObject
    public static void PlaySound(Sound sound, GameObject obj, bool randomPitch = false)
    {
        Vector3 position = obj.transform.position;
        if (CanPlay(sound))
        {
            Debug.Log("Playing " + sound + " on " + obj.name);
            AudioSource source = _player.AddComponent<AudioSource>();
            if (randomPitch)
            {
                source.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            }
            source.spatialBlend = 1.0f;
            source.maxDistance = _dists[sound];
            source.volume = _vols[sound];
            source.rolloffMode = AudioRolloffMode.Custom;
            source.PlayOneShot(GetAudioClip(sound));
            Destroy(source, GetAudioClip(sound).length);
        }
    }

    // Plays a Sound on the given AudioSource
    public static void PlaySound(Sound sound, AudioSource source, bool randomPitch = false)
    {
        if (CanPlay(sound) && !source.isPlaying)
        {
            if (randomPitch)
            {
                source.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            }
            Debug.Log("Playing " + sound + " on given source.");
            source.clip = GetAudioClip(sound);
            source.maxDistance = _dists[sound];
            source.volume = _vols[sound];
            source.Play();
        }
    }

    // Plays a Sound at the given position
    public static void PlaySound(Sound sound, Vector3 position, bool randomPitch=false)
    {
        if (CanPlay(sound))
        {
            Debug.Log("Playing " + sound + " at " + position);
            GameObject obj = new GameObject("Sound");
            obj.transform.position = position;
            AudioSource source = obj.AddComponent<AudioSource>();
            if (randomPitch)
            {
                source.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            }
            source.spatialBlend = 1.0f;
            source.maxDistance = _dists[sound];
            source.volume = _vols[sound];
            source.rolloffMode = AudioRolloffMode.Custom;
            source.PlayOneShot(GetAudioClip(sound));
            Object.Destroy(obj, GetAudioClip(sound).length);
        }
    }


    // Changes the Background Music to the given Sound
    public static void ChangeBackgroundMusic(Sound sound)
    {
        if (_currentBackground != sound)
        {
            Debug.Log("Changing Background Music to " + sound);
            _currentBackground = sound;
            _backgroundMusic.clip = GetAudioClip(sound);
            _backgroundMusic.volume = _vols[sound];
            _backgroundMusic.Play();
        }
    }

    // Changes the wait for a sound (-1 to set the wait to the audio's length)
    public static void SetWait(Sound sound, float newWait)
    {
        _waits[sound] = newWait != -1 ? newWait : _sounds[sound].length;
    }

    // Changes the volume for a sound
    public static void SetVolume(Sound sound, float newVolume)
    {
        _vols[sound] = newVolume;
    }

    // Returns the volume for a sound
    public static float GetVolume(Sound sound)
    {
        return _vols[sound];
    }

    // Changes the volume of the currentBackground audio source
    public static void ChangeBackgroundVolume(float newVolume)
    {
        _backgroundMusic.volume = newVolume;
    }

    // Checks if a given sound can be played given it's wait time
    private static bool CanPlay(Sound sound)
    {
        float previousPlay = _lastPlayed[sound];
        float wait = _waits[sound];
        if (wait == 0 || (Time.time < wait && previousPlay == 0) || previousPlay + wait < Time.time)
        {
            _lastPlayed[sound] = Time.time;
            return true;
        }
        else
        {
            return false;
        }
    }

    // Returns the audioclip of a given sound
    public static AudioClip GetAudioClip(Sound sound)
    {
        return SoundManager._sounds[sound];
    }
}
