# Asspull IIIx
## Tools
This repository contains the full source (for what it's worth) of Kawa's *Asspull IIIx* toolkit. You can build it in anything Visual Studio 2010 Express or better. Some example data is included.

### assimgview
The A3X BIOS has its own image format. This little tool can show these `.api` files for checking. It takes one parameter, the actual file to show.

### hdma2ass
This takes an image of a top-to-bottom gradient and converts it to an array of xBGR-1555 color values that you can use as HDMA gradient background data. Fair warning, that *does* cause banding. It takes up to three parameters, making assumptions if anything is missing: the input `.png` file, the output `.s` file, and what to name the data. Given only a `foo.png`, it'll assume `foo.s` with identifier `foo`.

### img2ass
This takes a PNG image of either four or eight bits per pixel and tries to convert it to the A3X BIOS' custom format. It takes up to three parameters: the input `.png` file, the output `.api` file, and whether or not to skip compression, the `-raw` parameter. Again, given only a `foo.png`, it'll assume `foo.api` unless told otherwise.

### tiled2ass
This takes a Tiled map exported to uncompressed JSON and converts it to A3X tilemap format, layer by layer. Only tile layers are supported. Any hidden layers are skipped, and a custom `tileOffset` property can be used to adjust what tile indices are used in the result. Also, to set palette information you can bind a custom `palette` property to entries in the tileset. Tiles can be flipped. The resulting data will use the layer names as identifiers, so you may want to make sure you name them right. This takes two parameters: the input `.json` file and optionally the output `.s` file, again assuming if only one is given.
