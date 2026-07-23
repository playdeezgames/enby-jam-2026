Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence
Imports TGGD.Processing

Friend Module LocationEventExtensions

    Private Delegate Sub EventInitializer(location As ILocation)

#If DEBUG Then
    Private Const TEST_WEIGHT = 250
#End If

    Private ReadOnly eventTable As New Dictionary(Of EventInitializer, Integer) From
        {
            {AddressOf SpawnShortcut, 100},
            {AddressOf SpawnFlowerPatch, 100},
            {AddressOf SpawnTraehi, 50},
            {AddressOf SpawnVendingMachine, 50},
            {AddressOf SpawnAbandonedHouse, 10},
            {AddressOf SpawnCatShrine, 10},
            {AddressOf SpawnKwikTrip, 10},
            {AddressOf SpawnRecycleBin, 10},
            {AddressOf SpawnDishes, 100},
            {AddressOf SpawnCarKeys, 1},
            {AddressOf SpawnCar, TEST_WEIGHT},
            {AddressOf SpawnNothing, 1000}
        }

    Private Sub SpawnCar(location As ILocation)
        Dim world = location.World
        Dim avatar = world.Avatar
        Dim feature = location.CreateFeature(FeatureTypes.CAR, "Car", "This is a car. Assuming you have keys to it and a learners permit, you can drive it. Yes, you can drive this with a learner's permit because it has a built-in AI driving instructor. Ain't the future grand?")
        feature.CreateVerb(VerbTypes.DRIVE, "Drive", "You stick the key in, fire it up, and drive it.")
        world.AddMessage($"{avatar.Name} finds {feature.Name}.")
    End Sub

    Private Sub SpawnCarKeys(location As ILocation)
        Dim world = location.World
        Dim avatar = world.Avatar
        If Not avatar.Inventory.Items.Any(Function(x) x.HasTag(Tags.CAR_KEYS)) Then
            Dim item = location.Inventory.CreateItem(ItemTypes.CAR_KEYS, "Car Keys", "These are car keys. They weren't where you'd thought they'd be!")
            item.SetTag(Tags.CAR_KEYS)
            world.AddMessage($"{avatar.Name} sees {item.Name} on the ground.")
        Else
            SpawnNothing(location)
        End If
    End Sub
#Region "Dishes"
    Private Sub SpawnDishes(location As ILocation)
        Dim world = location.World
        Dim avatar = world.Avatar
        Dim jools = RNG.RollDice("2d6")
        Dim feature = location.CreateFeature(FeatureTypes.DISHES, "Sink Full of Dirty Dishes", $"This is a sink full to the brim with dirty dishes. They didn't even rinse them. However, if you wash them, you will get {jools} jools.")
        Dim verb = feature.CreateVerb(VerbTypes.WASH_DISHES, "Wash Dishes", "You wash the stinking, disgusting dishes, dry them, and put them away.")
        verb.SetCounter(Counters.JOOLS, jools)
        world.AddMessage($"{avatar.Name} finds a {feature.Name}.")
    End Sub
#End Region
#Region "Recycle Bin"
    Private Sub SpawnRecycleBin(location As ILocation)
        Dim world = location.World
        Dim avatar = world.Avatar
        Dim feature = location.CreateFeature(FeatureTypes.RECYCLE_BIN, "Recycle Bin", "This is a recycle bin. You place litter in here, so that you are not a bad person.")
        feature.CreateVerb(VerbTypes.RECYCLE_LITTER, "Recycle Litter", "You put litter into the bin and feel better about yerself.")
        world.AddMessage($"{avatar.Name} finds a {feature.Name}.")
    End Sub
#End Region
#Region "Kwik Trip"
    Private Sub SpawnKwikTrip(location As ILocation)
        Dim world = location.World
        Dim avatar = world.Avatar
        Dim feature = location.CreateFeature(FeatureTypes.KWIK_TRIP, "Kwik Trip", "This is a Kwik Trip, a convenience store common in the midwestern US that sell gasoline/petrol and more importantly has a fountain drink dispenser that dispenses Dr Pepper, the elixer of the gods.")
        feature.CreateVerb(VerbTypes.BUY_DR_PEPPER, "Buy Dr Pepper", "You pour yerself a Dr Pepper from the fountain. You can barely contain yer exicitement!")
        world.AddMessage($"{avatar.Name} finds a {feature.Name}.")
    End Sub
#End Region
#Region "Cat Shrine"
    Private ReadOnly catNames As IEnumerable(Of String) = {"Captain Jack", "Nina", "Körmy"}
    Private Sub SpawnCatShrine(location As ILocation)
        Dim world = location.World
        Dim avatar = world.Avatar
        Dim catName = RNG.FromEnumerable(catNames)
        Dim feature = location.CreateFeature(FeatureTypes.CAT_SHRINE, "Cat Shrine", $"This is the shrine to a cat. Specifically, this is a shrine for a cat named `{catName}`.")
        feature.CreateVerb(VerbTypes.PRAY, "Pray", "You stand reverently quiet for a moment, contemplating.")
        world.AddMessage($"{avatar.Name} finds a {feature.Name}.")
    End Sub
#End Region
#Region "Abandoned House"
    Private Sub SpawnAbandonedHouse(location As ILocation)
        Dim world = location.World
        Dim avatar = world.Avatar
        If avatar.Inventory.Items.Any(Function(x) x.HasTag(Tags.SUPPRESS_ABANDONED_HOUSE)) Then
            SpawnNothing(location)
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
        item.InitializeCounter(Counters.SNAX, RNG.RollDice("2d6"), 0, 12)
        item.CreateVerb(VerbTypes.REACH_IN, "Reach In...", "You steel yer nerves and reach in to the pkastic bag, knowing the developer to be a devious person.")
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
    Private Sub SpawnFlowerPatch(location As ILocation)
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

    Private Sub SpawnShortcut(location As ILocation)
        Dim world = location.World
        Dim character = world.Avatar
        world.AddMessage($"{character.Name} discovers a potential shortcut!")
        location.SetTag(Tags.SHORTCUT)
    End Sub

    Private Sub SpawnNothing(location As ILocation)
        location.World.AddMessage("Nothing eventful occurs.")
    End Sub

    <Extension>
    Friend Sub GenerateEvent(location As ILocation)
        Dim eventDelegate = RNG.FromGenerator(eventTable)
        eventDelegate.Invoke(location)
    End Sub

End Module
