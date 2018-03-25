using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace IV
{
    public class SoundManager
    {
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank sound;
        AudioCategory soundCategory;

        readonly AudioEmitter emitter = new AudioEmitter();
        readonly AudioListener listener = new AudioListener();

        public void LoadContent(ContentManager content)
        {
            audioEngine = new AudioEngine("content\\Audio\\IVSounds.xgs");
            waveBank = new WaveBank(audioEngine, "content\\Audio\\IV Wave.xwb");
            sound = new SoundBank(audioEngine, "content\\Audio\\IV Sound.xsb");

            soundCategory = audioEngine.GetCategory("Default");
        }

        public void StopSound(Cue cue)
        {
            if(cue == null) return;
            cue.Stop(AudioStopOptions.AsAuthored);
        }

        public Cue PlaySound(string soundName)
        {
            var cue = sound.GetCue(soundName);
            cue.Play();
            return cue;
        }

        public Cue Play3DSound(string soundName,Vector3 emitterPosition)
        {
            var cue = sound.GetCue(soundName);  
            emitter.Position = emitterPosition;
            cue.Apply3D(listener, emitter);
          
            cue.Play();
            return cue;
        }

        public void SetListener(Vector3 position)
        {
            listener.Position = position;
        }

        public void Update(GameTime gameTime)
        {
            soundCategory.SetVolume(MathHelper.Clamp(GameSettings.SoundFx, 0, 1));
            audioEngine.Update();
        }
    }
}
