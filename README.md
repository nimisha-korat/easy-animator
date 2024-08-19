# Easy Animator

Easy Animator is a powerful and flexible Unity animation tool designed to simplify the process of playing, managing, and transitioning animations in your Unity projects. With a focus on ease of use and high performance, Easy Animator offers a wide range of features that cater to both simple and complex animation needs.

## Features

### 1. Quick Play
Easily play animations on-demand without any unnecessary setup steps. Simply obtain an `AnimationClip` and call `easyAnimator.Play(clip)`.

### 2. Easy Waiting
Register an End Event or use `yield return` with an `EasyAnimatorState` inside a Coroutine to wait for an animation to finish.

### 3. Smooth Transitions
Blend between animations over time, either linearly or using a custom curve, to ensure smooth and natural character movements.

### 4. Flexible Structure
Organize your animations using data structures like arrays and Scriptable Objects, avoiding the use of magic strings.

### 5. Live Inspector
View real-time details of your animations in the Unity Inspector, complete with manual controls for debugging and testing.

### 6. Finite State Machines
Easy Animator includes a flexible FSM system that is entirely separate from the animation system. They integrate seamlessly, but you can easily modify or replace the FSM with your own system if preferred.

### 7. High Performance
Easy Animator is optimized for performance, offering efficiency comparable to or better than other animation systems, with minimal impact on your project.

### 8. Smooth Integration
Integrate smoothly with other Unity plugins and features like Humanoid Animation Retargeting, Generic Rigs, Sprite animations, Root Motion, Animation Events, and Inverse Kinematics (IK).

### 9. Total Control
Gain full access to and control over all animation details in runtime scripts, including speed, time, and blend weight.

### 10. Simple Configuration
Manage animation details in the Unity Inspector, allowing them to be edited as part of a scene or prefab.

### 11. Custom Events
Register custom event callbacks to trigger at specific times during an animation, offering more control than Unity's standard Animation Events.

### 12. Animation Layers
Manage multiple animation sets simultaneously, typically on different body parts. Layers can either override or add to each other, and you can fade them in and out just like individual animations.

### 13. Animator Controllers
While Easy Animator does not require Animator Controllers, it supports a hybrid approach, allowing you to use them alongside direct `AnimationClip` references. You can even mix multiple Animator Controllers on a single character.

### 14. Animation Mixers
Blend between animations based on any input parameter, similar to Blend Trees. For example, blend between Idle, Walk, and Run animations based on the player's joystick tilt, enabling movement at any speed.

### 15. Tools
Use various utilities to create and modify animations, which can be played with Easy Animator or any other compatible animation system. These tools include bulk renaming Sprites or generating animations from Sprites based on their names.

### 16. Customization
Leverage Unity's Animation Job system for low-level access to the animation stream. Create custom state types to implement procedural animation, custom blending algorithms, or any other behavior you can imagine.

### 17. Source Code
The full source code of Easy Animator is included as plain C# files with detailed comments. This allows you to understand the internal workings, fix bugs, make modifications, and avoid dependency on the developer.

## Getting Started

1. Import the Easy Animator package into your Unity project.
2. Refer to the documentation for setup instructions and usage examples.
3. Explore the example scenes provided to see Easy Animator in action.

## License

Easy Animator is released under the MIT License, making it free for both personal and commercial use. See the [LICENSE.md](LICENSE.md) file for details.

## Support

If you encounter any issues or have any questions, please feel free to reach out through the community forums or the project’s GitHub repository.

