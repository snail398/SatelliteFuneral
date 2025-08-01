using Unity.Mathematics;

namespace Shared
{
    public static class TimestampExtensions {
        public static int TimeStampsAbsIncrement(int last, int next) {
            if (last > 0 && next < 0) //to prevent int overflow
                return (int.MaxValue - last) + (next - int.MinValue);
            return math.abs(next - last);
        }
        
        public static int TimeStampsDelta(int last, int next) {
            unchecked {
                if (last > 0 && next < 0) //to prevent int overflow
                    return (int.MaxValue - last) + (next - int.MinValue);
                if (next > 0 && last < 0) //to prevent int overflow
                    return (int.MaxValue - next) + (last - int.MinValue);
                return next - last;
            }
        }
        
        public static int CalculateNextTimeStamp(int baseTimeStamp, int delta) {
            int result;
            unchecked {
                result = baseTimeStamp + delta;
            }
            return result;
        }
    }
}