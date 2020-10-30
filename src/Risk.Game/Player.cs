namespace Risk.Game
{
    public class Player
    {
        public Player(string name, string token)
        {
            Name = name;
            Token = token;
        }

        public string Name { get; }
        public string Token { get; }

        public override string ToString()
        {
            return $"{Name} ({Token})";
        }
    }
}