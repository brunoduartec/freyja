# Player 004 character asset

- Export: `Player004_Male_Casual.fbx`
- Source file: `/Users/bruno.duartec/Documents/Projects/Low Poly World/downloads/male/Male_Casual.blend`
- Blender source file was opened in Blender 5.2.2 LTS through the Blender MCP CLI; the source `.blend` was not saved or modified.
- Export contains the `BaseHuman` mesh and `HumanArmature`. The source has 12 actions; the FBX contains 11 take labels, and Blender's FBX round-trip exposed 10 imported actions. No animation behavior was added or assigned, and Unity import has not been checked.
- Uniform scale: `0.37291844`; source bounds were scaled to `1.8 m` with soles at `z = 0`. Blender FBX round-trip measured approximately `1.7935 m` height and `z = -0.00044 m` at the sole plane. Source front faces `-Y`; FBX was exported with `axis_forward='-Z'`, `axis_up='Y'`. Confirm final scale and facing after Unity import.
- Materials use their source material colors. `Texture.png` is referenced by an unused image datablock, is not packed, and is not connected to any material image-texture node; it was not required by the rendered/exported appearance. No texture dependency was exported.
- Review render: `Preview_Male_Casual.png`.

## License and provenance

No license or source README was present beside the files in `downloads/male/`. The blend contains internal strings mentioning Quaternius and a `Male_Casual.fbx` path; these are clues only and do not prove the file's provenance. The official [Quaternius Animated Men Pack page](https://quaternius.com/packs/animatedmen.html) currently lists that pack as CC0, but this asset's association with that pack has not been independently verified. Keep the original download/package record if available before relying on that license status.
