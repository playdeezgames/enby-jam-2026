Imports Metaphor.Persistence

Friend Class InitializationContext
    Implements IInitializationContext
    Private Sub New(chosenName As String)
        Me.ChosenName = chosenName
    End Sub

    Public ReadOnly Property ChosenName As String Implements IInitializationContext.ChosenName

    Public Property BlueRoom As ILocation Implements IInitializationContext.BlueRoom

    Public Property SouthEastTown As ILocation Implements IInitializationContext.SouthEastTown

    Public Property SouthTown As ILocation Implements IInitializationContext.SouthTown

    Public Property SouthWestTown As ILocation Implements IInitializationContext.SouthWestTown

    Public Property EastTown As ILocation Implements IInitializationContext.EastTown

    Public Property CenterTown As ILocation Implements IInitializationContext.CenterTown

    Public Property WestTown As ILocation Implements IInitializationContext.WestTown

    Public Property NorthEastTown As ILocation Implements IInitializationContext.NorthEastTown

    Public Property NorthTown As ILocation Implements IInitializationContext.NorthTown

    Public Property NorthWestTown As ILocation Implements IInitializationContext.NorthWestTown

#If DEBUG Then
    Public Property PortalDestination As ILocation Implements IInitializationContext.PortalDestination
#End If

    Friend Shared Function Create(chosenName As String) As IInitializationContext
        Return New InitializationContext(chosenName)
    End Function
End Class
