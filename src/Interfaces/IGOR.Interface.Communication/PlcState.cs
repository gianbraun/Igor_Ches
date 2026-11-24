namespace IGOR.Interface.Communication
{
    /// <summary>
    /// Status of the sps 
    /// </summary>
    public enum PlcState
    {
        /// <summary>
        /// SPS is in invalid State
        /// </summary>
        Invalid,

        /// <summary>
        /// SPS is initialzing
        /// </summary>
        Init,

        /// <summary>
        /// SPS is in config mode
        /// </summary>
        Config,

        /// <summary>
        /// SPS is starting
        /// </summary>
        Start,

        /// <summary>
        /// SPS is running
        /// </summary>
        Run,

        /// <summary>
        /// SPS is stoped
        /// </summary>
        Stop,

        /// <summary>
        /// SPS is in error State
        /// </summary>
        Error
    }
}
