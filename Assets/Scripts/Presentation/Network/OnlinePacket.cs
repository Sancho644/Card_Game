namespace Game.Presentation.Network
{
    public sealed class OnlinePacket
    {
        public string Kind;
        public string Fingerprint;
        public string[] Deck;
        public bool Ready;
        public bool HostReady;
        public bool GuestReady;
        public bool HasGuest;
        public int Version;
        public long Request;
        public int ActionIndex;
        public OnlineFrame Frame;
        public string ErrorKey;
    }
}
