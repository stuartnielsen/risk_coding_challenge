namespace Risk.Shared
{
    public class Player
    {
        public Player()
        {
        }

        public Player(string name, string token)
        {
            Name = name;
            Token = token;
        }

        public string Name { get; set; }
        public string Token { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Token})";
        }
    }
}