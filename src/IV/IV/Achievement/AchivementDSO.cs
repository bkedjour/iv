using System.ComponentModel;

namespace IV.Achievement
{
    public class AchivementDSO : INotifyPropertyChanged
    {
        private int _enemyKilled;

        public int EnemyKilled
        {
            get { return _enemyKilled; }
            set
            {
                _enemyKilled = value;
                OnPropertyChanged("EnemyKilled");
            }
        }

        private int _playedGame;

        public int PlayedGame
        {
            get { return _playedGame; }
            set
            {
                _playedGame = value;
                OnPropertyChanged("PlayedGame");
            }
        }

        private int _jumpCount;

        public int JumpCount
        {
            get { return _jumpCount; }
            set
            {
                _jumpCount = value;
                OnPropertyChanged("JumpCount");
            }
        }

        private int _superJumpCount;

        public int SuperJumpCount
        {
            get { return _superJumpCount; }
            set
            {
                _superJumpCount = value;
                OnPropertyChanged("SuperJumpCount");
            }
        }

        private int _playerDeath;

        public int PlayerDeath
        {
            get { return _playerDeath; }
            set
            {
                _playerDeath = value;
                OnPropertyChanged("PlayerDeath");
            }
        }

        private int _ceroCount;

        public int CeroCount
        {
            get { return _ceroCount; }
            set
            {
                _ceroCount = value;
                OnPropertyChanged("CeroCount");
            }
        }

        private int _rejectedCounter;

        public int RejectedCounter
        {
            get { return _rejectedCounter; }
            set
            {
                _rejectedCounter = value;
                OnPropertyChanged("RejectedCounter");
            }
        }

        public int FallDownCount { get; set; }

        public bool ThirtyEnemiesKilledUnlocked { get; set; }
        public bool FinishGameAccomplished { get; set; }
        public bool PlayGameWithoutDying { get; set; }
        public bool PlayGameWithoutKilling { get; set; }
        public bool Jump100Accomplished { get; set; }
        public bool FallDownAccomplished { get; set; }
        public bool SuperJumpAccomplished { get; set; }
        public bool Die42TimeAccomplished { get; set; }
        public bool Cero100Accomplished { get; set; }
        public bool ZeroEnemiesAccomplished { get; set; }
        public bool IdleFor30SecAccomplished { get; set; }
        public bool Rejected3TimesAccomplished { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}