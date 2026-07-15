Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module WorldInitializer
    <Extension>
    Friend Sub Initialize(world As IWorld, context As IInitializationContext)
        world.Clear()
        TownInitializer.Initialize(world, context)
        world.CreateLocation(AbandonedHouseInitializer.Initialize(context))
        world.CreateLocation(BlueRoomInitializer.Initialize(context))
        world.AddMessage("So it begins!")
        world.Avatar.Look()
    End Sub
End Module
