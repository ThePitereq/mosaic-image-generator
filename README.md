# mosaic-image-generator

## CREATED BY
> Kuształa Kamil

> Ochowiak Kinga

> Olejniczak Piotr

## MAIN FUNCTIONS

**Mosaic Image Generator** allows to generate images from smaller ones making it look-loke mosaic.

Photo generator from smaller elements, in this case textures from Minecraft. The program supports any 16x16 texture. For other sizes, minor code changes would be required.

![Input](https://images.pvrust.eu/other/color_input.png)
![Output](https://images.pvrust.eu/other/color_output.png)

The program initially searches for textures closest to the color of a given pixel and then uses the previously learned ones to generate new images.

With further learning of the program, the time needed to generate photos can be reduced even by 5 times.

The program has several QoL functions such as scaling, rotating, generating to the photo (in the assumption of the project, the key is to generate the HTML code with the appearance, because the entire image generation code is much more resource-intensive than generating textures corresponding to the colors)
