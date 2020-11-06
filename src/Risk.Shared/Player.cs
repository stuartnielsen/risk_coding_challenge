namespace Risk.Shared
{
    public class Player
    {
        public Player()
        {
        }

        public Player(string name, string token, string callbackAddress)
        {
            Name = name;
            Token = token;
            CallbackAddress = callbackAddress;
        }

        public string Name { get; set; }
        public string Token { get; set; }
        public string CallbackAddress { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Token})";
        }
    }
}