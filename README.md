# Reaper_UnityVR
1. Control Reaper with Oculus Quest/Link
2. VR art school [Native Quest1 and Quest2] (will eventually get its own repo...)



VR ART SCHOOL
Build 23 Release 3 : 
The app is now using 4x5 render textures, each has a resolution of 512x512. The quest2 could easily run 1024x1024 per render texture!
Added changing size of pencil on trigger button. The 3 color swatches has sliders for changing HSV.
Added a simple decimated sculpt to draw.
Added post processing light effects.

Hint : 
On Quest2 : you can snap the drawing canvas to the controller on left controller joystick click - 
put the flat part of the controller to a real world flat surface and use this button to auto-align 
the VR drawing canvas with a flat surface(stable drawing board or table). The Quest1 controllers are not alligned in the same way so won't work well.

Known issues : 
You can see a bit of pixel artifacts at the seems [16 render textures tiled].
In the Unity Editor only 4 render textures are active - due to DirectX11 limitaions.
Don't activate the main UI with the primary button on left controller - it is not used. 

