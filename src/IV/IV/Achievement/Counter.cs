namespace IV.Achievement
{
    public class Counter : Achievement, ISubscriber<OnEnemyKilled>, ISubscriber<OnLevelAccomplished>,
                           ISubscriber<OnPlayerJump>, ISubscriber<OnPlayerFallDown>, ISubscriber<OnPlayerDie>,
        ISubscriber<OnCeroFired>,ISubscriber<OnRejected>
    {
        public Counter()
        {
            EventAggregator.Instance.Subscribe(this);
        }

        public void OnEvent(OnEnemyKilled enemy)
        {
            DataStoreObject.EnemyKilled++;
        }

        public void OnEvent(OnLevelAccomplished level)
        {
            if (level.Index == 5)
                DataStoreObject.PlayedGame++;
        }

        public void OnEvent(OnPlayerJump jump)
        {
            if (jump.IsNormalJump) DataStoreObject.JumpCount++;
            else DataStoreObject.SuperJumpCount++;
        }

        public void OnEvent(OnPlayerFallDown e)
        {
            DataStoreObject.FallDownCount++;
        }

        public void OnEvent(OnPlayerDie player)
        {
            DataStoreObject.PlayerDeath++;
        }

        public void OnEvent(OnCeroFired e)
        {
            DataStoreObject.CeroCount++;
        }

        public void OnEvent(OnRejected e)
        {
            DataStoreObject.RejectedCounter++;
        }
    }
}