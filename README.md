# Rockstar feat C#
This is an experiment of using source-code generators to compile Rockstar directly from within your C# code-base.
Check out https://github.com/dylanbeattie/rockstar for more information about this awesome language.

## Usage

Using Rockstar is as easy as adding the [NuGet package](http://nuget.org/packages/Rockstar.Generator). 

Write your Rockstar code directly on your C# code-base inside a comment block:

`````csharp

/* Let's rock with FizzBuzz!!!

   Midnight takes your heart and your soul
   While your heart is as high as your soul
   Put your heart without your soul into your heart

   Give back your heart


   Desire is a lovestruck ladykiller
   My world is nothing 
   Fire is ice
   Hate is water
   Until my world is Desire,
   Build my world up
   If Midnight taking my world, Fire is nothing and Midnight taking my world, Hate is nothing
   Shout "FizzBuzz!"
   Take it to the top

   If Midnight taking my world, Fire is nothing
   Shout "Fizz!"
   Take it to the top

   If Midnight taking my world, Hate is nothing
   Say "Buzz!"
   Take it to the top

   Whisper my world
*/

Rockstar.FizzBuzz.LetsRock();

`````

## Status
A few things are still missing from the code - arrays manipulation and string splitting being the main ones - but otherwise **it just works**.

Feel free to open an issue or a PR if you see something broken!






