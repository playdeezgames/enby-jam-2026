Imports Metaphor.Persistence

Friend Class InitializationContext
    Implements IInitializationContext
    Private Sub New(chosenName As String)
        Me.ChosenName = chosenName
    End Sub

    Public ReadOnly Property ChosenName As String Implements IInitializationContext.ChosenName

    Public Property Location As ILocation Implements IInitializationContext.Location

    Friend Shared Function Create(chosenName As String) As IInitializationContext
        Return New InitializationContext(chosenName)
    End Function
End Class
