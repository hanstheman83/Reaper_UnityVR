# Reaper_UnityVR
1. Control Reaper with Oculus Quest/Link
2. VR art school [Native Quest1 and Quest2] (will eventually get its own repo...)



VR ART SCHOOL
Build 21 Release 2 :
The app runs perfect on Quest2 and a bit laggy on Quest1. 
You can change colors with 3 color swatches. 
Remember that grapping the pencil is a toggle action - you don't need to press the grab button continously!
On Quest2 : you can snap the drawing canvas to the controller on left controller joystick click - 
put the flat part of the controller to a real world flat surface and use this button to auto-align 
the VR drawing canvas with a flat surface(stable drawing board or table)

Known issues : 
You can see a bit of pixel artifacts at the seems [16 render textures tiled].
In the Unity Editor only 4 render textures are active - due to DirectX11 limitaions.
Don't activate the main UI with the primary button on left controller - it is not used. 
