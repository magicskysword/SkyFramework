using System;

namespace SkyFrameWork
{
    public static class Generator
    {
        public static string GenerateStringID()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int) b + 1);
            }

            return $"{i - DateTime.Now.Ticks:x}";
        }

        public static long GenerateLongID()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }
    }
}