namespace WheelOfFortune.Interfaces
{
    public interface ISpinnable
    {
        /// <param name="duration">Dönüş süresi (saniye)</param>
        void Spin(float duration);

        /// <param name="targetIndex">Hedef dilim index'i</param>
        void Stop(int targetIndex);

        bool IsSpinning { get; }
    }
}
