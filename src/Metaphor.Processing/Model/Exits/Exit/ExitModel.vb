Imports Metaphor.Persistence

Friend Class ExitModel
    Implements IExitModel

    Private ReadOnly route As IRoute

    Private Sub New(route As IRoute)
        Me.route = route
    End Sub

    Public ReadOnly Property Direction As String Implements IExitModel.Direction
        Get
            Return route.Direction
        End Get
    End Property

    Public ReadOnly Property Name As String Implements IExitModel.Name
        Get
            Return route.GetName()
        End Get
    End Property

    Public Sub AttemptTake() Implements IExitModel.AttemptTake
        Dim world = route.World
        world.ClearMessages()
        Dim character = world.Avatar
        If route.AttemptTake(character) Then
            character.Look()
        End If
    End Sub

    Friend Shared Function Create(route As IRoute) As IExitModel
        Return New ExitModel(route)
    End Function
End Class
