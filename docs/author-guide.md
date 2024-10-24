# Better Crane Game (BCG)
## Contents
* [Introduction](#intro)
* [Tile Action](#action)
* [Crane Game Data Asset](#cgdata)
* [Prize Data Asset](#prize)

## Introduction<span id="intro"></span>
BCG is a mapmaker tool, it requires Content Patcher and SpaceCore and is intended to be used by mod authors.<br>
It attempts to de-hardcode most of the crane game, without modifying the base game's crane game.<br>

#### Current features
Custom:
- Prizes
- Play cost
- Play time
- Play credits
- Music

Other:
- SpaceCore virtual currency support
- Separate textures

## Tile Action<span id="action"></span>
BCG adds the `BetterCraneGame [CraneGameDataEntryKey]` tile action.
- `CraneGameDataEntryKey` refers to an entry in the `rokugin.BCG/CraneGameData` asset. Not setting this will use all default values.

The tile action can be added to a map either via Tiled or CP.<br>

Tiled:<br>
<!-- ![Screenshot of the tile action in Tiled]() -->


CP with the [EditMap Action](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-editmap.md):
```jsonc
{
  "Action": "EditMap",
  "Target": "Maps/YourMapHere",
  "MapTiles": [
    {
      "Position": {// tile position of the crane game interaction tile
        "X": 10,
        "Y": 10
      },
      "Layer": "Buildings",
      "SetProperties": {
        "Action": "BetterCraneGame {{ModId}}_ExampleGame"
      }
    }
  ]
}
```

## Crane Game Data Asset<span id="cgdata"></span>
BCG adds a custom data asset to the content pipeline called `rokugin.BCG/CraneGameData`.<br>
You can use CP's [EditData Action](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-editdata.md) to modify or add entries
to the asset in order to create different games with different settings.<br>

```jsonc
{
  "Action": "EditData",
  "Target": "rokugin.BCG/CraneGameData",
  "Entries": {
    "{{ModId}}_ExampleGame": {
      "Texture": "{{ModId}}/CraneGameTexture",
      "MusicCue": "{{ModId}}_ExampleMusic",
      "FastMusicCue": "{{ModId}}_ExampleMusicFast",
      "Currency": "{{ModId}}_Token",
      "Cost": 10,
      "CurrencyDisplayName": "tokens",
      "PlayTime": 20,
      "Credits": 5,
      "PrizeDataEntry": "{{ModId}}_ExamplePrizes",
      "LeftPrizeChance": 1,
      "RightPrizeChance": 1
    }
  }
}
```

- `{{ModId}}_ExampleGame` is what you would use in the tile action to launch a crane game with these settings. You can use whatever you want here as long as it doesn't have spaces in it.

- `Texture` allows you to target your own custom spritesheet, letting you set up as many different designs as you like. Can be omitted or set to `null` to use the default texture.

- `MusicCue` allows you to target different music, you can either [load your own](https://stardewvalleywiki.com/Modding:Audio) or target some other music in the game (just be warned that there's no fast version of other game music).

- `FastMusicCue` allows you to target different music to play when the claw is returning with a prize, you can also load your own or target some other game music.

- `Currency` can be set to any SpaceCore virtual currency, or `"coins"`, `null` or omitted for the default game currency.

- `Cost` is how much currency it costs to start the game.

- `CurrencyDisplayName` allows you to set a different name for your currency to show in the game start dialogue box. If using SpaceCore virtual currency and left blank or omitted, the value of `Currency` will be used, otherwise if using base game money the currency name will just be omitted.

- `PlayTime` is how many seconds each round of the game is, which starts counting as soon as you first move the claw.

- `Credits` is how many rounds you get to play.

- `PrizeDataEntry` is the entry key in the `rokugin.BCG/PrizeData` asset for the prize list you want to use, can be omitted or set to `null` to create a prize list based on the entire asset.

- `LeftPrizeChance` is a value between 0 and 1 to indicate the chance that the hidden prize behind the bush, just below the prize return, will spawn; 0 is never and 1 is always.

- `RightPrizeChance` is a value between 0 and 1 to indicate the chance that the hidden prize behind the bush, on the center divider at the top, will spawn; 0 is never and 1 is always.

## Prize Data Asset<span id="prize"></span>
BCG adds a custom data asset to the content pipeline called `rokugin.BCG/PrizeData`.<br>
You can use CP's [EditData Action](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-editdata.md) to modify or add entries
to the asset in order to create separate prize lists that can be used separately or all together in each game.<br>

```jsonc
{
  "Action": "EditData",
  "Target": "rokugin.BCG/PrizeData",
  "Entries": {
    "{{ModId}}_ExamplePrizes": {
      "CommonList": {
        "Prizes": [
          {
            "ItemId": "(O)16"
          }
        ]
      },
      "RareList": {
        "Prizes": [
          {
            "ItemId": "(O)337"
          }
        ]
      },
      "DeluxeList": {
        "Prizes": [
          {
            "ItemId": "(O)74"
          }
        ]
      },
      "LeftSecretPrizes": [
        {
          "ItemId": "(O)347"
        }
      ],
      "RightSecretPrizes": [
        {
          "ItemId": "(O)527"
        }
      ],
      "LeftDecorationPrizes": [
        {
          "ItemId": "(O)543"
        }
      ],
      "RightDecorationPrizes": [
        {
          "ItemId": "(O)541"
        }
      ]
    }
  }
}
```

- `{{ModId}}_ExamplePrizes` is what you would use in the `rokugin.BCG/CraneGameData` asset to set a specific prize list. Should be fine to use whatever you want here.

- `CommonList` is a model that contains a list of `Prizes`. 8 common prizes are spawned, chosen randomly from the list.

- `RareList` is a model that contains a list of `Prizes`. 2 rare prizes are spawned, chosen randomly from the list.

- `DeluxeList` is a model that contains a list of `Prizes`. 1 deluxe prize is spawned, chosen randomly from the list.

- `Prizes` is a list of models of [item spawn fields](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields), the most minimal only requires an `ItemId`.

- `LeftSecretPrizes` is a list of models of item spawn fields, like `Prizes`. 1 left secret prize is spawned, chosen randomly from the list.

- `RightSecretPrizes` is a list of models of item spawn fields, like `Prizes`. 1 right secret prize is spawned, chosen randomly from the list.

- `LeftDecorationPrizes` is a list of models of item spawn fields, like `Prizes`. 1 left decoration prize is spawned above the right conveyor belt, chosen randomly from the list.

- `RightDecorationPrizes` is a list of models of item spawn fields, like `Prizes`. 1 right decoration prize is spawned above the right conveyor belt, chosen randomly from the list.
