Imports Metaphor.Persistence

Friend Interface IInitializationContext
    ReadOnly Property ChosenName As String
    Property BlueRoom As ILocation
    Property SouthEastTown As ILocation
    Property SouthTown As ILocation
    Property SouthWestTown As ILocation
    Property EastTown As ILocation
    Property CenterTown As ILocation
    Property WestTown As ILocation
    Property NorthEastTown As ILocation
    Property NorthTown As ILocation
    Property NorthWestTown As ILocation
#If DEBUG Then
    Property PortalDestination As ILocation
#End If
End Interface
