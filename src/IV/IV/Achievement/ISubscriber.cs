using IV.Action_Scene.Enemies;

namespace IV.Achievement
{
    public interface ISubscriber<T>
    {
        void OnEvent(T e);
    }

    public class OnEnemyKilled
    {
        public Enemy Enemy { get; set; }
    }

    public class OnLevelAccomplished
    {
        public int Index { get; set; }
    }

    public class OnLevelStarted
    {
        public int Index { get; set; }
    }

    public class OnPlayerDie
    {
    }

    public class OnPlayerJump
    {
        public bool IsNormalJump { get; set; }
    }

    public class OnPlayerFallDown
    {
        
    }

    public class OnCeroFired
    {
        
    }

    public class OnZeroEnemy
    {
        
    }

    public class OnIdle
    {
        
    }

    public class OnRejected
    {
    }
}