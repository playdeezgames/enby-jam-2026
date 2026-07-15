Imports Metaphor.Provision

Friend Class Route
    Inherits KJEntity(Of RouteData)
    Implements IRoute

    Private Sub New(world As IWorld, data As WorldData, direction As String, routeId As Guid)
        MyBase.New(world, data)
        Me.RouteId = routeId
        Me.Direction = direction
    End Sub

    Public ReadOnly Property RouteId As Guid Implements IRoute.RouteId

    Public ReadOnly Property Direction As String Implements IRoute.Direction

    Public ReadOnly Property Destination As ILocation Implements IRoute.Destination
        Get
            Return Location.Create(World, _data, Data.DestinationLocationId)
        End Get
    End Property

    Public ReadOnly Property RouteType As String Implements IRoute.RouteType
        Get
            Return Data.RouteType
        End Get
    End Property

    Protected Overrides ReadOnly Property Data As RouteData
        Get
            Return _data.Routes(RouteId)
        End Get
    End Property

    Public Overrides Sub Remove()
        Throw New NotImplementedException()
    End Sub

    Friend Shared Function Create(world As IWorld, data As WorldData, direction As String, routeId As Guid?) As IRoute
        Return If(routeId.HasValue, New Route(world, data, direction, routeId.Value), Nothing)
    End Function
End Class
