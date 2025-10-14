# Tactical Battles Documentation

This is for briefly explaining concepts in the Tactical Battles that aren't easily inferred from looking at code.
Mostly to give the AI context for developing code.

### The Grid

The battle grid is represented isometrically at a 45 degree angle to the player. 

The following example will be used to help explain.
 (0,0) (1,0) (2,0)
 (0,1) (1,1) (2,1)
 (0,2) (1,2) (2,2)

Pawn "facing" refers to direction on a compass - this is used to help determine if a pawn is being flanked, and which direction the pawn's attacks will point.

Let's say there's a Pawn at (1,1). We consider:
 - NE to be (2,1)
 - SE to be (1,2)
 - SW to be (0,1)
 - NW to be (1,0) 