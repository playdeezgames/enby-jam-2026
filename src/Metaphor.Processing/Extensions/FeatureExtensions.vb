Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module FeatureExtensions
    <Extension>
    Friend Sub Examine(feature As IFeature)
        Dim world = feature.World
        world.ClearMessages()
        Dim character = world.Avatar
        world.AddMessage($"{character.Name} interacts with {feature.Name}.")
        world.AddMessage(feature.Flavor)
        Dim items = feature.Inventory.Items
        If items.Any Then
            world.AddMessage("Items:")
            For Each item In items
                world.AddMessage($"- {item.Name}")
            Next
        End If
    End Sub
End Module
