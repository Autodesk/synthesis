Tween for Unity
(c) 2016 Digital Ruby, LLC
https://www.digitalruby.com/unity-plugins/
Created by Jeff Johnson

Version 1.0.4

Tween for Unity is the easiest and simplest Tween script for Unity. In a matter of seconds you can be tweening and animating your game objects.

Tween supports float, Vector2, Vector3, Vector4 and Quaternion tweens.

TweenFactory is the class you will want to use to initiate tweens. There is no need to add any scripts to game objects. TweenFactory takes care of everything.

Simply call TweenFactory.Tween(...) and pass in your parameters and callback functions.

TweenFactory.DefaultTimeFunc can be set to your desired time function, default is Time.deltaTime.

Tweens may have a key, or null for no key. If adding a tween with a non-null key, existing tweens with the same key will be removed. Use the AddKeyStopBehavior field of TweenFactory to determine what to do in these cases.

Set Tween.ForceUpdate = true; if you want Tween to continue to run on objects that are not visible.

Make sure to add a "using DigitalRuby.Tween" to your scripts.

See TweenDemoScene for a demo scene, and look in TweenDemo.cs for code samples.