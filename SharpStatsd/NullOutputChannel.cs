﻿namespace SharpStatsd
{
    public sealed class NullOutputChannel : IOutputChannel
    {
        public void Send(byte[] buffer, int length)
        {
            // noop
        }
    }
}