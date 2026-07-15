Imports Metaphor.Persistence

Friend Module BlueRoomInitializer
    Friend Function Initialize(context As IInitializationContext) As LocationInitializer
        Return Sub(location)
                   location.SetName("The Blue Room")
                   location.SetFlavor("This is the Blue Room. You feel like you may have been here before.")
                   context.BlueRoom = location
                   location.CreateCharacter(CharacterTypes.N00B, InitializeAvatar(context))
                   location.CreateRoute(RouteTypes.BORING, Directions.OUT, context.SouthWestTown, AddressOf InitializeBlueRoomDoor)
                   context.SouthWestTown.CreateRoute(RouteTypes.BORING, Directions.[IN], location, AddressOf InitializeBlueRoomDoor)
#If DEBUG Then
                   location.CreateRoute(RouteTypes.BORING, Directions.SIDEWAYS, context.PortalDestination, AddressOf InitializePortal)
#End If
               End Sub
    End Function

#If DEBUG Then
    Private Sub InitializePortal(route As IRoute)
        route.SetName("Debug Portal")
        route.SetFlavor("You use the magical debug portal to go to the place that yer actively testing.")
    End Sub
#End If

    Private Sub InitializeBlueRoomDoor(route As IRoute)
        route.SetName("Blue Door")
        route.SetFlavor("You open the blue door, go through it, and gently close it behind you.")
    End Sub

    Private Function InitializeAvatar(context As IInitializationContext) As CharacterInitializer
        Return Sub(character)
                   character.SetName(context.ChosenName)
                   character.SetFlavor("Yer pronouns are he/him. It makes sense if you know Finnish.")
                   character.InitializeCounter(Counters.HEALTH, 100, 0, 100)
                   character.InitializeCounter(Counters.SATIETY, 100, 0, 100)
                   character.InitializeCounter(Counters.STOMACH, 0, 0, 50)
                   character.InitializeCounter(Counters.JOOLS, 0, 0, Integer.MaxValue)
                   character.SetMetadata(Metadatas.ATTACK_ROLL, "1d20")
                   character.SetMetadata(Metadatas.DEFEND_ROLL, "2d20")
                   character.World.Avatar = character
               End Sub
    End Function
End Module
