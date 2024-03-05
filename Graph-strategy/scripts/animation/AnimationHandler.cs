using System.Collections.Generic;
using System.Linq;
using Godot;

namespace AnimationHandling
{
	public partial class AnimationHandler : AnimatedSprite2D
	{
		// Handler properties
		private readonly Dictionary<StringName, AnimationState> states = new();
		private float horizontalOffset = 0f;
		internal AnimationState currState;

		public bool HorizontalFlip
		{
			get => FlipH;
			set
			{
				FlipH = value;
				Vector2 offset = Offset;
				offset.X = value?(-horizontalOffset):horizontalOffset;
				Offset = offset;
			}
		}


		public override void _Ready()
		{
			base._Ready();

			horizontalOffset = Offset.X;
		}

		/// <summary>
		/// Add a state with given name and given name for animation
		/// </summary>
		/// <param name="stateName"></param>
		/// <param name="animationName"></param>
		public AnimationState AddState(StringName stateName, string animationName)
		{
			if (states.ContainsKey(stateName))
			{
				return null;
			}
			if (!SpriteFrames.GetAnimationNames().Contains(animationName))
			{
				return null;
			}

			AnimationState newState = new(this, stateName, animationName);
			states[stateName] = newState;
			currState ??= newState;


			return newState;
		}
		internal void Transition(AnimationState state)
		{
			if (states.ContainsKey(state.name) && currState != state)
			{
				currState = state;
				Play();
			}
		}

		/// <summary>
		/// Play animation in the current state
		/// </summary>
		public void Play()
		{
			if (currState == null)
			{
				return;
			}
			Play(currState.animationName);
		}

		/// <summary>
		/// Flip sprite
		/// </summary>
		public void Flip() => FlipH = !FlipH;

	}
}
