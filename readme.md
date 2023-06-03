# Asspull IIIx
## Tools
This repository contains the full source (for what it's worth) of Kawa's *Asspull IIIx* toolkit. Originally they were all made in Visual C# but every one of them has now been rewritten in Python.

### assfix

This takes an AP3 ROM file, sets the checksum field in the header, and pads it out to the nearest power of two.

```
usage: assfix.py [-h] [-v] inFile [outFile]

Pad an A3X .ap3 ROM file and fix its checksum.

positional arguments:
  inFile         source .ap3 file
  outFile        target .ap3 file

optional arguments:
  -h, --help     show this help message and exit
  -v, --verbose  use verbose output
```

### ass2img

The A3X BIOS has its own image format. This little tool can convert them back to PNG (or GIF, or BMP, if you want to for whatever reason), or merely show them by not specifying an output file.

```
usage: ass2img.py [-h] [-v] inFile [outFile]

Display an A3X .api file or convert it to a regular image.

positional arguments:
  inFile         .api file to display or save
  outFile        if specified, file to save as

optional arguments:
  -h, --help     show this help message and exit
  -v, --verbose  use verbose output
```

### hdma2ass

This takes an image of a top-to-bottom gradient and converts it to an array of xBGR-1555 color values that you can use as HDMA gradient background data. Fair warning, that *does* cause banding. It takes up to three parameters, making assumptions if anything but the first is missing: the input image file, the output `.s` or `.c` file, and what to name the data. Given only a `foo.png`, it'll assume `foo.s` with identifier `foo`.

```
usage: hdma2ass.py [-h] inFile [outFile] [identifier]

Convert a color strip to HMDA color data.

positional arguments:
  inFile      source picture
  outFile     target .s or .c file
  identifier  identifier name

optional arguments:
  -h, --help  show this help message and exit
```

### img2ass

This takes a PNG image (or GIF, or BMP, if want to for whatever reason) of either four or eight bits per pixel and tries to convert it to the A3X BIOS' custom format. Basically  It takes up to three parameters: the input image file, the output `.api` file, and whether or not to skip compression, the `--raw`/`-r` parameter. Again, given only a `foo.png`, it'll assume `foo.api` unless told otherwise.

If a `foo-hN.png` exists, that file will be converted to an HDMA gradient, like with `hdma2ass`, and embedded in the resulting file. _N_ can be any number from 0 to 7, and the gradient will apply to that color index. Use the `--nograds`/`-g` parameter to skip this part.

```
usage: img2ass.py [-h] [-r] [-v] inFile [outFile]

Convert an image to the A3X .api format.

positional arguments:
  inFile         source image file
  outFile        target .api file

optional arguments:
  -h, --help       show this help message and exit
  -r, --raw        skip compression
  -v, --verbose    use verbose output
  -g, --nograds    skip HDMA gradients
  -c, --clipgrads  clip HDMA gradients
```

### mkloc

This takes a JSON file describing a locale and keyboard layout, and converts it to a `.loc` file that you can use on the A3X by storing it on a disk and listing it in a `start.cfg` file on that disk, with a line like `locale=foo.loc`, or by selecting it in the Navigator.

```
usage: mkloc.py [-h] inFile [outFile]

Convert an A3X locale from JSON to binary or vice versa.

positional arguments:
  inFile      source file
  outFile     target .loc or .json file

optional arguments:
  -h, --help  show this help message and exit
```

### tiled2ass

This takes a Tiled map exported to uncompressed JSON and converts it to A3X tilemap format, layer by layer. Only tile layers are supported. Any hidden layers are skipped, and a custom `tileOffset` property can be used to adjust what tile indices are used in the result. Also, to set palette information you can bind a custom `palette` property to entries in the tileset. Tiles can be flipped. The resulting data will use the layer names as identifiers, so you may want to make sure you name them right. This takes two parameters: the input `.json` file and optionally the output `.s` or `.c` file, again assuming if only one is given.

```
usage: tiled2ass.py [-h] inFile [outFile]

Convert a Tiled JSON map to A3X map data.

positional arguments:
  inFile      source file
  outFile     target .s or .c file

optional arguments:
  -h, --help  show this help message and exit
```

