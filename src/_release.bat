set VER=023
del www\cubes%VER%.zip
del www\cubes%VER%_src.zip
cd bin
7z a -tzip ..\www\cubes%VER%.zip cubes.exe mesh.dll ..\cubes.rtf ..\cubes.ini
cd ..
7z a -tzip www\cubes%VER%_src.zip *.* Properties img -xr!.vs

