# LogiWheel
This is a simple mod that implements the [Logitech Steering SDK](https://www.logitechg.com/en-au/innovation/developer-lab.html) into the game via DynamicVariables under the user root.

## Install
To install grab the latest [LogiWheel.dll](https://github.com/LeCloutPanda/LogiWheel/releases/latest/download/LogiWheel.dll) and place it into your rml_mods folder also grab the [LogitechSteeringWheelEnginesWrapper.dll](https://github.com/LeCloutPanda/LogiWheel/releases/latest/download/LogitechSteeringWheelEnginesWrapper.dll) and place it in the same directory as your ``Resonite`` install. If you need information on modding resonite please visit the [Resonite Mod Loader page](https://github.com/resonite-modding-group/ResoniteModLoader). 

## How to use
Plug your wheel setup in and it should be picked up and start dumping the wheels values to a slot called ``LogiWheels - Values`` under your user root. If no values are being picked up and it says it's connected under the mod settings there is a ``Refresh`` option simply set it to true and it should reset the SDK and start picking up values again.

## TODO
- [ ] Make the initalisation a little nicer and not require the user to refresh it
- [ ] Have an optional ui showing wheel inputs and buttons(Possibly use [art0007i](https://github.com/art0007i/ImGuiUnityInject)'s implementation of [ImGui](https://github.com/ocornut/imgui))
- [ ] Output all avaliable values to DynamicVariables
- [ ] Option to output raw values rather then normalised ones(Some values output between -32767 and 32767 so I normalise them under the hood before spitting it out)
- [ ] Stop/Remove ``LogiWheel - Values`` slot if controller is unplugged
- [ ] Changable update rate
- [ ] Stop updating values when not focused on app(Likely will make this an option)
- [ ] Option to select what controller index to output
- [ ] Abilitiy to use multiple controllers at once

## Note
This was simply a funny project I wanted to do cause I have this wheel so if you find use for this then so be it, I likely will make an osc version of this as a standalone exe or something down the road so it doesn't need to be a mod but for now this works and is still fun to mess with.
