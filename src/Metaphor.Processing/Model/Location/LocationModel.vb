Imports Metaphor.Persistence

Friend Class LocationModel
    Implements ILocationModel

    Private ReadOnly location As ILocation

    Private Sub New(location As ILocation)
        Me.location = location
    End Sub

    Public ReadOnly Property Verbs As IEnumerable(Of IVerbModel) Implements ILocationModel.Verbs
        Get
            Return location.Verbs.Select(Function(x) LocationVerbModel.Create(location, x))
        End Get
    End Property

    Public ReadOnly Property Others As IEnumerable(Of ICharacterModel) Implements ILocationModel.Others
        Get
            Dim avatar = location.World.Avatar
            Return location.GetOtherCharacters(avatar).Select(AddressOf CharacterModel.Create)
        End Get
    End Property

    Friend Shared Function Create(location As ILocation) As ILocationModel
        Return New LocationModel(location)
    End Function
End Class
