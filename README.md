# FacadeFE

FacadeFE is a Grasshopper plugin which works on facade Finite element subdivision. It aims to link Rhino environment to a proprietary external FEA engine (as a beam system). And this by generating ASCII data readable by the engine.

Technically speaking, the plugin takes user input in terms of facade panels geometry, profile properties, boundary conditions and loads. It arranges them internally to get one facade system, which has a geometry order optimized for Finite Element calculations (a radial anti-clockwise order of faces, and edges). After that, the facade system goes through a Finite Element subdivision of the system, so that all nodes and finite elements are generated. The distribution of wind loads on each panel is also included, by assigning their respective weighing to corresponding beam elements.

(This is an earlier version implementing the FE subdivision and ASCII syntax generation. The plugin was developed further internally.)

For more information: [link](https://ilyasabdelmoula.com/FacadeFE)
