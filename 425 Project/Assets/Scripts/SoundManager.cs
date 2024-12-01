using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        PlayerLowHP,        // FUNCTIONAL
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
        LowHPBackground,    // FUNCTIONAL
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
        public Sound sound;
        public AudioClip clip;
        public float volume = 1;
        public float maxDistance = 10;
        public float wait = -1;  // set to -1 to be set to the length of the audioclip
    }

    public SoundComponents[] soundComponents;

    private static GameObject Player;
    private static Sound currentBackground;
    private static AudioSource backgroundMusic;
    private static Dictionary<Sound, AudioClip> sounds = new Dictionary<Sound, AudioClip>();
    private static Dictionary<Sound, float> waits = new Dictionary<Sound, float>();
    private static Dictionary<Sound, float> vols = new Dictionary<Sound, float>();
    private static Dictionary<Sound, float> dists = new Dictionary<Sound, float>();
    private static Dictionary<Sound, float> lastPlayed = new Dictionary<Sound, float>();

    private void Start()
    {
        Player = GameObject.Find("PlayerModel");
        foreach(SoundComponents soundComponent in soundComponents)
        {
            sounds.Add(soundComponent.sound, soundComponent.clip);
            waits.Add(soundComponent.sound, soundComponent.wait != -1 ? soundComponent.wait : soundComponent.clip.length);
            vols.Add(soundComponent.sound, soundComponent.volume);
            dists.Add(soundComponent.sound, soundComponent.maxDistance);
            lastPlayed.Add(soundComponent.sound, 0f);
        }
        backgroundMusic = Player.AddComponent<AudioSource>();
        backgroundMusic.loop = true;
        ChangeBackgroundMusic(SoundManager.Sound.NormalBackground);
    }

    // Plays a Sound on the Player
    public static void PlaySound(Sound sound)
    {
        PlaySound(sound, Player);
    }

    // Plays a Sound on the given GameObject
    public static void PlaySound(Sound sound, GameObject obj, bool randomPitch = false)
    {
        Vector3 position = obj.transform.position;
        if (canPlay(sound))
        {
            Debug.Log("Playing " + sound + " on " + obj.name);
            AudioSource source = Player.AddComponent<AudioSource>();
            if (randomPitch)
            {
                source.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            }
            source.spatialBlend = 1.0f;
            source.maxDistance = dists[sound];
            source.volume = vols[sound];
            source.rolloffMode = AudioRolloffMode.Custom;
            source.PlayOneShot(getAudioClip(sound));
            Destroy(source, getAudioClip(sound).length);
        }
    }

    // Plays a Sound on the given AudioSource
    public static void PlaySound(Sound sound, AudioSource source, bool randomPitch = false)
    {
        if (canPlay(sound) && !source.isPlaying)
        {
            if (randomPitch)
            {
                source.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            }
            Debug.Log("Playing " + sound + " on given source.");
            source.clip = getAudioClip(sound);
            source.maxDistance = dists[sound];
            source.volume = vols[sound];
            source.Play();
        }
    }

    // Plays a Sound at the given position
    public static void PlaySound(Sound sound, Vector3 position, bool randomPitch=false)
    {
        if (canPlay(sound))
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
            source.maxDistance = dists[sound];
            source.volume = vols[sound];
            source.rolloffMode = AudioRolloffMode.Custom;
            source.PlayOneShot(getAudioClip(sound));
            Object.Destroy(obj, getAudioClip(sound).length);
        }
    }


    // Changes the Background Music to the given Sound
    public static void ChangeBackgroundMusic(Sound sound)
    {
        if (currentBackground != sound)
        {
            Debug.Log("Changing Background Music to " + sound);
            currentBackground = sound;
            backgroundMusic.clip = getAudioClip(sound);
            backgroundMusic.volume = vols[sound];
            backgroundMusic.Play();
        }
    }

    // Changes the wait for a sound (-1 to set the wait to the audio's length)
    public static void SetWait(Sound sound, float newWait)
    {
        waits[sound] = newWait != -1 ? newWait : sounds[sound].length;
    }

    // Changes the volume for a sound
    public static void SetVolume(Sound sound, float newVolume)
    {
        vols[sound] = newVolume;
    }

    // Returns the volume for a sound
    public static float GetVolume(Sound sound)
    {
        return vols[sound];
    }

    // Changes the volume of the currentBackground audio source
    public static void ChangeBackgroundVolume(float newVolume)
    {
        backgroundMusic.volume = newVolume;
    }

    // Checks if a given sound can be played given it's wait time
    private static bool canPlay(Sound sound)
    {
        float previousPlay = lastPlayed[sound];
        float wait = waits[sound];
        if (wait == 0 || (Time.time < wait && previousPlay == 0) || previousPlay + wait < Time.time)
        {
            lastPlayed[sound] = Time.time;
            return true;
        }
        else
        {
            return false;
        }
    }

    // Returns the audioclip of a given sound
    public static AudioClip getAudioClip(Sound sound)
    {
        return SoundManager.sounds[sound];
    }
}
