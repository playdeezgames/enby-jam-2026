Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module WorldInitializer
    <Extension>
    Friend Sub Initialize(world As IWorld, context As IInitializationContext)
        world.Clear()
        world.CreateLocation("On Yer Journey", "Yer on yer journey.", LocationInitializer.Initialize(context))
        world.AddMessage("So it begins!")
        world.Avatar.Look()
    End Sub
End Module
