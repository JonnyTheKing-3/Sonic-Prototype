# Sonic-Prototype
Unity Prototype for a Sonic-style game

---

### Just a heads up!
This is a **fan-made project** created for fun and learning. It's not affiliated with or endorsed by SEGA in any way.

**Sonic the Hedgehog** and related characters are trademarks of **SEGA Holdings Co., Ltd.**

Everything here is shared for **non-commercial use only** — feel free to explore, play around with it, learn from it, or build your own fan projects. Just please **don’t sell it or use it to make money**.

If you use or modify parts of this project, make sure to **credit everyone listed in the CREDITS document** — and me (Jonathan Garcia Rodriguez). 😊

CONTACT:
Feel free to contact me through my socials below for any questions or if you'd like to contribute:
- Discord: jonnytheking_3 (Preferred)
- GMAIL: jonathangrcrdrgz@gmail.com
- Linkedin: https://www.linkedin.com/in/jonathangrcrdrgz/

DEMO:
You can find the demo in my portfolio linked right below + other projects I've made and am working on :) 
https://jonathangrcrdrgz.wixsite.com/jtkportfolio


CONTROLS:
Currently it's set up like so, but in the editor you can modify everything:

- WASD = movement
- Arrow keys/mouse = camera movement (non-adjustable, but you can modify the code to get your preferred setup!)
- Space = Jump
- P = homing attack
- O = Spindash
- I = Boost
- L = Stomp
- K = Slide
- n = Slow down time
- m = Speed up time

One thing to note though is loop de loops. Just 2 things:
1) the movement direction is based off the forward direction of the loop de loop, just move around in the loop de loop from different camera angles and you'll see what I mean
2) any loop de loop that has some cinematic camera movement works only one appropriatly only from one direction
Just mess around with them and you'll get it. For more info on that, read the loop de loop commits as well as the comments for the code for the camera follow, SonicMovement and loop de loop!

HOW TO USE:

Duplicate the samplescene, and just erase all level design things as well as the cubes (slopes) and spheres. This makes sure that everything is set up approrpriately so you can just start making your level!
Also, if you want to add the camera base for the loop de loop, just add it to any loop de loop as it's 3rd child and assign it's cart/spline to the space in the loopdeloop prefab that's called camera path :)
