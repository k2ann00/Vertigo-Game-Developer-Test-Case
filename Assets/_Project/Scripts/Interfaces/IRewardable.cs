using WheelOfFortune.Data;

namespace WheelOfFortune.Interfaces
{
    public interface IRewardable
    {
        /// <param name="reward">Toplanacak ödül</param>
        void CollectReward(RewardData reward);

        void LoseAllRewards();

        int TotalRewardsCollected { get; }
    }
}
