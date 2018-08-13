# ModelPacker

## About

This program makes it easy to create a texture atlas from multiple models. It does this by first packing the textures in a sensible way and then moving the uv coordinates of the corresponding model with it. You can also choose to merge all the meshes into 1 big mesh, this won't affect the positions of the vertices so you might have to place the pivot in the correct place beforehand.

This program makes heavy use of ImageMagick and Assimp. For that reason you can find the supported image formats [here](https://www.imagemagick.org/script/formats.php) and the supported model formats [here](https://github.com/assimp/assimp#supported-file-formats).



## Usage

You can drag & drop your model files, textures, and settings file into the UI window. The order in which the models and textures are listed matters, in the UI they are sorted by name. The texture and model are selected by index thus the first texture should fit the first model, the second texture the second model and so forth. 

By default it will generate a settings file after you have exported. You can load this settings file again and it will populate all the fields with the settings you had when you exported.

You can also choose to add padding to the textures, this will help mitigate bleeding issues you otherwise can get when the game engine switches to a lower mipmap. This value is based on pixels (a value of 10 will add 10 pixels of padding to each texture, thus resulting in a 20 pixels distance between 2 textures). The color of the padding is determined by the neighboring pixel, the same result as if the texture was set to [clamp](https://docs.microsoft.com/en-us/windows/desktop/direct3d9/clamp-texture-address-mode) in a rendering engine. 



## Building from source

After you have cloned this project, you have to install the sub module. You can do this by running the following command:

```git submodule update --init --recursive```

After that you can open the project in your favorite IDE (rider recommended) and make sure the NuGet packages are installed. After the projects are loaded, you should first build the `AssimpNet.Interop.Generator` project. After that you can either build the `ModelPacker.UI` or `ModelPacker.CMD` project depending on your preference. You can find the built files in the bin folder of the selected project.

Building and running on Linux and OSX should work although it is only tested on Ubuntu 16.04. You will have to install mono to be able to compile and run the program. It is required to install the latest mono as instructed [here](https://www.mono-project.com/download/stable/#download-lin), otherwise you won't be able to compile and run .net 4.5.2. It won't be possible to build and/or run the `ModelPacker.UI` project since mono does not support WPF. Because of this, you might have to right click on the `ModelPacker.CMD` project and select build there.
