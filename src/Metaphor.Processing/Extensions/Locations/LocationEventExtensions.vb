Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence
Imports TGGD.Processing

Friend Module LocationEventExtensions

    Private Delegate Sub EventInitializer(location As ILocation)

    Private ReadOnly eventTable As New Dictionary(Of EventInitializer, Integer) From
        {
            {AddressOf NothingEvent, 100},
            {AddressOf ShortcutEvent, 10},
            {AddressOf FlowerPatchEvent, 10}
        }

#Region "Pick Flower"
    Private Sub FlowerPatchEvent(location As ILocation)
        Dim world = location.World
        Dim character = world.Avatar
        world.AddMessage($"{character.Name} discovers a patch of wild flower!")
        location.CreateFeature(FeatureTypes.FLOWER_PATCH, "Flower Patch", "This is patch of flowers. Which means that it is an area of ground that has flowers growing from it.", AddressOf InitializeFlowerPatch)
    End Sub

    Private Sub InitializeFlowerPatch(feature As IFeature)
        feature.InitializeCounter(Counters.FLOWERS_REMAINING, RNG.RollDice("3d4"), 0, 12)
        feature.CreateVerb(VerbTypes.PICK_FLOWER, "Pick a Flower", "You use your plant murdering hand to pluck a defenseless flower from the flower patch. You do not hear the wailing lament of the other flowers in response to the death of one of its brethren.")
    End Sub
#End Region

    Private Sub ShortcutEvent(location As ILocation)
        Dim world = location.World
        Dim character = world.Avatar
        world.AddMessage($"{character.Name} discovers a potential shortcut!")
        location.SetTag(Tags.SHORTCUT)
    End Sub

    Private Sub NothingEvent(location As ILocation)
        location.World.AddMessage("Nothing eventful occurs.")
    End Sub

    <Extension>
    Friend Sub GenerateEvent(location As ILocation)
        Dim eventDelegate = RNG.FromGenerator(eventTable)
        eventDelegate.Invoke(location)
    End Sub

End Module
