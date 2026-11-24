namespace IGOR.Games.Morris
{
    /// <summary>
    /// Possible game phases.
    /// </summary>
    public enum GamePhase
    {
        /// <summary>
        /// Players can set a stone to any free cell
        /// </summary>
        Placing,

        /// <summary>
        /// Players can move one of their stones on a free adjacent cell
        /// </summary>
        Moving,

        /// <summary>
        /// Players can move one of their stones on any free cell
        /// </summary>
        Jumping
    }
}
