Imports Metaphor.Persistence

Friend Module AbandonedHouseInitializer
    Friend Function Initialize(context As IInitializationContext) As LocationInitializer
        Return Sub(location)
                   location.SetName("Abandoned House")
                   location.SetFlavor("This house is abandoned. The yard is overgrown. The doors are ripped off, and the windows are made of sheetgoods. You detect the unmistakable odor of a klonkku.")
                   location.CreateRoute(RouteTypes.BORING, Directions.OUT, context.SouthTown, AddressOf InitializeOpenDoorway)
                   context.SouthTown.CreateRoute(RouteTypes.BORING, Directions.IN, location, AddressOf InitializeOpenDoorway)
                   location.Inventory.CreateItem(ItemTypes.PRINTER, AddressOf InitializeDestroyedPrinter)
                   location.Inventory.CreateItem(ItemTypes.PKASTIC_BAG, AddressOf InitializePkasticBag)
                   Dim basement = location.World.CreateLocation(InitializeBasement(location))
#If DEBUG Then
                   context.PortalDestination = location
#End If
               End Sub
    End Function

    Private Function InitializeBasement(house As ILocation) As LocationInitializer
        Return Sub(location)
                   location.SetName("Basement")
                   location.SetFlavor("This is the basement of the abandoned house. The stench of klonkku is overpowering.")
                   location.CreateRoute(RouteTypes.BORING, Directions.UP, house, AddressOf InitializeStairs)
                   house.CreateRoute(RouteTypes.BORING, Directions.DOWN, location, AddressOf InitializeStairs)
                   location.CreateFeature(AddressOf InitializeKlonkkuCorpse)
               End Sub
    End Function

    Private Sub InitializeKlonkkuCorpse(feature As IFeature)
        feature.SetName("Klonkku Corpse")
        feature.SetFlavor("You behold a very dead klonkku. It smells like you'd expect. For some reason, its left index finger is brown, and yer pretty sure that's not chocolate.")
        feature.CreateVerb(VerbTypes.CHECK_BUTTHOLE, AddressOf InitializeCheckKlonkkuButthole)
    End Sub

    Private Sub InitializeCheckKlonkkuButthole(verb As IVerb)
        verb.SetName("Check Butthole")
        verb.SetFlavor("Well, yer sure a curious fella, aintcha?")
    End Sub

    Private Sub InitializeStairs(route As IRoute)
        route.SetName("Stairs")
        route.SetFlavor("You go up(or down) the stairs. The dev couldn't be bothered to make two different flavor texts.")
    End Sub

    Private Sub InitializePkasticBag(item As IItem)
        item.SetName("Pkastic Bag")
        item.SetFlavor("No, that is not a misspelling. This item is made from pkastic. You have mixed feelings about reaching inside.")
        item.CreateVerb(VerbTypes.REACH_IN, AddressOf InitializeReachInPkasticBag)
    End Sub

    Private Sub InitializeReachInPkasticBag(verb As IVerb)
        verb.SetName("Reach In")
        verb.SetFlavor("Remembering last time, you cringe a bit as you reach in to the pkastic bag.")
    End Sub

    Private Sub InitializeDestroyedPrinter(item As IItem)
        item.SetName("Destroyed Printer")
        item.SetFlavor("This printer looks like it has been thoroughly bashed to smithereens with a cricket bat.")
    End Sub

    Private Sub InitializeOpenDoorway(route As IRoute)
        route.SetName("Open Doorway")
        route.SetFlavor("You walk through a completely open doorway. The doors that previously hung here are gone.")
    End Sub
End Module
