namespace DamageSystem
{
    public static class FriendlyFireManager
    {
        /// <summary>
        /// Determines whether damage should be prevented due to friendly fire.
        /// </summary>
        public static bool ViolatesFriendlyFire(Team attacker, Team target)
        {
            // Neutral can hit and be hit by anyone
            if (attacker == Team.Neutral || target == Team.Neutral)
                return false;

            // Same team can't hit each other
            return attacker == target;
        }
    }
}