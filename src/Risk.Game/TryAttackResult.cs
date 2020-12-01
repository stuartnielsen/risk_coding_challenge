namespace Risk.Game
{
    public class TryAttackResult
    {
        public bool AttackInvalid { get; set; }
        public bool CanContinue { get; set; }
        public bool BattleWasWon { get; set; }
        public string Message { get; set; }
    }
}