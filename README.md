# Prefab Asset Utility
A utility package that keeps track of the connections between prefabs and their attached assets

# Performance
This package has been tested on a project with 10K+ prefabs, initial cache generation took roughly 30 seconds and after that it incrementally updates itself

# Asset Serialization Mode
Must be set to Force Text in order for this package to work

# Usage
Get all prefabs which use a specific asset/component based on its GUID
`PrefabUtility.GetPrefabsForGUID("f7a4213c60a3426995bb8b901c2ea1fd")`

Or perform the reverse and get all GUIDs assigned to a prefab based on its path
`PrefabUtility.GetGUIDsForPrefab("Assets/Prefabs/MyAwesomePrefab.prefab")`

# Editor only
This package only works in the Editor, it has been tested on Windows but should also work on macOS though this hasn't been tested yet

# Settings
Open `Edit` -> `Preferences` -> `Prefab Asset Cache` to force refresh the cache and enable/disable saving the cache on every prefab change to the disk

# Cache location
This package stores its cache in the Library folder in `PrefabToGUID.json` and `GUIDToPrefab.json`
