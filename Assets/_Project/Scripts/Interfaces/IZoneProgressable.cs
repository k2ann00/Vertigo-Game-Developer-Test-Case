using WheelOfFortune.Data;

namespace WheelOfFortune.Interfaces
{
    public interface IZoneProgressable
    {
        void NextZone();

        ZoneData GetCurrentZone();

        /// <param name="zoneNumber">Hedef zone numarası</param>
        void JumpToZone(int zoneNumber);

        void ResetZones();

        int CurrentZoneNumber { get; }
    }
}
