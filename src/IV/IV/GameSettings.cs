using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace IV
{
    public class GameSettings
    {
        public static int SavedWindowWidth;
        public static int SavedWindowHeight;
        public static int WindowWidth;
        public static int WindowHeight;
        public static int LevelIndex;
        public static float SoundFx;
        public static float MusicVol;
        public static bool IsFullScreenMode;

        public int _WindowWidth { get; set; }
        public int _WindowHeight { get; set; }
        public int _LevelIndex { get; set; }
        public float _SoundFx { get; set; }
        public float _MusicVol { get; set; }
        public bool _IsFullScreenMode { get; set; }

        private readonly string folder;

        public GameSettings()
        {
            folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            folder = Path.Combine(folder, "ITG");
            folder = Path.Combine(folder, "IV");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            folder = Path.Combine(folder,"GameSettings.xml");
        }

        public GameSettings DefaultSettings
        {
            get
            {
                return new GameSettings
                           {
                               _IsFullScreenMode = true,
                               _LevelIndex = 0,
                               _MusicVol = 0.149253428f,
                               _SoundFx = 1,
                               _WindowHeight = 720,
                               _WindowWidth = 1280
                           };
            }
        }

        public void Save()
        {
            _WindowWidth = SavedWindowWidth;
            _WindowHeight = SavedWindowHeight;
            _LevelIndex = LevelIndex;
            _SoundFx = SoundFx;
            _MusicVol = MusicVol;
            _IsFullScreenMode = IsFullScreenMode;

            using (Stream stream = File.Create(folder))
            {
                var ser = new XmlSerializer(GetType());
                ser.Serialize(stream, this);
            }

        }

        public  GameSettings Load()
        {
            if (!File.Exists(folder))
                return DefaultSettings;

            using(Stream stream = File.OpenRead(folder))
            {
                try
                {
                    var ser = new XmlSerializer(typeof (GameSettings));
                    return (GameSettings) ser.Deserialize(stream);
                    
                }
                catch (InvalidOperationException)
                {
                    stream.Close();
                    File.Delete(folder);
                    return DefaultSettings;
                }
                
            }
        }

        public void SetSettings()
        {
            if (_WindowWidth != 1024 && _WindowWidth != 1200 && _WindowWidth != 1228 && _WindowWidth != 1280
                && _WindowWidth != 1366 && _WindowWidth != 1440 && _WindowWidth != 1600)
                _WindowWidth = 1280;
            if (_WindowHeight != 768 && _WindowHeight != 900 && _WindowHeight != 720)
                _WindowHeight = 720;
            WindowWidth = SavedWindowWidth = _WindowWidth;
            WindowHeight = SavedWindowHeight = _WindowHeight;
            if (_LevelIndex < 0 || _LevelIndex > 4) _LevelIndex = 0;
            LevelIndex = _LevelIndex;
            _SoundFx = MathHelper.Clamp(_SoundFx, 0, 1);
            SoundFx = _SoundFx;
            _MusicVol = MathHelper.Clamp(_MusicVol, 0, 1);
            MusicVol = _MusicVol;
            IsFullScreenMode = _IsFullScreenMode;

#if DEBUG
            WindowWidth = 1120;
            WindowHeight = 630;
            IsFullScreenMode = false;
#endif
        }
    }
}
