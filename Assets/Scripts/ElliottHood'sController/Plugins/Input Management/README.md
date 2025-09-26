# Input System

An extension of Unity's new Input System that allows for easy integration of scriptable input behaviours and transferring control between systems

## Features

- Input Receivers
- One-line input buffering
    - Ex. if (canJump) Jump.TryUseBuffer()
- Scriptable input behaviors, such as 
    - Charge Buffering (automatically completing a charge when its almost fully complete)
    - Double Tap detection
- Easily filtering input through different game systems

![Demo](./GithubResources/Demo1.gif)

## References

- Inspired by Aarthificial: 

[![Stealing Input From The Player](https://img.youtube.com/vi/pOEyYwKtHJo/0.jpg)](https://www.youtube.com/watch?v=pOEyYwKtHJo)

## Dependencies

- Unity's Input System version 1.7.0 or higher

## Installation

### Using Unity Package Manager

1. Open your Unity project.
2. Go to **Window > Package Manager**.
3. Click the **+** button in the top-left corner.
4. Select **Add package from git URL...**.
5. Enter the following URL: https://github.com/ElliottHood/Input-Management.git