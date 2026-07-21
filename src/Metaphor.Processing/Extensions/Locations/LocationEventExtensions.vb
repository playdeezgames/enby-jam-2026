Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence
Imports TGGD.Processing

Friend Module LocationEventExtensions

    Private Delegate Sub EventInitializer(location As ILocation)

    Private ReadOnly eventTable As New Dictionary(Of EventInitializer, Integer) From
        {
            {AddressOf NothingEvent, 100},
            {AddressOf ShortcutEvent, 10},
            {AddressOf FlowerPatchEvent, 10},
            {AddressOf SpawnTraehi, 5},
            {AddressOf SpawnVendingMachine, 5},
            {AddressOf SpawnAbandonedHouse, 25}
        }
#Region "Abandoned House"
    Private Sub SpawnAbandonedHouse(location As ILocation)
        Dim world = location.World
        Dim avatar = world.Avatar
        If avatar.Inventory.Items.Any(Function(x) x.HasTag(Tags.SUPPRESS_ABANDONED_HOUSE)) Then
            NothingEvent(location)
            Return
        End If
        Dim feature = location.CreateFeature(FeatureTypes.ABANDONED_HOUSE, "Abandoned House", "This house is abandoned. The lawn is overgrown. The doors have been ripped off of the hinges, and the windows are made of sheet goods.", AddressOf InitializeAbandonedHouse)
        world.AddMessage($"{avatar.Name} finds an {feature.Name}.")
    End Sub
    Private Sub InitializeAbandonedHouse(feature As IFeature)
        feature.Inventory.CreateItem(ItemTypes.DESTROYED_PRINTER, "Destroyed Printer", "This printer has been smashed to smithereens. It will never work again. It is an ex-printer.", AddressOf InitializeDestroyedPrinter)
        feature.Inventory.CreateItem(ItemTypes.PKASTIC_BAG, "Pkastic Bag", "This is a bag. It is made of pkastic. No, don't ask me what pkastic is, just look at the 'K' key and notice how it is right next to the 'L' key.", AddressOf InitializePkasticBag)
    End Sub

    Private Sub InitializePkasticBag(item As IItem)
        item.SetTag(Tags.SUPPRESS_ABANDONED_HOUSE)
        'TODO: reach in
    End Sub

    Private Sub InitializeDestroyedPrinter(item As IItem)
        item.SetTag(Tags.SUPPRESS_ABANDONED_HOUSE)
    End Sub
#End Region
#Region "Vending machine"
    Private Sub SpawnVendingMachine(location As ILocation)
        Dim world = location.World
        Dim avatar = world.Avatar
        Dim feature = location.CreateFeature(FeatureTypes.VENDING_MACHINE, "Vending Machine", "This is a vending machine that sells snax in exchange for jools. I don't know what it is doing standing out here in the middle of nowhere, either.", AddressOf InitializeVendingMachine)
        world.AddMessage($"{avatar.Name} finds a {feature.Name}.")
    End Sub

    Private Sub InitializeVendingMachine(feature As IFeature)
        feature.CreateVerb(VerbTypes.BUY_SNAX, "Buy Snax(1 jools)", "You put the jools into the jools receptacle and press the `Snax` button....")
    End Sub
#End Region
#Region "Traehi"
    Private Sub SpawnTraehi(location As ILocation)
        Dim world = location.World
        Dim avatar = world.Avatar
        Dim character = location.CreateCharacter(CharacterTypes.TRAEHI, "Traehi", "This person obviously likes flowers. Their clothing has a floral pattern. They have flowers in their hair. They a holding a basket of fresh cut flowers.", AddressOf InitializeTraehi)
        world.AddMessage($"{avatar.Name} encounters {character.Name}, the well-known lover of all things floral!")
    End Sub

    Private Sub InitializeTraehi(character As ICharacter)
        character.CreateVerb(VerbTypes.GIVE_FLOWER, "Give Flower", "You give them a flower. Isn't that nice?")
    End Sub
#End Region

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
