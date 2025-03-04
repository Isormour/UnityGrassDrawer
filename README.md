# 📌 Grass Indirect Renderer Tool

![Main Banner](GrassRenderer.gif)

Tool for unity that allows to paint grass on multiple objects and uses indirect rendering.

## 📸 Demo

![Main Banner](GrassRendererDraw.gif)

- :bug: Known Issues
- Scene that you want to have grass on, needs to be saved and seleced as main.
- otherwise program cant find proper path.
## 🚀 Functions

- ✅ Allows you to easly paint grass.
- ✅ Lets you create custom grass material.
- ✅ Creates object with data for specific gameObject on Scene.
- ✅ Culls grasses that are out of viewport
-
- TODO:
-
- 1 Object managment.
- Find solution to better manage large amount(milions) of grass instances. 
- Right now they are renderered fast,but modification(adding/removing) on one huge array is bad approach.
-  
- 2 QOL
- Rewrite position sampling so it could include many objects at the same time.
- right now ony one object is selected and can be painted.
