Imports Metaphor.Persistence

Friend Module TownInitializer
    Friend Sub Initialize(world As IWorld, context As IInitializationContext)
        world.CreateLocation(InitializeNorthWestTown(context))
        world.CreateLocation(InitializeNorthTown(context))
        world.CreateLocation(InitializeNorthEastTown(context))
        world.CreateLocation(InitializeWestTown(context))
        world.CreateLocation(TownCenterInitializer.Initialize(context))
        world.CreateLocation(context.InitializeEastTown())
        world.CreateLocation(InitializeSouthWestTown(context))
        world.CreateLocation(InitializeSouthTown(context))
        world.CreateLocation(InitializeSouthEastTown(context))
        context.NorthWestTown.CreateRoute(RouteTypes.BORING, Directions.SOUTH, context.WestTown, AddressOf InitializeRoad)
        context.WestTown.CreateRoute(RouteTypes.BORING, Directions.NORTH, context.NorthWestTown, AddressOf InitializeRoad)
        context.NorthEastTown.CreateRoute(RouteTypes.BORING, Directions.WEST, context.NorthTown, AddressOf InitializeRoad)
        context.NorthTown.CreateRoute(RouteTypes.BORING, Directions.EAST, context.NorthEastTown, AddressOf InitializeRoad)
        context.SouthEastTown.CreateRoute(RouteTypes.BORING, Directions.NORTH, context.EastTown, AddressOf InitializeRoad)
        context.EastTown.CreateRoute(RouteTypes.BORING, Directions.SOUTH, context.SouthEastTown, AddressOf InitializeRoad)
        context.SouthWestTown.CreateRoute(RouteTypes.BORING, Directions.EAST, context.SouthTown, AddressOf InitializeRoad)
        context.SouthTown.CreateRoute(RouteTypes.BORING, Directions.WEST, context.SouthWestTown, AddressOf InitializeRoad)
        context.CenterTown.CreateRoute(RouteTypes.BORING, Directions.NORTH, context.NorthTown, AddressOf InitializeRoad)
        context.NorthTown.CreateRoute(RouteTypes.BORING, Directions.SOUTH, context.CenterTown, AddressOf InitializeRoad)
        context.CenterTown.CreateRoute(RouteTypes.BORING, Directions.EAST, context.EastTown, AddressOf InitializeRoad)
        context.EastTown.CreateRoute(RouteTypes.BORING, Directions.WEST, context.CenterTown, AddressOf InitializeRoad)
        context.CenterTown.CreateRoute(RouteTypes.BORING, Directions.SOUTH, context.SouthTown, AddressOf InitializeRoad)
        context.SouthTown.CreateRoute(RouteTypes.BORING, Directions.NORTH, context.CenterTown, AddressOf InitializeRoad)
        context.CenterTown.CreateRoute(RouteTypes.BORING, Directions.WEST, context.WestTown, AddressOf InitializeRoad)
        context.WestTown.CreateRoute(RouteTypes.BORING, Directions.EAST, context.CenterTown, AddressOf InitializeRoad)
    End Sub

    Private Sub InitializeRoad(route As IRoute)
        route.SetName("Road")
        route.SetFlavor("You walk along a well traveled road.")
    End Sub

    Private Function InitializeSouthEastTown(context As IInitializationContext) As LocationInitializer
        Return Sub(location)
                   location.SetName("Southeast Corner of Town")
                   location.SetFlavor("You find yerself in the southeast corner of town.")
                   context.SouthEastTown = location
               End Sub
    End Function

    Private Function InitializeSouthTown(context As IInitializationContext) As LocationInitializer
        Return Sub(location)
                   location.SetName("South Side of Town")
                   location.SetFlavor("You find yerself in the south side of town.")
                   context.SouthTown = location
               End Sub
    End Function

    Private Function InitializeSouthWestTown(context As IInitializationContext) As LocationInitializer
        Return Sub(location)
                   location.SetName("Southwest Corner of Town")
                   location.SetFlavor("You find yerself in the southwest corner of town.")
                   context.SouthWestTown = location
               End Sub
    End Function

    Private Function InitializeWestTown(context As IInitializationContext) As LocationInitializer
        Return Sub(location)
                   location.SetName("West Side of Town")
                   location.SetFlavor("You find yerself in the west side of town.")
                   context.WestTown = location
               End Sub
    End Function

    Private Function InitializeNorthEastTown(context As IInitializationContext) As LocationInitializer
        Return Sub(location)
                   location.SetName("Northeast Corner of Town")
                   location.SetFlavor("You find yerself in the northeast corner of town.")
                   context.NorthEastTown = location
               End Sub
    End Function

    Private Function InitializeNorthTown(context As IInitializationContext) As LocationInitializer
        Return Sub(location)
                   location.SetName("North Side of Town")
                   location.SetFlavor("You find yerself in the north side of town.")
                   context.NorthTown = location
               End Sub
    End Function

    Private Function InitializeNorthWestTown(context As IInitializationContext) As LocationInitializer
        Return Sub(location)
                   location.SetName("Sorthwest Corner of Town")
                   location.SetFlavor("You find yerself in the northwest corner of town.")
                   context.NorthWestTown = location
               End Sub
    End Function
End Module
