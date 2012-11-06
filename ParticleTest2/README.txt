===== Welcome to Particle Simulator =====
Project: ParticleTest2
Author: Cody Duncan
Date: 
    December 28th, 2011 v1.0
        Intial Project Development
    November 3rd 2012   v1.1
        - Fixed redundant matrix calculations in physics simulation pixel shader.
        - Added ability to toggle the core gravity mode to reset particles with
            initial velocities that make them spiral or orbit.
        - Added Command Line arguments to adjust particle count, pull strength,
            and core gravity constants.
        - Massively Increased performance by switching particle texture with a smaller texture.
        - Added more notes in code.
        - Updated Readme
Requirements: Visual Studio 2008, XNA 3.1, ShaderModel 3.0 on Graphics Card
Description: Stateful particle system that includes collision with walls, not with other particles.

References:
http://www.gamasutra.com/view/feature/2122/building_a_millionparticle_system.php
borrows some source:  http://www.catalinzima.com/tutorials/4-uses-of-vtf/particle-systems/
credit for Particle Physics.fx and Particle.fx shaders also goes to Catalin Zima,though several
modifications were made to allow for attractors and gravitation.

inspired by:  http://www.youtube.com/watch?v=ACHJ2rGyP10  (dhscaresme) and  http://www.youtube.com/watch?v=CyAZ2Y7nOTw  (GearGOD)

===== Help =====
using command line argument 
--help
will print much of the information below to the console.

===== Command Line Arguments ===== 
To use args:
     description -> example format  
   dash  before argName, value in next arg  -> -arg value
   slash before argName, value in next arg  -> /arg value 
   colon  after Argname, followed by value  -> -arg:value 
   equals after Argname, followed by value  -> -arg=value 
   Argnames can be full name or first letter, noted in 
   Args section of each parameter listed below. 

   particleCount: particleCount^2 is the number of particles rendered.
        Default: 512 (262144 total particles) 
        Args:   -particles /particles -p /p 

   pullStrength : Power of Mouse-click gravity(left)/antigravity(right).
        Default: 100 
        Note: Pull power weakens linearly over greater distance, rather 
              than exponentially, like gravity  
        Args:   -mousePull /mousePull -m /m 

   CoreGravity  : Power of Gravity in Center of space (mode 3).
        Default: 45000  
        Args:   -gravity /gravity -g /g  

===== Controls ===== 
Modes (number corresponds to key to press)
   1: No Gravity
      press 1 to set to this mode  

   2: Downward Gravity: gravity pulls down to bottom of window
      press 2 to set to this mode 

   3: Core Gravity: gravity pulls toward center of window 
      press 3 to set to this mode 

Keyboard Controls 
   Enter       - Reset particles
   Shift(left) - Toggle mouse gravity power
                  Default: Normal
                  Normal: Pull power = pullStrength(default 100)
                  Boost : Pull power = 5 * pullStrength, for extra pull
   4           - Toggle Orbit/Spiral on Core Gravity Reset
                  Default: Spiral
                  Spiral - particles slowly spiral toward center.
                  Orbit  - particles orbit center, don't get closer or futher
                  Note: This change shows when you reset the particles,
                  and only on Core Gravity Mode. Example:
                    1. hit 3 to get to core gravity mode
                    2. hit Enter to reset particles to spiral velocities
                    3. hit 4 toggle to orbital reset velocities
                    4. hit Enter to reset with orbital velocities
                    5. hit 4 then Enter to toggle back to spiral velocities
Mouse Controls
   Left-click  - attract to cursor
   Right-click - repel from cursor

Camera Controls
   up,down,left,right,z,x 
      Note: using these throws off the mapping of clicks to attraction/repel points.
            probably need to fix that later, but it's fine for a proof of concept.
