namespace FsmSystem
{
    /// <summary>
    /// Defines the lifecycle contract for a state bound to a specific Machine type.
    /// </summary>
    /// <typeparam name="TMachine">The Machine type this state belongs to.</typeparam>
    public interface IState<TMachine>
        where TMachine : Machine<TMachine>
    {
        /// <summary>
        /// Called exactly once when the Machine enters this state.
        /// </summary>
        void Start();

        /// <summary>
        /// Called once per Machine.Update() while this state is the current state.
        /// </summary>
        void Update();

        /// <summary>
        /// Called once per Machine.FixedUpdate() while this state is the current state.
        /// </summary>
        void FixedUpdate();

        /// <summary>
        /// Called exactly once when the Machine exits this state.
        /// </summary>
        void End();
    }
}
