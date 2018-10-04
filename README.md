# Two Point Hospital Mods

## Available mods

* [Skip GPOffice](https://www.nexusmods.com/twopointhospital/mods/8)<br/>
  Skip the extra visit to the GPOffice when the diagnosis certainty is higher than the configureable value.

* [Epidemic Helper](https://www.nexusmods.com/twopointhospital/mods/9)<br/>
  Provides helper actions for the epidemic challenges, like logging the infected character names, highlight them or vaccinate all of them.

* [Qualification Utils](https://www.nexusmods.com/twopointhospital/mods/10)<br/>
  Manage the qualifications and rank of your staff.

## Development / Build

You need to update the references to the assemblies. For example the [EpidemicHelper project file](https://github.com/Silverdark/tph-mods/blob/master/EpidemicHelper/EpidemicHelper/EpidemicHelper.csproj) has references like this:

```
<Reference Include="0Harmony12">
    <HintPath>..\..\..\..\..\..\Program Files (x86)\SteamLibrary\SteamApps\common\TPH\TPH_Data\Managed\0Harmony12.dll</HintPath>
    <Private>False</Private>
</Reference>
```

Update the path to your game path.

## Credits

* Thanks to [newman55](https://github.com/newman55) for the creation of the [Unity Mod Manager](https://github.com/newman55/unity-mod-manager).