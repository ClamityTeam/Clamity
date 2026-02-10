:: Pull tML - This should always use the latest stable release
cd ..
mkdir tml
cd tml
curl https://github.com/tModLoader/tModLoader/releases/latest/download/tModLoader.zip -L --output tModLoader.zip
unzip tModLoader.zip
cd ..

:: Create tModLoader.targets
echo ^<Project ToolsVersion="14.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003"^>^<Import Project="tml/tMLMod.targets" /^>^</Project^> > tModLoader.targets
cd Clamity
