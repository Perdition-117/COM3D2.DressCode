# COM3D2.DressCode

Automatically switches costume per scene. Currently supports the following scenes:

- Dance
- Pole dance
- Yotogi
- Private mode
- Honeymoon

![interface](https://github.com/Perdition-117/COM3D2.DressCode/assets/87424475/3654fdb4-9005-4a40-8c1e-c13fba093d3e)

## Usage

The interface may be accessed via the DressCode button on the office screen.

![button](https://user-images.githubusercontent.com/87424475/170781015-1125e75c-cb4f-4c8d-9371-1641b54bdd2c.png)

Scenes may be configured on globally or per maid, using one of the following settings:

- Default (costume will not be switched)
- Scene (use the costume specified for the scene)
- Personal (use the costume specified for the maid)

Click the Edit button in order to open edit mode and create your costume.

To use the same costume for all maids, simply select the Scene setting for the scene, create a costume, and optionally select the Scene setting for any maids previously configured with a different setting.

If a costume has not been created for the configured setting, Default behaviour applies. Maids that have not yet been configured will use the configured scene setting.

DressCode generates thumbnails prefixed `dresscode_` in the `Thumb` directory when a costume is saved. These may safely be deleted and will be regenerated the next time a costume is saved.

## Installing

Get the latest version from [the release page](../../releases/latest). Extract the archive contents into the `BepInEx` directory.
