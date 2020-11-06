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
        public Player(string name, string token, string callback)
        {
            Name = name;
            Token = token;
            CallbackBaseAddress = callback;
        }

        public string Name { get; set; }
        public string Token { get; set; }
        //address of player
        public string CallbackBaseAddress { get; set; }

        //deployment status
        public DeploymentStatus deploymentStatus { get; set; }
  
        public override string ToString()
        {
            return $"{Name} ({Token})";
        }
    }
}