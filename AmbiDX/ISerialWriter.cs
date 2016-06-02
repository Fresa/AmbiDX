using System;

namespace AmbiDX
{
    public interface ISerialWriter : IDisposable
    {
        void Write(byte[] bytes);
    }
}