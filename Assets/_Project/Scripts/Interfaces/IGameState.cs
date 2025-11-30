using WheelOfFortune.Data;

namespace WheelOfFortune.Interfaces
{
    public interface IGameState
    {
        /// <param name="newState">Yeni state</param>
        void ChangeState(GameState newState);

        GameState GetCurrentState();

        bool CanTransitionTo(GameState targetState);
    }
}
