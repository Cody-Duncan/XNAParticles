ParticleTest2 (Otherwise known as XNAParticles)

Project: ParticleTest2
Author: Cody Duncan
Date: December 28th, 2011
Requirements: VisualStudio2008, XNA 3.1, ShaderModel 3.0
Description: Stateful particle system that includes collission with walls, not with other particles.
References:
http://www.gamasutra.com/view/feature/2122/building_a_millionparticle_system.php
borrows some source:  http://www.catalinzima.com/tutorials/4-uses-of-vtf/particle-systems/
credit for Particle Physics.fx and Particle.fx shaders also goes to Catalin Zima,though several
modifications were made to allow for attractors and gravitation.

Controls:
on keyboard:
1-no gravity
2-downward gravity
3-gravity at center
enter- reset simulation (will start particles on rotary trajectories if gravity is at center)
Left-Shift - toggle increased attractor/repel to cursor strength
Esc - close program

on mouse:
left-click - attract to cursor
right-click - repel from cursor


up,down,left,right,z,x - camera controls, but using these throws off the mapping of clicks to attraction/repel points.
probably need to fix that later, but it's fine for a proof of concept.
inspired by:  http://www.youtube.com/watch?v=ACHJ2rGyP10  (dhscaresme) and  http://www.youtube.com/watch?v=CyAZ2Y7nOTw  (GearGOD)
