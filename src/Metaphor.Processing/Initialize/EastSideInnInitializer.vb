Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module EastSideInnInitializer
    <Extension>
    Friend Function InitializeEastTown(context As IInitializationContext) As LocationInitializer
        Return Sub(location)
                   location.SetName("East Side of Town")
                   location.SetFlavor("You find yerself in the east side of town.")
                   context.EastTown = location
                   location.World.CreateLocation(InitializeInn(context))
                   location.CreateFeature(AddressOf InitializeSign)
               End Sub
    End Function

    Private Sub InitializeSign(feature As IFeature)
        feature.SetName("Sign")
        feature.SetFlavor("The sign reads `Jusdatip Inn`, and below it reads `Gorachan: Proprietor`.")
    End Sub

    Private Function InitializeInn(context As IInitializationContext) As LocationInitializer
        Return Sub(location)
                   location.SetName("Inn")
                   location.SetFlavor("Yer inside Jusdatip Inn. Which is ironic, if you think about it.")
                   location.CreateRoute(RouteTypes.BORING, Directions.OUT, context.EastTown, AddressOf InitializeInnDoor)
                   context.EastTown.CreateRoute(RouteTypes.BORING, Directions.IN, location, AddressOf InitializeInnDoor)
                   location.CreateCharacter(CharacterTypes.NPC, AddressOf InitializeGorachan)
                   location.World.CreateLocation(location.InitializeCellar(context))
               End Sub
    End Function

    <Extension>
    Private Function InitializeCellar(inn As ILocation, context As IInitializationContext) As LocationInitializer
        Return Sub(location)
                   location.SetName("Inn Cellar")
                   location.SetFlavor("Yer in the cellar of the inn. It smells of rat turd, and sounds like an early 80s hair band.")
                   location.CreateRoute(RouteTypes.BORING, Directions.UP, inn, InitializeStairs("up"))
                   inn.CreateRoute(RouteTypes.INN_CELLAR, Directions.DOWN, location, InitializeStairs("down"))
                   location.CreateVerb(VerbTypes.SEARCH, AddressOf InitializeSearch)
               End Sub
    End Function

    Private Sub InitializeCellarQuest(verb As IVerb)
        verb.SetName("Ask about work")
        verb.SetFlavor("Gorachan sez: ""Well, I got a rat problem in my cellar. I'll pay one jools per rat tail, up to ten.""")
    End Sub

    Private Sub InitializeSearch(verb As IVerb)
        verb.SetName("Search...")
        verb.SetFlavor("You look around the cellar for rats.")
    End Sub

    Private Function InitializeStairs(directionName As String) As RouteInitializer
        Return Sub(route)
                   route.SetName("Stairs")
                   route.SetFlavor($"You go {directionName} the rickety-ass, poorly maintained, mysteriously sticky stairs.")
               End Sub
    End Function

    Private Sub InitializeGorachan(character As ICharacter)
        character.SetName("Gorachan")
        character.SetFlavor("""Welcome to Jusdatip Inn, ya cunt!""(he's not vulgar, just Australian)")
        character.CreateVerb(VerbTypes.CHECK_BUTTHOLE, AddressOf InitializeCheckButthole)
        character.CreateVerb(VerbTypes.CELLAR_QUEST, AddressOf InitializeCellarQuest)
        character.CreateVerb(VerbTypes.GIVE_RAT_TAIL, AddressOf InitializeGiveRatTail)
    End Sub

    Private Sub InitializeGiveRatTail(verb As IVerb)
        verb.SetName("Give Rat Tail(s)")
        verb.SetFlavor("You turn in the rat tail(s) to Gorachan for the reward.")
    End Sub

    Private Sub InitializeCheckButthole(verb As IVerb)
        verb.SetName("Check Butthole")
        verb.SetFlavor("Gorachan sez: ""If you wanna check my butthole, you'll have to buy me dinner first, mate!"" He MIGHT be kidding.")
    End Sub

    Private Sub InitializeInnDoor(route As IRoute)
        route.SetName("Inn Door")
        route.SetFlavor("You go through a door. It is made of wood. Hard, HARD wood. So hard, you can't believe it.")
    End Sub
End Module
