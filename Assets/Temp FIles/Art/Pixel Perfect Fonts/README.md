# Pixel Perfect Fonts

# References:
Followed this tutorial: 
https://www.youtube.com/watch?v=Ov2XR81oVcI (NO LONGER WORKS IN UNITY 6.0 FOR SOME REASON)

# Changing the Asset: (NO LONGER WORKS IN UNITY 6.0 FOR SOME REASON)
- Edit the png file in a program like Aseprite
- Use this website: https://yal.cc/r/20/pixelfont/ and import the new file
    - Make sure all characters are spaced completely to the left of their box for accurate auto spacing!!
    - If you want to increase spacing use this in the 'Rules & Overrides' field: 
        - right \u0020 1
    - Adjust settings to remove padding and separation. (For 6x7 Regule5, the width is 6 and height is 7)
- Click Save TTF, rename the file, and move it into to unity
- Inside Unity, change the the font import settings to
    - Font Size: Either the Height, or Width whichever is larger
    - Rendering Mode: Hinted Raster
    - Ascent Calculation Mode: Face Ascender Metric
    - Character: Custom Set:
        - !"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~∎
- Then go to Window -> TextMeshPro -> Font Asset Creator, and use these settings:
    - Source Font File: your source font file
    - Sampling Point Size: *TTF Font Size rounded up to the nearest power of 2
    - Padding: 1
    - Packing Method Optimum
    - Character Set: Unicode Range (Hex)
    - Render Mode: RASTER_HINTED
- Click Generate Font Atlas
- Then Click Save As... and you should get a functioning Pixel-Perfect TextMeshPro font asset
- You can safely delete the TTF afterwards

# For use in Asesprite use this extension:
https://github.com/fletchmakes/font-custom
(json files are attached)