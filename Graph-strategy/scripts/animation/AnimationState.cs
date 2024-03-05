using Godot;

namespace AnimationHandling
{
	public partial class AnimationState : GodotObject
	{
		/// <summary>
		/// unique name of state
		/// </summary>
		[Export]
		public string name;

		/// <summary>
		/// Name of animation to be played in this state
		/// </summary>
		[Export]
		public string animationName;

		private AnimationHandler handler;

		public AnimationState(AnimationHandler handler, string name, string animationName)
		{
			this.name = name;
			this.animationName = animationName;
			this.handler = handler;
		}
	}
}
