# Two Point Hospital Mods

Thanks to [newman55](https://github.com/newman55) for the creation of the [Unity Mod Manager](https://github.com/newman55/unity-mod-manager).

## Development / Build

You need to update the references to the assemblies. For example the [EpidemicHelper project file](https://github.com/Silverdark/tph-mods/blob/master/EpidemicHelper/EpidemicHelper/EpidemicHelper.csproj) has references like this:

```
<Reference Include="0Harmony12">
    <HintPath>..\..\..\..\..\..\Program Files (x86)\SteamLibrary\SteamApps\common\TPH\TPH_Data\Managed\0Harmony12.dll</HintPath>
    <Private>False</Private>
</Reference>
```

Update the path to your game path.