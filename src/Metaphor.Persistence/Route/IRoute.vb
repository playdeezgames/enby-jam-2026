Public Delegate Sub RouteInitializer(route As IRoute)
Public Interface IRoute
    Inherits IMetaphorEntity
    ReadOnly Property RouteId As Guid
    ReadOnly Property Direction As String
    ReadOnly Property Destination As ILocation
    ReadOnly Property RouteType As String
End Interface
