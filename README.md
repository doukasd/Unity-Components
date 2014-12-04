#Unity-Components
Some random Unity3D stuff that at some point I wish I could have found.

Example projects are provided but the components can also be downloaded as Unity Packages for simpler install and use.

##Contents
###Procedural Cylinder
Creates a cylinder procedurally including UVs based on the desired number of sides and height segments.

###Procedural Plane
Creates a plane procedurally with a specified number of horizontal and vertical segments and uniform UVs.

For each corner of the plane an offset can be specified meaning that corner can be moved. That makes this very useful for digital keystoning, using that plane to hold a render texture of your main camera and tweaking the position of those corners means you can keystone the final output and use on a projector for example for projection mapping.

###Digital Keystone
This is practically an addon to the *Procedural Plane* that allows you to edit the 4 corners in Play Mode and save/load the edits, useful for adjusting the keystone while projecting on a projector.

##License
The content of this repository is licensed under the terms of the Apache License, Version 2.0.